using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;
using System.Xml;

namespace CalendarStyle
{
    public class GoogleCalendar
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        static string[] Scopes = { CalendarService.Scope.CalendarEvents };
        static string ApplicationName = "Style Bots calendar";

        public string[] Calendar()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("plzcalendar.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "tokenCalendar.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            //pull calendar url
            string googleCalendarUrl;
            googleCalendarUrl = ConfigurationManager.AppSettings.Get("googleSheetUrl");

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List(googleCalendarUrl);
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.

            Events events = request.Execute();
            string[] sentEvents = new string[events.Items.Count];
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                var num = 0;
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                    sentEvents[num] = (eventItem.Summary, when).ToString();
                    num++;
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
            return sentEvents;


        }


        public string AddMatch(string team1, string team2, DateTime scheduledDate, string timeZone)
        {
            UserCredential credential;
            Console.WriteLine("made it into add match");
            using (var stream =
                new FileStream("plzcalendar.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "tokenCalendars.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            Console.WriteLine("converted " + XmlConvert.ToString(scheduledDate, XmlDateTimeSerializationMode.Utc));
            Console.WriteLine("timezone " + timeZone);
            string googleCalendarUrl;
            googleCalendarUrl = ConfigurationManager.AppSettings.Get("googleCalendarUrl");
            Event newEvent = new Event()
            {
                Summary = team1 + " versus " + team2,
                Description = "A style match between " + team1 + " and " + team2,
                Start = new EventDateTime()
                {
                    DateTime = DateTime.Parse(XmlConvert.ToString(scheduledDate, XmlDateTimeSerializationMode.Utc)).AddHours(5),
                    TimeZone = timeZone,

                },
                End = new EventDateTime()
                {
                    DateTime = DateTime.Parse(XmlConvert.ToString(scheduledDate, XmlDateTimeSerializationMode.Utc)).AddHours(5),
                    TimeZone = timeZone,
                }

                /* America / New_York est
 America / Chicago central
 America / Denver mountain
 America / Los_Angeles pacific*/

            };
            Console.WriteLine("converted time " + scheduledDate.ToUniversalTime());

            EventsResource.InsertRequest request = service.Events.Insert(newEvent, googleCalendarUrl);
            try
            {
                Event createdEvent = request.Execute();
                Console.WriteLine("Event created: {0}", createdEvent.HtmlLink);
                return createdEvent.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex + " exception");
            }



            // Define parameters of request.
            /*EventsResource.ListRequest request = service.Events.List("r6addjhbp2mlduvh1o9mq9gua0@group.calendar.google.com");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            
            // List events.

            Events events = request.Execute();
            string[] sentEvents = new string[events.Items.Count];
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                var num = 0;
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                    sentEvents[num] = (eventItem.Summary, when).ToString();
                    num++;
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
            return sentEvents;
            */
            return null;

        }
    }
}