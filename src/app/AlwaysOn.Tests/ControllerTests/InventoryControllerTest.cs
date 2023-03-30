// ----------------------------------------------------------------------
// <copyright file="InventoryControllerTests.cs" company="Costco Wholesale">
// Copyright (c) Costco Wholesale. All rights reserved.
// </copyright>
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AlwaysOn.Shared.Exceptions;
using AlwaysOn.Shared.Interfaces;
using AlwaysOn.Shared.Models;
using Costco.ECom.API.InventoryAvailability.Controllers;
using Costco.ECom.API.InventoryAvailability.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace AlwaysOn.Tests
{
    public class InventoryControllerTests
    {
        ILogger<InventoryController> mockLogger;

        [SetUp]
        public void Setup()
        {
            mockLogger = new Mock<ILogger<InventoryController>>().Object;
        }

        [Test]
        public async Task ListInventoryItems_Returns_InventoryItems()
        {
            // Arrange
            var mockDatabase = new Mock<IInventoryService>();
            mockDatabase.Setup(db => db.ListInventoryItemsAsync(100))
                        .ReturnsAsync(GetTestInventoryItems());
            var redismockDatabase = new Mock<IRedisCacheService<InventoryItem>>();
            var mockTelemetryClient = new TelemetryClient();
            var inMemorySettings = new Dictionary<string, string> {
    {"REDIS_HOST_NAME", "REDIS_HOST_NAME"},
    {"REDIS_PORT_NUMBER", "10000"},
                {"REDIS_KEY", "samplekeyfortest" }
};

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var controller = new InventoryController(mockLogger, mockDatabase.Object, redismockDatabase.Object, null, mockTelemetryClient, configuration);

            // Act
            var result = await controller.ListInventoryItemsAsync();

            // Assert
            Assert.IsInstanceOf<ActionResult<IEnumerable<InventoryItem>>>(result); // expecting list of InventoryItems
            Assert.IsInstanceOf<OkObjectResult>(result.Result); // expecting HTTP 200 result
        }

        [Test]
        public async Task ListInventoryItems_DatabaseUnavailable_Returns_InternalServerError()
        {
            // Arrange
            var mockDatabase = new Mock<IInventoryService>();
            mockDatabase.Setup(db => db.ListInventoryItemsAsync(100))
                        .Throws(new AlwaysOnDependencyException(HttpStatusCode.ServiceUnavailable));
            var redismockDatabase = new Mock<IRedisCacheService<InventoryItem>>();

            var mockTelemetryClient = new TelemetryClient();
            var inMemorySettings = new Dictionary<string, string> {
    {"REDIS_HOST_NAME", "REDIS_HOST_NAME"},
    {"REDIS_PORT_NUMBER", "10000"},
                {"REDIS_KEY", "samplekeyfortest" }
};

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var controller = new InventoryController(mockLogger, mockDatabase.Object, redismockDatabase.Object, null, mockTelemetryClient, configuration);

            // Act
            var result = await controller.ListInventoryItemsAsync();

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, ((ObjectResult)result.Result).StatusCode);
        }

        [Test]
        public async Task ListInventoryItems_TooManyRequests_Returns_ServiceUnavailable()
        {
            // Arrange
            var mockDatabase = new Mock<IInventoryService>();
            mockDatabase.Setup(db => db.ListInventoryItemsAsync(100))
                        .Throws(new AlwaysOnDependencyException(HttpStatusCode.TooManyRequests));

            var redismockDatabase = new Mock<IRedisCacheService<InventoryItem>>();

            var mockTelemetryClient = new TelemetryClient();

            var inMemorySettings = new Dictionary<string, string> {
    {"REDIS_HOST_NAME", "REDIS_HOST_NAME"},
    {"REDIS_PORT_NUMBER", "10000"},
                {"REDIS_KEY", "samplekeyfortest" }
};

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var controller = new InventoryController(mockLogger, mockDatabase.Object, redismockDatabase.Object, null, mockTelemetryClient, configuration);

            // Act
            var result = await controller.ListInventoryItemsAsync();

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, ((ObjectResult)result.Result).StatusCode);
        }


        private List<InventoryItem> GetTestInventoryItems()
        {
            return new List<InventoryItem>()
            {
                new InventoryItem()
                {
                    Id = Guid.NewGuid(),
                    Desc= "First test item",
                    Name = "First Item",
                    Price = 220,
                    QtyBeginning = 450,
                    QtyOnHand = 270
                },
                new InventoryItem() {
                    Id = Guid.NewGuid(),
                    Desc= "Second test item",
                    Name = "Second Item",
                    Price = 100,
                    QtyBeginning = 1000,
                    QtyOnHand = 790

                }
            };

        }

    }
}