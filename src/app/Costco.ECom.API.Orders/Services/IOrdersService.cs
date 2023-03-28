namespace Costco.ECom.API.Orders.Services
{
    public interface IOrdersService
    {
        /// <summary>
        /// Get a specific OrderItem by its ID
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        Task<OrderItem> GetOrderItemByIdAsync(Guid itemId);

        /// <summary>
        /// Writes a new OrderItem to the database
        /// </summary>
        /// <param name="OrderItem"></param>
        /// <returns></returns>
        Task AddNewOrderItemAsync(OrderItem OrderItem);

        /// <summary>
        /// Fetches N number of OrderItem
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IEnumerable<OrderItem>> ListOrderItemsAsync(int limit);

        /// <summary>
        /// Upserts OrderItem
        /// </summary>
        /// <param name="item">Full OrderItem object to be updated</param>
        /// <returns></returns>
        Task UpsertOrderItemAsync(OrderItem item);
    }
}
