// ----------------------------------------------------------------------
// <copyright file="OrdersControllerTests.cs" company="Costco Wholesale">
// Copyright (c) Costco Wholesale. All rights reserved.
// </copyright>
// ----------------------------------------------------------------------
using System.Drawing.Text;
using System.Threading.Tasks;
using AlwaysOn.Shared.Models.DataTransfer;
using Bogus;
using Bogus.DataSets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Costco.ECom.API.InventoryAvailability.Services
{
    public class InventoryMockService : IInventoryService
    {
        public async Task AddNewCatalogItemAsync(CatalogItem catalogItem)
        {
            await Task.Run( () => { Thread.Sleep(100); });
        }

        public Task AddNewCommentAsync(ItemComment comment)
        {
            return Task.Run(() => { Thread.Sleep(100); });
        }

        public Task AddNewInventoryItemAsync(InventoryItem InventoryItem)
        {
            return Task.Run(() => { Thread.Sleep(100); });
        }

        public Task AddNewRatingAsync(ItemRating rating)
        {
            return Task.Run(() => { Thread.Sleep(100); });
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task<HealthCheckResult>.FromResult(HealthCheckResult.Healthy());
        }

        public Task DeleteItemAsync<T>(string objectId, string partitionKey = null)
        {
            return Task.Run(() => { Thread.Sleep(100); });
        }

        public Task<InventoryItem?> GetInventoryItemByIdAsync(Guid itemId)
        {
            InventoryItem? mockData = new Faker<InventoryItem>()
                .RuleFor(o => o.Id, f => itemId)
                .RuleFor(o => o.Desc, f => f.Random.Word())
                .RuleFor(o => o.Name, f => f.Random.Word())
                .RuleFor(o => o.Price, f => f.Random.Number(50, 500))
                .RuleFor(o => o.QtyBeginning, f => f.Random.Number(500, 1000))
                .RuleFor(o => o.QtyOnHand, f => f.Random.Number(0, 500));

            return Task.FromResult(mockData);
        }

        public Task<IEnumerable<InventoryItem>> ListInventoryItemsAsync(int limit)
        {
            return Task.FromResult(GetItemsFake(limit));
        }

        public Task UpsertInventoryItemAsync(InventoryItem item)
        {
            return Task.Run(() => { Thread.Sleep(100); });
        }

        public Task<CatalogItem> GetCatalogItemByIdAsync(Guid itemId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CatalogItem>> ListCatalogItemsAsync(int limit)
        {
            throw new NotImplementedException();
        }

        public Task UpsertCatalogItemAsync(CatalogItem item)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ItemComment>> GetCommentsForCatalogItemAsync(Guid itemId, int limit)
        {
            throw new NotImplementedException();
        }

        public Task<ItemRating> GetRatingByIdAsync(Guid ratingId, Guid itemId)
        {
            throw new NotImplementedException();
        }

        public Task<ItemComment> GetCommentByIdAsync(Guid commentId, Guid itemId)
        {
            throw new NotImplementedException();
        }

        public Task<RatingDto> GetAverageRatingForCatalogItemAsync(Guid itemId)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<InventoryItem> GetItemsFake(int limit = 1, Guid itemId = default)
        {
            var mockData = new Faker<InventoryItem>()
                .RuleFor(o => o.Id, f => itemId == default ? Guid.NewGuid() : itemId)
                .RuleFor(o => o.Desc, f => f.Random.Word())
                .RuleFor(o => o.Name, f => f.Random.Word())
                .RuleFor(o => o.Price, f => f.Random.Number(50, 500))
                .RuleFor(o => o.QtyBeginning, f => f.Random.Number(500, 1000))
                .RuleFor(o => o.QtyOnHand, f => f.Random.Number(0, 500));

            return mockData.Generate(limit);
        }
    }
}
