using System.Threading.Tasks;

namespace AlwaysOn.Shared.Interfaces
{
    public interface IRedisCacheService<T>
    {
        Task<T> GetItemAsync(string key);
        Task<T> UpdateItemAsync(string key, T item);
        Task<bool> DeleteItemAsync(string key);
    }
}