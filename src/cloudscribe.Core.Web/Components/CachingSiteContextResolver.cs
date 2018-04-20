﻿using cloudscribe.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace cloudscribe.Core.Web.Components
{
    public interface ICachingSiteContextResolver : ISiteContextResolver
    {

    }

    /// <summary>
    /// this implementation is a baby step towards moving away from use of saaskit
    /// </summary>
    public class CachingSiteContextResolver : SiteContextResolver, ICachingSiteContextResolver
    {
        public CachingSiteContextResolver(
            IMemoryCache cache,
            ISiteQueries siteRepository,
            SiteDataProtector dataProtector,
            IOptions<MultiTenantOptions> multiTenantOptions,
            ILogger<CachingSiteContextResolver> logger,
            IOptions<CachingSiteResolverOptions> cachingOptionsAccessor
            ) :base(siteRepository, dataProtector, multiTenantOptions)
        {
            _cache = cache;
            _cachingOptions = cachingOptionsAccessor.Value;
            _log = logger;
        }

        private IMemoryCache _cache;
        private CachingSiteResolverOptions _cachingOptions;
        private ILogger _log;

        private async Task<List<string>> GetAllSiteFoldersFolders()
        {
            var listCacheKey = "folderList";
            if (_cache.Get(listCacheKey) is List<string> result)
            {
                _log.LogDebug("Folder List retrieved from cache with key \"{cacheKey}\".", listCacheKey);
                return result;
            }

            result = await SiteQueries.GetAllSiteFolders();
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_cachingOptions.FolderListCacheDuration);

            _log.LogDebug("Caching folder list with keys \"{cacheKey}\".", listCacheKey);
            _cache.Set(listCacheKey, result, cacheEntryOptions);

            return result;

        }

        private async Task<string> GetCacheKey(string hostName,string pathStartingSegment)
        {
            if (MultiTenantOptions.Mode == MultiTenantMode.FolderName)
            {
                var folders = await GetAllSiteFoldersFolders();

                return folders.Contains(pathStartingSegment) ? pathStartingSegment : "root";

            }

            return hostName.ToLowerInvariant();
        }

        public override async Task<SiteContext> ResolveSite(
            string hostName,
            string pathStartingSegment,
            CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            var cacheKey = await GetCacheKey(hostName, pathStartingSegment);
            var result = (SiteContext)_cache.Get(cacheKey);
            if(result == null)
            {
                result = await base.ResolveSite(hostName, pathStartingSegment, cancellationToken);
                if(result != null)
                {
                    _cache.Set(
                        cacheKey,
                        result,
                        new MemoryCacheEntryOptions()
                         .SetAbsoluteExpiration(_cachingOptions.SiteCacheDuration)
                         );
                }
            }


            return result;

        }

        public override async Task<SiteContext> GetById(Guid siteId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = "site-" + siteId.ToString();

            var result = (SiteContext)_cache.Get(cacheKey);
            if (result == null)
            {
                result = await base.GetById(siteId, cancellationToken);
                if (result != null)
                {
                    _cache.Set(
                        cacheKey,
                        result,
                        new MemoryCacheEntryOptions()
                         .SetAbsoluteExpiration(_cachingOptions.SiteCacheDuration)
                         );
                }
            }


            return result;
        }


    }
}
