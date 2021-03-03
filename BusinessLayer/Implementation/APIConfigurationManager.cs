using Constants;
using DataLayer;
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
            var configurations = _APIConfigurationRepository.Get(TableConstants.APIConfigurationNameColumn, configurationName);

            if (configurations == null || configurations.Count == 0)
                throw new Exception($"No configurations found for {configurationName}");

            return configurations[0];
        }
    }
}
