using Models;
using System;

namespace DataLayer
{
    public interface IReportRepository
    {
        string GetReportId(string type, DateTime startDate, DateTime endDate);

        void Create(Report item);

        void Update(Report item);
    }
}
