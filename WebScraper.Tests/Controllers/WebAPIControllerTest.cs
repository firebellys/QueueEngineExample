using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebScraper;
using WebScraper.Controllers;
using Newtonsoft.Json;
using SchedulingService.Models;
using Newtonsoft.Json.Linq;

namespace WebScraper.Tests.Controllers
{
    [TestClass]
    public class WebAPIControllerTest
    {
        [TestMethod]
        public void Get()
        {
            // Arrange
            WebAPIController controller = new WebAPIController();

            // Act
            IEnumerable<string> result = controller.Get();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetById()
        {
            // Arrange
            WebAPIController controller = new WebAPIController();

            // Act
            controller.Request = new HttpRequestMessage();
            controller.Request.SetConfiguration(new HttpConfiguration());
            var postResult = controller.Post("http://www.google.com");
            var content = postResult.Content.ReadAsStringAsync().Result.ToString();
            
            // Check for the request just made
            var result = controller.Get(JsonConvert.DeserializeObject<Job>(content).Id);
            
            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [TestMethod]
        public void PostHappyPath()
        {
            // Arrange
            WebAPIController controller = new WebAPIController();

            // Act
            controller.Request = new HttpRequestMessage();
            controller.Request.SetConfiguration(new HttpConfiguration());
            var result = controller.Post("http://www.google.com");

            // Assert 
            Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [TestMethod]
        public void PostNullTest()
        {
            // Arrange
            WebAPIController controller = new WebAPIController();

            // Act
            controller.Request = new HttpRequestMessage();
            controller.Request.SetConfiguration(new HttpConfiguration());
            var result = controller.Post("");

            // Assert 
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod]
        public void PostMalformedUrl()
        {
            // Arrange
            WebAPIController controller = new WebAPIController();

            // Act
            controller.Request = new HttpRequestMessage();
            controller.Request.SetConfiguration(new HttpConfiguration());
            var result = controller.Post("asdaf//634dfsdgf//vb.rt34f4tjy");

            // Assert 
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod]
        public void Delete()
        {
            // Arrange
            WebAPIController controller = new WebAPIController();

            controller.Request = new HttpRequestMessage();
            controller.Request.SetConfiguration(new HttpConfiguration());
            var postResult = controller.Post("http://www.google.com");
            var content = postResult.Content.ReadAsStringAsync().Result.ToString();

            var deleteResult = controller.Delete(content.Replace(@"\", "").Replace("\"", "").Replace(@"/", ""));

            // Assert 
            Assert.AreEqual(System.Net.HttpStatusCode.OK, deleteResult.StatusCode);
        }
    }
}
