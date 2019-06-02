# Introduction

This is a library I built to manage expirable resources for a mod API that is being deprecated.

It allows you to manage a bunch of resources in a ring-like structure, meaning that once you get to the end of the list, it will start looking for available resources from the beginning of the list.  A resource is available if:
* It is managed by the Resource manager
* It has never been checked out
* It was checked out, but it has been long enough that the ttl for that checkout has expired
* it was checked out, but then checked back in

As part of the checkout call, yo may specify a maximum search depth, if that depth is exceeded while looking for an available resource then an `InvalidOperationException` will be thrown

# Example
```csharp
var t = RingResourceManager.FromShortBoundaries(10, 250);
var actuals = Enumerable.Repeat(0, 200).Select(_ => t.CheckOutResource());
var expecteds = Enumerable.Range(10, 200).Select(_ => (short)_);
var tests = expecteds.Zip(actuals, (x,y)=>(x,y));
foreach (var item in tests)
{
 var (expected, actual) = item;
 Assert.AreEqual(expected, actual);
}

```
