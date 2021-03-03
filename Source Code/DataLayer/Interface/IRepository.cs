using System.Collections.Generic;

namespace DataLayer
{
    public interface IRepository<T>
    {
        List<T> Get(string itemIdentifierName, string identifierValue);

        void Create(T item);

        void Update(T item);

        void Delete(string itemIdentifier);
    }
}
