using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;


namespace WebApi.Services
{
    public interface ICalendarService
    {
        string GetCalendar();
        string GetCalendar(Event Evtcal);
    }
    public class CalendarService : ICalendarService
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        public CalendarService(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        public string toUniversalTime(DateTime dt)
        {
            string DateFormat = "yyyyMMddTHHmmssZ";
            return dt.ToUniversalTime().ToString(DateFormat);
        }
        public string GetCalendar()
        {
            var now = DateTime.Now;
            var estart = now.AddDays(1);
            var eend = now.AddMinutes(30);


            var calendar = new Ical.Net.Calendar();
            IList<Attendee> attendees = new List<Attendee>();

            Organizer orgniser = new Organizer
            {
                CommonName = "Face My Resume",
                Value = new Uri($"mailto:noreply@lsinextgen.com")
            };

            //attendees.Add(new Attendee("MAILTO:sivascsl@gmail.com")
            //{
            //    CommonName = "Siva Shankaran",
            //    Role = ParticipationRole.RequiredParticipant,
            //    ParticipationStatus = ToDoParticipationStatus.Tentative,
            //    Rsvp = true,
            //    Type = "INDIVIDUAL"
            //});

            attendees.Add(new Attendee("MAILTO:rsivas@yahoo.com")
            {
                //Role = ParticipationRole.RequiredParticipant,
                //ParticipationStatus = ToDoParticipationStatus.Tentative,
                //Rsvp = true,
                //Type = "INDIVIDUAL"
            });


            Alarm ealarm = new Alarm();
            ealarm.Action = AlarmAction.Display;
            ealarm.Trigger = new Trigger(TimeSpan.FromMinutes(-30));

            const string eTz = "Eastern Standard Time";
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(eTz);
            var timezone = VTimeZone.FromSystemTimeZone(tzi);
            calendar.AddTimeZone(timezone);
            //  calendar.AddTimeZone(new VTimeZone("America/New_York"));
            calendar.Method = "REQUEST";
            var e = new CalendarEvent
            {
                Start = new CalDateTime(estart),
                End = new CalDateTime(eend),
                LastModified = new CalDateTime(now),
                Location = "",
                Alarms = { ealarm },
                Sequence = 0,
                Summary = "Telephonic Interview",
                Description = "Calendar sent using Face My Resume App",
                Status = "CONFIRMED",
                Attendees = attendees,
                Organizer = orgniser

            };
            calendar.Events.Add(e);

            var e2 = new CalendarEvent
            {
                Start = new CalDateTime(eend),
                End = new CalDateTime(eend),
                LastModified = new CalDateTime(now),
                Location = "",
                Alarms = { ealarm },
                Sequence = 0,
                Summary = "InPerson Interview",
                Description = "Calendar sent using Face My Resume App",
                Status = "CONFIRMED",
                Attendees = attendees,
                Organizer = orgniser
            };
            //calendar.Events.Add(e2);


            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(calendar);

            //return serializedCalendar.Replace("github.com/rianjs/","facemyresume.com/app/").Replace("ical.net", "cal");
            return serializedCalendar.Replace("\n ", "");
        }
        public string GetCalendar(Event evtcal)
        {

            var attendees = evtcal.Participants;
            DateTime dtnow = DateTime.Now;
            string strnow = toUniversalTime(DateTime.Now);
            DateTime stdt = Convert.ToDateTime(evtcal.StartTime);
            DateTime eddt = Convert.ToDateTime(evtcal.EndTime);

            if(eddt == null)
            {
                stdt.AddMinutes(30);
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("METHOD:REQUEST");
            sb.AppendLine("BEGIN:VTIMEZONE");
            sb.AppendLine("TZID:Eastern Standard Time");
            sb.AppendLine("BEGIN:STANDARD");
            sb.AppendLine("DTSTART:" + toUniversalTime(stdt));
            sb.AppendLine("RRULE:FREQ=YEARLY;BYDAY=1SU;BYHOUR=2;BYMINUTE=0;BYMONTH=11");
            sb.AppendLine("TZNAME:Eastern Standard Time");
            sb.AppendLine("TZOFFSETFROM:-0400");
            sb.AppendLine("TZOFFSETTO:-0500");
            sb.AppendLine("END:STANDARD");
            sb.AppendLine("BEGIN:DAYLIGHT");
            sb.AppendLine("DTSTART:" + toUniversalTime(stdt));
            sb.AppendLine("RRULE:FREQ=YEARLY;BYDAY=2SU;BYHOUR=2;BYMINUTE=0;BYMONTH=3");
            sb.AppendLine("TZNAME:Eastern Daylight Time");
            sb.AppendLine("TZOFFSETFROM:-0500");
            sb.AppendLine("TZOFFSETTO:-0400");
            sb.AppendLine("END:DAYLIGHT");
            sb.AppendLine("END:VTIMEZONE");
            sb.AppendLine("BEGIN:VEVENT");
                foreach (EventUser attn in attendees)
                {
                 var atuser =  _context.EventUser.Include(x=>x.Participant).Where(x => (x.Id == attn.Id)).SingleOrDefault();
                sb.AppendLine("ATTENDEE;CN=" + atuser.Participant.FullName + ";ROLE=REQ-PARTICIPANT;PARTSTAT=TENTATIVE;CUTYPE=INDIVIDUAL;RSVP=TRUE:MAILTO:" + atuser.Participant.Email);
                    if (atuser.IsOrganizer)
                        sb.AppendLine("ORGANIZER;CN="+ atuser.Participant.FullName+":MAILTO:"+ atuser.Participant.Email);
                }
                sb.AppendLine("DESCRIPTION:"+evtcal.Description);
                sb.AppendLine("DTEND;TZID=Eastern Standard Time:" + toUniversalTime(eddt));
                sb.AppendLine("DTSTAMP:" + strnow);
                sb.AppendLine("DTSTART;TZID=Eastern Standard Time:" + toUniversalTime(stdt));
                sb.AppendLine("LAST-MODIFIED:" + strnow);
                sb.AppendLine("LOCATION:");
                sb.AppendLine("SEQUENCE:0");
                sb.AppendLine("ACTION:DISPLAY");
                sb.AppendLine("SUMMARY:"+evtcal.Title);
                sb.AppendLine("UID:" + evtcal.UID);
                sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");
            return sb.ToString();
        }
       
    }
}
