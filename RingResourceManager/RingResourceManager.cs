using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RingResources
{
  class RingResourceNode<T>
  {
    public T value
    {
      get;
      set;
    }

    public long expirationTime
    {
      get;
      private set;
    }

    public RingResourceNode<T> next
    {
      get;
      set;
    }

    private bool _isCheckedOut;

    public bool isCheckedOut
    {
      get
      {
        return this._isCheckedOut && DateTimeOffset.Now.ToUnixTimeMilliseconds() < this.expirationTime;
      }
      private set
      {
        _isCheckedOut = value;
      }
    }

    public void checkIn()
    {
      this.isCheckedOut = false;
      this.expirationTime = 0;
    }

    public RingResourceNode<T> checkout(long ttl = 1500, int allowedDepth = 10)
    {
      if (allowedDepth == 0) throw new InvalidOperationException("Max search depth reached"){};

      if(this.isCheckedOut) return next.checkout(ttl, allowedDepth-1);
      
      this.isCheckedOut = true;
      this.expirationTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ttl;
      return this;
    }

    public RingResourceNode(T value){
      this.value = value;
      this.expirationTime = 0;
    }
  }

  public static class RingResourceManager
  {
    public static RingResourceManager<short> FromShortBoundaries(short start, short end)
    {
      var resourceList = Enumerable.Range(start, end - start + 1).Select(x => (short)x).ToList();
      return new RingResourceManager<short>(resourceList);
    }
  }

  public class RingResourceManager<T>
  {
    private List<RingResourceNode<T>> resourceNodes = new List<RingResourceNode<T>>();
    private Dictionary<T, RingResourceNode<T>> resourceLocations = new Dictionary<T, RingResourceNode<T>>();
    private RingResourceNode<T> latest;

    public RingResourceManager(ICollection<T> resources)
    {
      if (resources.Count == 0) return;
      var lastItem = default(RingResourceNode<T>);
      var tmp = default(RingResourceNode<T>);
      for (int i = 0; i < resources.Count; i++)
      {
        var item = resources.ElementAt(i);
        tmp = new RingResourceNode<T>(item);
        if(lastItem != default(RingResourceNode<T>))
        {
          lastItem.next = tmp;
        }
        resourceNodes.Add(tmp);
        resourceLocations[item] = tmp;
        lastItem = tmp;
      }
      tmp.next = this.resourceNodes[0];
      latest = this.resourceNodes[0];
    }

    /// <summary>
    /// Checks out a resource that is managed by the manager
    /// </summary>
    /// <param name="ttl">used to determine checkout expiration time, value in milliseconds</param>
    /// <param name="maxDepth">the maximum number of traversals that will be tried before giving up</param>
    /// <exception cref="InvalidOperationException">thrown when the manager can't find an available resource within the provided traversal limit</exception>
    /// <returns>the resource being managed</returns>
    public T CheckOutResource(long ttl = 5000, int maxDepth = 10)
    {
      var checkedOutNode = latest.checkout(ttl, maxDepth);
      latest = checkedOutNode.next;
      return checkedOutNode.value;
    }

    public void CheckInResource(T resource)
    {
      RingResourceNode<T> tmp;
      var ok = resourceLocations.TryGetValue(resource, out tmp);
      if (!ok) throw new ArgumentOutOfRangeException("Specified resource is not being managed");
      tmp.checkIn();
    }
  }
}
