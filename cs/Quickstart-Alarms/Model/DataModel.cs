using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Quickstart_Alarms.Model
{
    public static class DataModel
    {
        public static ObservableCollection<MyAlarm> Alarms { get; private set; }

        /// <summary>
        /// Adds an alarm to the storage, and schedules the alarm in the notifications platform
        /// </summary>
        /// <param name="alarm"></param>
        /// <returns></returns>
        public static async Task AddAlarm(MyAlarm alarm)
        {
            try
            {
                // Generate Id for it
                alarm.Id = Guid.NewGuid();

                Alarms.Add(alarm);

                await Task.Run(async delegate
                {
                    await SaveAlarmsAsync();

                    AlarmHelper.ScheduleAlarm(alarm);
                });
            }

            catch { }
        }

        public static async Task DeleteAlarm(MyAlarm alarm)
        {
            try
            {
                Alarms.Remove(alarm);

                await Task.Run(async delegate
                {
                    await SaveAlarmsAsync();

                    AlarmHelper.RemoveAlarm(alarm);
                });
            }
            catch { }
        }

        private static Task _loadTask;

        /// <summary>
        /// Initial app start should call this, which loads the alarms
        /// </summary>
        /// <returns></returns>
        public static Task LoadAsync()
        {
            if (_loadTask == null)
            {
                _loadTask = GenerateLoadTask();
            }

            return _loadTask;
        }

        public static void EnsureAllScheduled()
        {
            foreach (var alarm in Alarms)
            {
                AlarmHelper.EnsureScheduled(alarm);
            }
        }

        private static async Task GenerateLoadTask()
        {
            try
            {
                await Task.Run(async delegate
                {
                    var file = await GetAlarmsFileAsync();

                    try
                    {
                        using (Stream s = await file.OpenStreamForReadAsync())
                        {
                            MyAlarm[] alarms = (MyAlarm[])AlarmsSerializer.ReadObject(s);

                            Alarms = new ObservableCollection<MyAlarm>(alarms);
                        }
                    }
                    catch
                    {
                        await file.DeleteAsync();
                        Alarms = new ObservableCollection<MyAlarm>();
                    }
                });
            }

            catch (Exception ex)
            {
                Alarms = new ObservableCollection<MyAlarm>();
            }
        }

        private static async Task SaveAlarmsAsync()
        {
            try
            {
                var alarms = Alarms.ToArray();
                
                var file = await GetAlarmsFileAsync();

                using (Stream s = await file.OpenStreamForWriteAsync())
                {
                    AlarmsSerializer.WriteObject(s, alarms);
                }
            }
            catch { }
        }

        private static DataContractSerializer AlarmsSerializer = new DataContractSerializer(typeof(MyAlarm[]));

        private static Task<StorageFile> GetAlarmsFileAsync()
        {
            return ApplicationData.Current.LocalFolder.CreateFileAsync("Alarms.dat", CreationCollisionOption.OpenIfExists).AsTask();
        }
    }
}
