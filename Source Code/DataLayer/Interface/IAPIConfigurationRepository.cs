using Models;
using System.Collections.Generic;

namespace DataLayer
{
    public interface IAPIConfigurationRepository
    {
        List<APIConfiguration> Get(string itemIdentifierName, string identifierValue);
    }
}
