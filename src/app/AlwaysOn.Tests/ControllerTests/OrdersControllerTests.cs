// ----------------------------------------------------------------------
// <copyright file="OrdersControllerTests.cs" company="Costco Wholesale">
// Copyright (c) Costco Wholesale. All rights reserved.
// </copyright>
// ----------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;
using AlwaysOn.Shared.Interfaces;
using AlwaysOn.Shared.Models;
using Costco.ECom.API.Orders.Controllers;
using Costco.ECom.API.Orders.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AlwaysOn.Tests
{
    public class OrdersControllerTests
    {
        ILogger<OrdersController> mockLogger;

        [SetUp]
        public void Setup()
        {
            mockLogger = new Mock<ILogger<OrdersController>>().Object;
        }

        [Test]
        public async Task PlaceOrder_Returns_Response()
        {
            var id = Guid.NewGuid();
            var order = new OrderItem() { Id = id };
            // Arrange
            var mockDatabase = new Mock<IOrdersService>();
            mockDatabase.Setup(db => db.AddNewOrderItemAsync(order))
                        .Returns(new Task<bool>(() => false));
            var mockMessageProducerService = new Mock<IMessageProducerService>();
            mockMessageProducerService.Setup(db => db.SendSingleMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Delay(1));
            var controller = new OrdersController(mockDatabase.Object, mockMessageProducerService.Object, mockLogger);

            // Act
            var result = await controller.PlaceOrderAsync(new OrderItem() { Id = id });

            Assert.AreEqual(result, true);
        }
    }
}