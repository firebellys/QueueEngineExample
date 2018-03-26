using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebScraper;
using WebScraper.Controllers;

namespace WebScraper.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Web Scraper API Platform", result.ViewBag.Title);
        }
    }
}
