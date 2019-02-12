using Apache.Geode.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GemFire
{
    public class GemFireCache : IDistributedCache
    {
        private readonly PoolFactory _poolFactory;
        private readonly Cache _cache;
        private ILogger<GemFireCache> _logger;
        private static IRegion<string, string> _cacheRegion;
        private string _regionName = "IDistributedCacheRegion";
        private readonly SemaphoreSlim _connectLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public GemFireCache(PoolFactory poolFactory, Cache cache, ILogger<GemFireCache> logger = null)
        {
            _poolFactory = poolFactory ?? throw new ArgumentNullException(nameof(poolFactory));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger;
        }

        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            try
            {
                return Encoding.ASCII.GetBytes(_cacheRegion[key]);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            try
            {
                var cacheData = _cacheRegion[key];
                return Task.Run(() => Encoding.ASCII.GetBytes(cacheData));
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            throw new NotImplementedException();
        }

        public Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            _logger.LogError("Called RefreshAsync, which hasn't been implemented");
            return Task.FromResult(0);
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            _cacheRegion.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            return Task.Run(() => _cacheRegion.Remove(key));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Connect();

            // NOTE: DistributedCacheEntryOptions includes info like when this item should expire... not sure how to make use of it from here
            var stringVal = Encoding.ASCII.GetString(value);
            _cacheRegion[key] = stringVal;
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            var stringVal = Encoding.ASCII.GetString(value);
            return Task.Run(() => _cacheRegion[key] = stringVal);
        }

        private void Connect()
        {
            if (_cacheRegion != null)
            {
                return;
            }

            _connectLock.Wait();
            try
            {
                _poolFactory.Create("IDistributedCachePool");
                // create/get region
                var regionFactory = _cache.CreateRegionFactory(RegionShortcut.PROXY).SetPoolName("pool");
                try
                {
                    _logger?.LogTrace("Create CacheRegion");
                    _cacheRegion = regionFactory.Create<string, string>(_regionName);
                    _logger?.LogTrace("CacheRegion created");
                }
                catch (Exception e)
                {
                    _logger?.LogInformation(e, "Create CacheRegion failed... now trying to get the region");
                    _cacheRegion = _cache.GetRegion<string, string>(_regionName);
                }
            }
            finally
            {
                _connectLock.Release();
            }
        }
    }
}
