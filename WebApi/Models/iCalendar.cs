using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class iCalendar
    {
        public int Id { get; set; }
        public DateTime EventStartDateTime { get; set; }
        public DateTime EventEndDateTime { get; set; }
        public DateTime EventTimeStamp { get; set; }
        public DateTime EventCreatedDateTime { get; set; }
        public DateTime EventLastModifiedTimeStamp { get; set; }
        public string UID { get; set; }
        public string EventDescription { get; set; }
        public string EventLocation { get; set; }
        public string EventSummary { get; set; }
        public string AlarmTrigger { get; set; }
        public string AlarmRepeat { get; set; }
        public string AlarmDuration { get; set; }
        public string AlarmDescription { get; set; }
        public iCalendar()
        {
            EventTimeStamp = DateTime.Now;
            EventCreatedDateTime = EventTimeStamp;
            EventLastModifiedTimeStamp = EventTimeStamp;
        }
    }
}
