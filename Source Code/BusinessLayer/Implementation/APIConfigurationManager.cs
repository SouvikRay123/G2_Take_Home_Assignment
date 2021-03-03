using Constants;
using DataLayer;
using Helper;
using Models;
using System;

namespace BusinessLayer
{
    public class APIConfigurationManager : IAPIConfigurationManager
    {
        IAPIConfigurationRepository _APIConfigurationRepository;

        public APIConfigurationManager(IAPIConfigurationRepository apiConfigurationRepository)
        {
            _APIConfigurationRepository = apiConfigurationRepository;
        }

        public APIConfiguration Get(string configurationName)
        {
            string cacheKey = $"APIConfiguration_{TableConstants.APIConfigurationNameColumn}_{configurationName}";
            APIConfiguration configuration;

            configuration = ApplicationCache.RetrieveFromCache<APIConfiguration>(cacheKey);

            if (configuration != null)
                return configuration;
            
            var dataStoreResult = _APIConfigurationRepository.Get(TableConstants.APIConfigurationNameColumn, configurationName);

            if (dataStoreResult == null || dataStoreResult.Count == 0)
                throw new Exception($"No configurations found for {configurationName}");

            ApplicationCache.AddToCache<APIConfiguration>(cacheKey, dataStoreResult[0]);

            return dataStoreResult[0];
        }
    }
}
