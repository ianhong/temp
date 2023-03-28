using AlwaysOn.Shared.Interfaces;
using AlwaysOn.Shared.Exceptions;
using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AlwaysOn.Shared.Services
{
    public class ServiceBusProducerService : IMessageProducerService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _senderClient;
        private readonly ILogger<ServiceBusProducerService> _logger;

        public ServiceBusProducerService(ILogger<ServiceBusProducerService> logger, SysConfiguration sysConfig)
        {
            _logger = logger;
            _serviceBusClient = new ServiceBusClient(sysConfig.ServicebusConnection); ;
            _senderClient = _serviceBusClient.CreateSender(sysConfig.ServicebusQueueName);
        }

        /// <summary>
        /// Very simple health check. Attempts to send an empty message
        /// Adds a property "HEALTHCHECK=TRUE" to the message
        /// </summary>
        /// <returns></returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Event Hub health probe requested");
            if (_serviceBusClient.IsClosed)
            {
                _logger.LogError($"Unexpected 'Closed' status of Service Bus in {nameof(CheckHealthAsync)}");
                return new HealthCheckResult(HealthStatus.Unhealthy);
            }

            try
            {
                var message = new ServiceBusMessage("{}");
                message.ApplicationProperties.Add("HEALTHCHECK", "TRUE");
                await _senderClient.SendMessageAsync(message, cancellationToken);
                return new HealthCheckResult(HealthStatus.Healthy);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception on sending health probe message to Service Bus");
                return new HealthCheckResult(HealthStatus.Unhealthy, exception: e);
            }
        }

        public async Task SendMessageBatchAsync(IEnumerable<(string messageBody, string action)> messages, CancellationToken cancellationToken = default)
        {
            try
            {
                using ServiceBusMessageBatch messageBatch = await _senderClient.CreateMessageBatchAsync(cancellationToken);
                foreach (var message in messages)
                {
                    messageBatch.TryAddMessage(new ServiceBusMessage(messages.ToString()));
                }
                await _senderClient.SendMessagesAsync(messageBatch, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception on sending message batch to Service Bus");
                throw new AlwaysOnDependencyException(HttpStatusCode.InternalServerError, innerException: e);
            }
        }
        public async Task SendSingleMessageAsync(string messageBody, string action = null, CancellationToken cancellationToken = default)
        {
            try
            {
                dynamic creationEvent = new
                {
                    action = action,
                    data = messageBody
                };
                string data = JsonConvert.SerializeObject(creationEvent);
                ServiceBusMessage msg = new ServiceBusMessage(data);
                await _senderClient.SendMessageAsync(msg, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception on sending message to Service Bus");
                throw new AlwaysOnDependencyException(HttpStatusCode.InternalServerError, innerException: e);
            }
        }
    }
}

