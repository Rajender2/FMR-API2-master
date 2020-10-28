using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Data
{
    public class DashboardDTO
    {
        public DashboardDTO()
        {
            CandidateResponses = new List<ChartData>();
            JobOrders = new List<ChartData>();
        }
        public int JobOrdersCount { get; set; }
        public int RecruitersCount { get; set; }
        public int InterviewsCount { get; set; }
        public List<ChartData> CandidateResponses { get; set; }
        public List<ChartData> JobOrders { get; set; }
    }

    public class ChartData
    {
        public ChartData()
        {
            Data = new List<int>();
        }
        public string Name { get; set; }
        public List<int> Data { get; set; }
    }
}
