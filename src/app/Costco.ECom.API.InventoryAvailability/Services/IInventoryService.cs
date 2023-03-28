namespace Costco.ECom.API.InventoryAvailability.Services
{
    public interface IInventoryService : IDatabaseService
        {
            /// <summary>
            /// Get a specific InventoryItem by its Id
            /// </summary>
            /// <param name="itemId"></param>
            /// <returns></returns>
            Task<InventoryItem?> GetInventoryItemByIdAsync(Guid itemId);

            /// <summary>
            /// Writes a new InventoryItem to the database
            /// </summary>
            /// <param name="InventoryItem"></param>
            /// <returns></returns>
            Task AddNewInventoryItemAsync(InventoryItem InventoryItem);

            /// <summary>
            /// Fetches N number of InventoryItem
            /// </summary>
            /// <param name="limit"></param>
            /// <returns></returns>
            Task<IEnumerable<InventoryItem>> ListInventoryItemsAsync(int limit);

            /// <summary>
            /// Upserts InventoryItem
            /// </summary>
            /// <param name="item">Full InventoryItem object to be updated</param>
            /// <returns></returns>
            Task UpsertInventoryItemAsync(InventoryItem item);
        }
}
