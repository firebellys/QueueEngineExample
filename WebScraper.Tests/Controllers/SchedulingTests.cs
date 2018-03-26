using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SchedulingService;
using Newtonsoft.Json;
using SchedulingService.Models;

namespace WebScraper.Tests.Controllers
{
    [TestClass]
    public class SchedulingTests
    {
        [TestMethod]
        public void TestGetById()
        {
            // Act
            var postResult = SchedulingServiceAgent.QueueRequest("http://www.google.com");
            var getResult = SchedulingServiceAgent.GetStatusById(JsonConvert.DeserializeObject<Response>(postResult).Id);

            // Assert
            Assert.IsNotNull(getResult);
        }
        [TestMethod]
        public void TestQueue()
        {
            // Act
            var postResult = SchedulingServiceAgent.QueueRequest("http://www.google.com");

            // Assert
            Assert.IsNotNull(postResult);
        }
    }
}
