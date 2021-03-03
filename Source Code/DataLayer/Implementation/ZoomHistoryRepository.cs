using Models;
using System.Collections.Generic;

namespace DataLayer
{
    public class ZoomHistoryRepository : IZoomHistoryRepository
    {
        public void Create(ZoomHistory item)
        {
            throw new System.NotImplementedException();
        }

        public List<ZoomHistory> Get(string itemIdentifierName, string identifierValue)
        {
            throw new System.NotImplementedException();
        }

        public void Update(ZoomHistory item)
        {
            throw new System.InvalidOperationException("Not allowed to delete Zoom history");
        }

        public void Delete(string itemIdentifier)
        {
            throw new System.InvalidOperationException("Not allowed to delete Zoom history");
        }
    }
}
