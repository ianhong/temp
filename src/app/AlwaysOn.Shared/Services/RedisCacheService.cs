// <copyright file="RedisCacheService.cs" company="Costco Wholesale">
// Copyright (c) Costco Wholesale. All rights reserved.
// </copyright>
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AlwaysOn.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AlwaysOn.Shared.Services
{
    public class RedisCacheService<T> : IRedisCacheService<T> where T : class
    {
        private readonly ILogger<RedisCacheService<T>> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisCacheService(ILoggerFactory loggerFactory, ConnectionMultiplexer redis)
        {
            _logger = loggerFactory.CreateLogger<RedisCacheService<T>>();
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task<bool> DeleteItemAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public IEnumerable<string> GetUsers()
        {
            var server = GetServer();
            var data = server.Keys();

            return data?.Select(k => k.ToString());
        }

        public async Task<T> GetItemAsync(string key)
        {
            var data = await _database.StringGetAsync(key);

            if (data.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<T> UpdateItemAsync(string key, T item)
        {
            var created = await _database.StringSetAsync(key, JsonSerializer.Serialize(item));

            if (!created)
            {
                _logger.LogInformation("Problem occur persisting the item.");
                return null;
            }

            _logger.LogInformation("inventoryItem item persisted succesfully.");

            return await GetItemAsync(key);
        }

        private IServer GetServer()
        {
            var endpoint = _redis.GetEndPoints();
            return _redis.GetServer(endpoint.First());
        }
    }
}
