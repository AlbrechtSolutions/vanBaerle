using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace LiveIntegration.Test
{
  [TestClass]
  public class SynchronizeHandlerTests
  {
    private const string RequestUrl = "http://integration.local.dynamicweb.pt/public/synchronize.ashx";

    [TestMethod]
    public void RequestWithoutParameterReturnsNoWorkPerformed()
    {
      using (WebClient client = new WebClient())
      {
        string s = client.DownloadString(RequestUrl);

        Assert.IsTrue(s.ToLower().Contains("no work to perform"));
      }
    }

    [TestMethod]
    public void InvalidRequestReturnsNoWorkPerformed()
    {
      string postData = "orders=<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<clients></clients>";

      using (WebClient client = new WebClient())
      {
        client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

        string response = client.UploadString(RequestUrl, postData);

        Assert.IsTrue(response.ToLower().Contains("no work to perform"));
      }
    }

    [TestMethod]
    public void ValidRequestWithInvalidOrderReturnsWarning()
    {
      string postData = "orders=<?xml version=\"1.0\" encoding=\"UTF-8\"?><Orders><Order><OrderID>123</OrderID><name>Marco</name></Order><Order><OrderID>456</OrderID><name>Santos</name></Order></Orders>";

      using (WebClient client = new WebClient())
      {
        client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

        string response = client.UploadString(RequestUrl, postData);

        Assert.IsTrue(response.ToLower().Contains("invalid order id"));
      }
    }

    [TestMethod]
    public void ValidRequestWithInvalidOrderStateReturnsWarning()
    {
      string postData = "orders=<?xml version=\"1.0\" encoding=\"UTF-8\"?><Orders><Order><OrderID>ORDER10</OrderID><OrderStateID>Marco</OrderStateID></Order></Orders>";

      using (WebClient client = new WebClient())
      {
        client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

        string response = client.UploadString(RequestUrl, postData);

        Assert.IsTrue(response.ToLower().Contains("order state id is invalid"));
      }
    }

    [TestMethod]
    public void ValidRequestWithInvalidPropertyReturnsWarning()
    {
      string postData = "orders=<?xml version=\"1.0\" encoding=\"UTF-8\"?><Orders><Order><OrderID>ORDER10</OrderID><Chong>Marco</Chong></Order></Orders>";

      using (WebClient client = new WebClient())
      {
        client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

        string response = client.UploadString(RequestUrl, postData);

        Assert.IsTrue(response.ToLower().Contains("unable to update property"));
      }
    }

    [TestMethod]
    public void ValidRequestWithValidOrderReturnsAllOrdersSynced()
    {
      string postData = "orders=<?xml version=\"1.0\" encoding=\"UTF-8\"?><Orders><Order><OrderID>ORDER10</OrderID><OrderPriceBeforeFeesWithVAT>3.25</OrderPriceBeforeFeesWithVAT></Order><Order><OrderID>ORDER15</OrderID><OrderPriceWithVAT>11.5</OrderPriceWithVAT></Order></Orders>";

      using (WebClient client = new WebClient())
      {
        client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

        string response = client.UploadString(RequestUrl, postData);

        Assert.IsTrue(response.ToLower().Contains("all orders synced"));
      }
    }
  }
}