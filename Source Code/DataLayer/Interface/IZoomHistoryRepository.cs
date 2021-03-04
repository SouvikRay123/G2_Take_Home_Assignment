using Models;
using System;
using System.Collections.Generic;

namespace DataLayer
{
    public interface IZoomHistoryRepository
    {
        List<ZoomHistory> Get(DateTime startDate, DateTime endDate);

        void Create(List<ZoomHistory> item);
    }
}
