using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp.Infrastructure;

namespace Test.RssdpPortable
{
  [TestClass]
  public class DisposableManagedObjectTests
  {

    [TestMethod]
    public void DisposableManagedObject_DisposeSetsIsDisposed()
    {
      var testObject = new MockDisposableObject();
      Assert.IsFalse(testObject.IsDisposed);
      testObject.Dispose();
      Assert.IsTrue(testObject.IsDisposed);
    }

    [ExpectedException(typeof(System.ObjectDisposedException))]
    [TestMethod]
    public void DisposableManagedObject_ThrowIfDisposedDoesWhatItSaysOnTheTin()
    {
      var testObject = new MockDisposableObject();
      var result = testObject.TestMethod();
      Assert.AreEqual("oops", result);
      testObject.Dispose();
      result = testObject.TestMethod();
    }

  }

  public class MockDisposableObject : DisposableManagedObjectBase
  {
    public string TestMethod()
    {
      base.ThrowIfDisposed();

      return "oops";
    }
  }
}