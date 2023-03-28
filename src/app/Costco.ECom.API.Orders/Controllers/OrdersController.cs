namespace Costco.ECom.API.Orders.Controllers
{
     
    [ApiController]
        [ApiVersion("1.0")]
        [Route("orderservice/api/{version:apiVersion}/[controller]")]
        public class OrdersController : ControllerBase
        {
            private readonly ILogger<OrdersController> _logger;
            private readonly IOrdersService _databaseService;
            private readonly IMessageProducerService _messageProducerService;
            public OrdersController(IOrdersService databaseService,
               IMessageProducerService messageProducerService,
               ILogger<OrdersController> logger)
            {
                _databaseService = databaseService;
                _messageProducerService = messageProducerService;
                _logger = logger;
            }

            [HttpPost(Name = "PlaceOrder")]
            [ProducesResponseType(typeof(bool), (int)HttpStatusCode.Created)]
            public async Task<bool> PlaceOrderAsync(OrderItem order)
            {
                order.Id = order.Id ?? Guid.NewGuid();
                await _databaseService.AddNewOrderItemAsync(order);
                await _messageProducerService.SendSingleMessageAsync(Helpers.JsonSerialize(order), Constants.UpdateQuantityActionName, default(CancellationToken));
                return true;
            }

        }
    
}
