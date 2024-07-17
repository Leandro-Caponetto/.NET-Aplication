using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Core.Shared.CacheServices
{
    public abstract class BaseCacheService
    {
        private readonly IMemoryCache cache;

        public BaseCacheService(IMemoryCache cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// obtiene un objeto T de la cache. Si dicho elemento no existe, lo busca utilizando la func, luego lo guarda en cache y finalmente lo retorna
        /// </summary>
        /// <typeparam name="T">clase del objeto guardado en cache</typeparam>
        /// <param name="cacheKey">key con la que se guardara el objeto en cache</param>
        /// <param name="expriationTime">una vez alcanzado e expirationTime el objeto se elimina de la cache</param>
        /// <param name="func">funcion a utilizar para obtener el objeto en caso de que no exista en cache</param>
        /// <returns></returns>
        protected T GetAllElementsCache<T>(string cacheKey, TimeSpan expriationTime, Func<T> func)
        {
            T cacheObject;

            if (!cache.TryGetValue(cacheKey, out cacheObject))
            {
                cacheObject = func.Invoke();

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time
                    .SetAbsoluteExpiration(expriationTime);

                // Save data in cache.
                cache.Set(cacheKey, cacheObject, cacheEntryOptions);
            }

            return cacheObject;
        }

        /// <summary>
        /// obtiene un objeto T de la cache. Si dicho elemento no existe, lo busca utilizando la func, luego lo guarda en cache y finalmente lo retorna
        /// </summary>
        /// <typeparam name="T">clase del objeto guardado en cache</typeparam>
        /// <param name="cacheKey">key con la que se guardara el objeto en cache</param>
        /// <param name="expriationTime">una vez alcanzado e expirationTime el objeto se elimina de la cache</param>
        /// <param name="func">funcion a utilizar para obtener el objeto en caso de que no exista en cache</param>
        /// <returns></returns>
        protected async Task<T> GetAllElementsCacheAsync<T>(string cacheKey, TimeSpan expriationTime, Func<T> func)
            => await cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = expriationTime;
                return Task.FromResult(func.Invoke());
            });

        /// <summary>
        /// elimina un objeto de la cache segun su key
        /// </summary>
        /// <param name="key"></param>
        protected void ResetCacheKey(string key)
            => cache.Remove(key);
    }
}
