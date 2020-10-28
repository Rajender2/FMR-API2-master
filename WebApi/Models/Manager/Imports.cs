using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;

namespace WebApi.Models
{

    public class CsvCandidateMapping : CsvMapping<ImportCandidate>
    {
        public CsvCandidateMapping()
            : base()
        {
            MapProperty(0, x => x.Name);
            MapProperty(1, x => x.Email);
            MapProperty(2, x => x.Position);
            MapProperty(3, x => x.Education);
            MapProperty(4, x => x.Experience);
            MapProperty(5, x => x.Skills);
            MapProperty(6, x => x.DOB);
            MapProperty(7, x => x.Twitter);
            MapProperty(8, x => x.LinkedIn);
            MapProperty(9, x => x.Phone);
            MapProperty(10, x => x.Address);
            MapProperty(11, x => x.City);
            MapProperty(12, x => x.ZipCode);
        }
    }

    public class ImportCandidate
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Position { get; set; }
            public string Education { get; set; }
            public string Experience { get; set; }
            public string Skills { get; set; }
            public DateTime? DOB { get; set; }
            public string Twitter { get; set; }
            public string LinkedIn { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
        }
    public class CsvQuestionMapping : CsvMapping<ImportQuestion>
    {
        public CsvQuestionMapping()
            : base()
        {
            MapProperty(0, x => x.Question);
            MapProperty(1, x => x.Duration);
            MapProperty(2, x => x.Description);
            MapProperty(3, x => x.Type);
        }
    }
    public class ImportQuestion
        {
             public string Question { get; set; }
             public int Duration { get; set; }
             public int BufferTime { get; set; }
             public string Description { get; set; }
             public string Type { get; set; }
        }

    public class CsvJobOrderMapping : CsvMapping<ImportJoborder>
    {
        public CsvJobOrderMapping()
            : base()
        {
            MapProperty(0, x => x.Title);
            MapProperty(1, x => x.JobType);
            MapProperty(2, x => x.Skills);
            MapProperty(3, x => x.Experience);
            MapProperty(4, x => x.Openings);
            MapProperty(5, x => x.Location);
            MapProperty(6, x => x.Summary);
            MapProperty(7, x => x.RecruiterEmail);
        }
    }
    public class ImportJoborder
    {
        public string Title { get; set; }
        public string JobType { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }
        public int Openings { get; set; }
        public string Location { get; set; }
        public string Summary { get; set; }
        public string RecruiterEmail { get; set; }
    }
}
