using AlwaysOn.Shared.Models.DataTransfer;
using Newtonsoft.Json;

namespace Costco.ECom.API.InventoryAvailability.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("inventoryservice/api/{version:apiVersion}/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly IInventoryService _databaseService;
        private readonly SysConfiguration _sysConfig;
        private readonly IRedisCacheService<InventoryItem> _redisCacheService;
        private readonly TelemetryClient _telemetryClient;
        private readonly RestClient _restClient;
        private readonly IConfiguration _config;

        public InventoryController(ILogger<InventoryController> logger,
            IInventoryService databaseService,
           IRedisCacheService<InventoryItem> redisCacheService,
        SysConfiguration sysConfig, TelemetryClient tc, IConfiguration config)
        {
            _logger = logger;
            _databaseService = databaseService;
            _redisCacheService = redisCacheService;
            _sysConfig = sysConfig;
            _telemetryClient = tc;
            _restClient = new RestClient();
            _config = config;
        }


        /// <summary>
        /// Retrieves N number of InventoryItems, N defaults to 100
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet(Name = nameof(ListInventoryItemsAsync))]
        [ProducesResponseType(typeof(IEnumerable<InventoryItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(InventoryItem), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> ListInventoryItemsAsync(int limit = 100)
        {
            _logger.LogDebug("Received request to get InventoryItem upto limit", limit);
            try
            {
                var res = await _databaseService.ListInventoryItemsAsync(limit);
                return res == null || res.Count() == 0 ?  NotFound() : Ok(res);
            }
            catch (AlwaysOnDependencyException e)
            {
                _logger.LogError(e, "AlwaysOnDependencyException on querying database, StatusCode={statusCode}", e.StatusCode);
                int responseStatusCode = e.StatusCode == HttpStatusCode.TooManyRequests ? (int)HttpStatusCode.ServiceUnavailable : (int)HttpStatusCode.InternalServerError;
                return StatusCode(responseStatusCode, $"Error in processing. Correlation ID: {Activity.Current?.RootId}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception on querying database");
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Error in processing. Correlation ID: {Activity.Current?.RootId}.");
            }
        }

        /// <summary>
        /// Gets an InventoryItem by ID
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [HttpGet("{itemId:guid}", Name = nameof(GetInventoryItemByIdAsync))]
        [ProducesResponseType(typeof(InventoryItem), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(InventoryItem), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<InventoryItem>> GetInventoryItemByIdAsync(Guid itemId)
        {
            _logger.LogDebug("Received request to get InventoryItem {InventoryItem}", itemId);

            try
            {
                var connectionString = _config["REDIS_HOST_NAME"] + ":" + _config["REDIS_PORT_NUMBER"] + ",password=" + _config["REDIS_KEY"] + ",ssl=True,abortConnect=False";
                _logger.LogInformation(connectionString);
                _telemetryClient.TrackEvent(new EventTelemetry("redisConnection") { Name = connectionString });
                var cachedItem = await _redisCacheService.GetItemAsync(itemId.ToString());
                if (cachedItem != null) { return cachedItem; }
                var res = await _databaseService.GetInventoryItemByIdAsync(itemId);
                if (res != null)
                {
                    await _redisCacheService.UpdateItemAsync(itemId.ToString(), res);
                }
                _telemetryClient.TrackPageView(itemId.ToString());
                _telemetryClient.TrackMetric(new MetricTelemetry() { Name = itemId.ToString() });
                return res != null ? Ok(res) : NotFound();
            }
            catch (AlwaysOnDependencyException e)
            {
                _logger.LogError(e, "AlwaysOnDependencyException on querying database, StatusCode={statusCode}", e.StatusCode);
                int responseStatusCode = e.StatusCode == HttpStatusCode.TooManyRequests ? (int)HttpStatusCode.ServiceUnavailable : (int)HttpStatusCode.InternalServerError;
                return StatusCode(responseStatusCode, $"Error in processing. Correlation ID: {Activity.Current?.RootId}.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception on querying database");
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Error in processing. Correlation ID: {Activity.Current?.RootId}.");
            }
        }


        /// <summary>
        /// Creates a InventoryItem in the database
        /// </summary>
        /// <param name="itemDto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.Created)]
        [ApiKey]
        public async Task<ActionResult<bool>> CreateNewInventoryItemAsync(InventoryItem itemDto)
        {
            _logger.LogDebug("Received request to create new InventoryItemId={InventoryItemId}", itemDto);
            itemDto.Id = itemDto.Id ?? Guid.NewGuid();
            await _databaseService.AddNewInventoryItemAsync(itemDto);
            return true;
        }

        /// <summary>
        /// Updates a InventoryItem in the database
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemDto"></param>
        /// <returns></returns>
        [HttpPut("{itemId:guid}")]
        [ProducesResponseType(typeof(InventoryItem), (int)HttpStatusCode.Accepted)]
        [ApiKey]
        public async Task<ActionResult<InventoryItem>> UpdateInventoryItemAsync(Guid itemId, InventoryItem itemDto)
        {
            _logger.LogDebug("Received request to update InventoryItemId={InventoryItemId}", itemId);

            var existingItem = await _databaseService.GetInventoryItemByIdAsync(itemId);
            if (existingItem == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            existingItem.Name = itemDto.Name ?? existingItem.Name;

            return await UpsertInventoryItemAsync(itemId, existingItem);
        }

        /// <summary>
        /// Upserts a catatalogItem in the database
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task<ActionResult<InventoryItem>> UpsertInventoryItemAsync(Guid itemId, InventoryItem item)
        {
            try
            {
                await _databaseService.UpsertInventoryItemAsync(item);
                _logger.LogInformation("InventoryItemId={InventoryItemId} was upserted in the database", item.Id);
            }
            catch (AlwaysOnDependencyException e)
            {
                _logger.LogError(e, "AlwaysOnDependencyException on storing InventoryItemId={InventoryItemId}, StatusCode={statusCode}", item.Id, e.StatusCode);
                int responseStatusCode = e.StatusCode == HttpStatusCode.TooManyRequests ? (int)HttpStatusCode.ServiceUnavailable : (int)HttpStatusCode.InternalServerError;

                return StatusCode(responseStatusCode, $"Error in processing. Correlation ID: {Activity.Current?.RootId}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception on storing InventoryItemId={InventoryItemId}", item.Id);
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Error in processing. Correlation ID: {Activity.Current?.RootId}");
            }

            return CreatedAtRoute(nameof(GetInventoryItemByIdAsync), new { itemId = item.Id }, item);
        }

        /// <summary>
        /// Check Connection
        /// </summary>
        /// <returns>RestResponse</returns>
        [HttpPost("CheckConnection", Name = nameof(CheckConnectionAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> CheckConnectionAsync(ConnectionDto connectionDto)
        {
            try
            {
                var request = new RestRequest(connectionDto.ConnectionUrl, Method.Get);
                var response = await _restClient.GetAsync(request);

                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception on querying database");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    e.Message,
                    e.StackTrace,
                    e.Source
                });
            }
        }

        /// <summary>
        /// Self Pulse
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpGet("SelfPulse", Name = nameof(SelfPulseAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult SelfPulseAsync()
        {
            try
            {
                return Ok(new
                {
                    Message = "Self connection is working"
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception on querying database");
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Error in processing. Correlation ID: {Activity.Current?.RootId}");
            }
        }
    }
}
