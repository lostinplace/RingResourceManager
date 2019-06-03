using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RingResources;

namespace RingResourceManagerTests
{
  [TestClass]
  public class ResourceManagerTests
  {
    [TestMethod]
    public void TestShortListConstruction()
    {
      var t = RingResourceManager.FromShortBoundaries(10, 250);
      var actuals = Enumerable.Repeat(0, 200).Select(_ => t.CheckOutResource());
      var expecteds = Enumerable.Range(10, 200).Select(_ => (short)_);
      var tests = expecteds.Zip(actuals, (x,y)=>(x,y));
      foreach (var item in tests)
      {
        var (expected, actual) = item;
        Assert.AreEqual(expected, actual);
      }
    }

    [TestMethod]
    public void TestResourceExhaustion()
    {
      var t = RingResourceManager.FromShortBoundaries(10, 13);
      try
      {
        var actuals = new List<short>()
        {
          t.CheckOutResource(),
          t.CheckOutResource(),
          t.CheckOutResource(),
          t.CheckOutResource(),
          t.CheckOutResource(),
        };
        Assert.Fail("no exception thrown");
      }
      catch (InvalidOperationException ex)
      {
        Assert.IsTrue(true, "correct exception was thrown");
      }catch(Exception ex)
      {
        Assert.Fail("Incorrect exception thrown");
      }
    }

    [TestMethod]
    public void TestCheckoutsAfterCheckin()
    {
      var t = RingResourceManager.FromShortBoundaries(10, 13);
      var expecteds = new List<short>()
      {
        10,11,12,13,10,11
      };
      var actuals = new List<short>()
      {
        t.CheckOutResource(),
        t.CheckOutResource(),
        t.CheckOutResource(),
        t.CheckOutResource(),
      };
      t.CheckInResource(10);
      t.CheckInResource(11);
      actuals.Add(t.CheckOutResource());
      actuals.Add(t.CheckOutResource());

      var tests = expecteds.Zip(actuals, (x, y) => (x, y));
      foreach (var item in tests)
      {
        var (expected, actual) = item;
        Assert.AreEqual(expected, actual);
      }
    }

    [TestMethod]
    public void TestCheckoutsAfterExpiration()
    {
      var t = RingResourceManager.FromShortBoundaries(10, 13);
      var expecteds = new List<short>()
      {
        10,11,12,13,10,11
      };
      var actuals = new List<short>()
      {
        t.CheckOutResource(100),
        t.CheckOutResource(100),
        t.CheckOutResource(100),
        t.CheckOutResource(100),
      };
      Thread.Sleep(100);
      actuals.Add(t.CheckOutResource());
      actuals.Add(t.CheckOutResource());

      var tests = expecteds.Zip(actuals, (x, y) => (x, y));
      foreach (var item in tests)
      {
        var (expected, actual) = item;
        Assert.AreEqual(expected, actual);
      }
    }
  }
}
