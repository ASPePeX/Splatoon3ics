using CommandLine;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using Calendar = Ical.Net.Calendar;

namespace Splatoon3ics
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var s3i = new S3I(args);
            s3i.Run();

        }

        class S3I
        {
            public class Options
            {
                [Option('u', "scheduleurl", Required = true, HelpText = "GDQ Schedule URL")]
                public string? ScheduleUrl { get; set; }
                [Option('o', "outputpath", Required = false, HelpText = "Output path")]
                public string? OutputPath { get; set; }
            }

            readonly ParserResult<Options> options;
            readonly HttpHelper httpHelper;
            readonly MD5 md5;

            public S3I(string[] args)
            {
                options = Parser.Default.ParseArguments<Options>(args);
                httpHelper = new HttpHelper();
                md5 = MD5.Create();
            }

            public void Run()
            {
                options?.WithParsed(o =>
                {
                    if (!string.IsNullOrWhiteSpace(o.ScheduleUrl))
                    {
                        var jsonData = HttpUtility.HtmlDecode(httpHelper.GetUrl(o.ScheduleUrl).Result);
                        var splatData = SplatData.FromJson(jsonData);

                        var serializer = new CalendarSerializer();
                        var calendarSalmonrun = ParseSalmonRunSchedule(splatData);
                        var serializedCalendarSalmonRun = serializer.SerializeToString(calendarSalmonrun);
                        serializedCalendarSalmonRun = Regex.Replace(serializedCalendarSalmonRun, @"DTSTAMP:.*?\n", "");

                        File.WriteAllText(o.OutputPath + "salmonrun.ics", serializedCalendarSalmonRun);
                    }
                });
            }

            private Calendar ParseSalmonRunSchedule(SplatData splatData)
            {
                var calendar = new Calendar();
                calendar.AddTimeZone(TimeZoneInfo.Utc);

                foreach (var rotation in splatData.Data.CoopGroupingSchedule.RegularSchedules.Nodes)
                {
                    DateTime dtStart = rotation.StartTime.UtcDateTime.AddSeconds(1);
                    DateTime dtEnd = rotation.EndTime.UtcDateTime.AddSeconds(-1);

                    var calEvent = new CalendarEvent()
                    {
                        Summary = $"{rotation.Setting.CoopStage.Name} - {rotation.Splatoon3InkKingSalmonidGuess}",
                        Start = new CalDateTime(dtStart, "Utc"),
                        End = new CalDateTime(dtEnd, "Utc"),
                        Location = $"{rotation.Setting.CoopStage.Name}",
                        Description = $"Weapons:\n{rotation.Setting.Weapons[0].Name}\n{rotation.Setting.Weapons[1].Name}\n{rotation.Setting.Weapons[2].Name}\n{rotation.Setting.Weapons[3].Name}\n\nKing:\n{rotation.Splatoon3InkKingSalmonidGuess}"
                    };

                    calendar.Events.Add(calEvent);
                }

                return calendar;
            }
        }



    }


}
