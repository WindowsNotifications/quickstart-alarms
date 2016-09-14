using Microsoft.Toolkit.Uwp.Notifications;
using Quickstart_Alarms.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Quickstart_Alarms.Helpers
{
    public static class AlarmSchedulerHelper
    {
        private const int DAYS_IN_ADVANCE_TO_SCHEDULE = 5;

        public static void ScheduleAlarm(MyAlarm alarm)
        {
            ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();

            DateTime now = DateTime.Now;

            DateTime[] alarmTimes = GetAlarmTimesForAlarm(alarm);

            foreach (var time in alarmTimes)
            {
                if (time.AddSeconds(5) > now)
                {
                    var scheduledNotif = GenerateAlarmNotification(alarm, time);
                    notifier.AddToSchedule(scheduledNotif);
                }
            }
        }

        private static ScheduledToastNotification GenerateAlarmNotification(MyAlarm alarm, DateTime alarmTime)
        {
            // Using NuGet package Microsoft.Toolkit.Uwp.Notifications
            ToastContent content = new ToastContent()
            {
                Scenario = ToastScenario.Alarm,

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = $"Alarm: {alarm.Name}"
                            },

                            new AdaptiveText()
                            {
                                Text = alarmTime.ToString()
                            }
                        }
                    }
                },

                Actions = new ToastActionsSnoozeAndDismiss()
            };

            return new ScheduledToastNotification(content.GetXml(), alarmTime);
        }

        private static DateTime[] GetAlarmTimesForAlarm(MyAlarm alarm)
        {
            if (alarm.IsOneTime())
            {
                return new DateTime[] { alarm.SingleFireTime };
            }
            else
            {
                DateTime today = DateTime.Today;
                List<DateTime> answer = new List<DateTime>();
                for (int i = 0; i < DAYS_IN_ADVANCE_TO_SCHEDULE; i++)
                {
                    if (alarm.DaysOfWeek.Contains(today.DayOfWeek))
                    {
                        answer.Add(today.Add(alarm.TimeOfDay));
                    }

                    today = today.AddDays(1);
                }

                return answer.ToArray();
            }
        }
    }
}
