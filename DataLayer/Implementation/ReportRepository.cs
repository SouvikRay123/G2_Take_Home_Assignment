using Models;
using System.Collections.Generic;

namespace DataLayer
{
    public class ReportRepository : IReportRepository
    {
        public void Create(Report item)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string itemIdentifier)
        {
            throw new System.InvalidOperationException("Not allowed to delete report");
        }

        public List<Report> Get(string itemIdentifierName, string identifierValue)
        {
            throw new System.NotImplementedException();
        }

        public void Update(Report item)
        {
            throw new System.NotImplementedException();
        }
    }
}
