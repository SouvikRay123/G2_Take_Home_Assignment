using Models;

namespace BusinessLayer
{
    public interface IAPIConfigurationManager
    {
        APIConfiguration Get(string configurationName);
    }
}
