using Microsoft.Toolkit.Uwp.Notifications;
using Quickstart_Alarms.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Quickstart_Alarms
{
    public static class AlarmHelper
    {
        private const int DAYS_IN_ADVANCE_TO_SCHEDULE = 5;

        public static void ScheduleAlarm(MyAlarm alarm)
        {
            EnsureScheduled(alarm, checkForExisting: false);
        }

        public static void RemoveAlarm(MyAlarm alarm)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var tag = GetTag(alarm);

            // Find all of the scheduled toasts for the alarm
            var scheduledNotifs = notifier.GetScheduledToastNotifications()
                .Where(i => i.Tag.Equals(tag));

            // Remove all of those from the schedule
            foreach (var n in scheduledNotifs)
            {
                notifier.RemoveFromSchedule(n);
            }
        }

        public static void EnsureScheduled(MyAlarm alarm)
        {
            EnsureScheduled(alarm, checkForExisting: true);
        }

        private static void EnsureScheduled(MyAlarm alarm, bool checkForExisting)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            IReadOnlyList<ScheduledToastNotification> existing = null;
            if (checkForExisting)
            {
                var tag = GetTag(alarm);
                existing = notifier.GetScheduledToastNotifications()
                    .Where(i => i.Tag.Equals(tag))
                    .ToList();
            }

            DateTime now = DateTime.Now;

            DateTime[] alarmTimes = GetAlarmTimesForAlarm(alarm);

            foreach (var time in alarmTimes)
            {
                if (time.AddSeconds(5) > now)
                {
                    // If the alarm isn't scheduled already
                    if (!checkForExisting || !existing.Any(i => i.DeliveryTime == time))
                    {
                        var scheduledNotif = GenerateAlarmNotification(alarm, time);
                        notifier.AddToSchedule(scheduledNotif);
                    }
                }
            }
        }

        private static string GetTag(MyAlarm alarm)
        {
            // Tag needs to be 16 chars or less, so hash the Id
            return alarm.Id.GetHashCode().ToString();
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

            // We can easily enable Universal Dismiss by generating a RemoteId for the alarm that will be
            // the same on both devices. We'll just use the alarm delivery time. If an alarm on one device
            // has the same delivery time as an alarm on another device, it'll be dismissed when one of the
            // alarms is dismissed.
            string remoteId = (alarmTime.Ticks / 10000000 / 60).ToString(); // Minutes

            return new ScheduledToastNotification(content.GetXml(), alarmTime)
            {
                Tag = GetTag(alarm),

                // RemoteId is a 1607 feature, if you support older systems, use ApiInformation to check if property is present
                RemoteId = remoteId
            };
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
