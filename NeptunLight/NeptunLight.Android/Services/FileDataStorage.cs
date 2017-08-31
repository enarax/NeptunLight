﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using NeptunLight.Models;
using NeptunLight.Services;
using Newtonsoft.Json;
using ReactiveUI;

namespace NeptunLight.Droid.Services
{
    public class FileDataStorage : ReactiveObject, IDataStorage
    {
        private static string FileLocation => Path.Combine(Application.Context.FilesDir.Path, "neptunData.json");

        private NeptunData _currentData;

        public NeptunData CurrentData
        {
            get => _currentData;
            set => this.RaiseAndSetIfChanged(ref _currentData, value);
        }
        public async Task LoadDataAsync(bool forceReload = false)
        {
            if (File.Exists(FileLocation))
            {
                NeptunData deserializedData = null;
                await Task.Run(() =>
                {
                    string text = File.ReadAllText(FileLocation);
                    deserializedData = JsonConvert.DeserializeObject<NeptunDataProxy>(text).ToNeptunData();
                });
                CurrentData = deserializedData;
            }
        }

        public async Task SaveDataAsync()
        {
            await Task.Run(() =>
            {
                File.WriteAllText(FileLocation, JsonConvert.SerializeObject(NeptunDataProxy.FromNeptunData(CurrentData)));
            });
        }

        public async Task ClearDataAsync()
        {
            await Task.Run(() =>
            {
                if (File.Exists(FileLocation))
                {
                    File.Delete(FileLocation);
                }
                CurrentData = null;
            });
        }

        private class NeptunDataProxy
        {
            public static NeptunDataProxy FromNeptunData(NeptunData d)
            {
                return new NeptunDataProxy
                {
                    Calendar = d.Calendar.ToList(),
                    Messages = d.Messages.ToList(),
                    SubjectsPerSemester = d.SubjectsPerSemester.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToList()),
                    ExamsPerSemester = d.ExamsPerSemester.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToList()),
                    SemesterInfo = d.SemesterInfo.ToList(),
                    Periods = d.Periods.ToList()
                };
            }

            public NeptunData ToNeptunData()
            {
                return new NeptunData()
                {
                    Calendar = Calendar,
                    Messages = Messages,
                    SubjectsPerSemester =  SubjectsPerSemester.ToDictionary(kvp => Semester.Parse(kvp.Key), kvp => (IReadOnlyCollection<Subject>)kvp.Value),
                    ExamsPerSemester = ExamsPerSemester.ToDictionary(kvp => Semester.Parse(kvp.Key), kvp => (IReadOnlyCollection<Exam>)kvp.Value),
                    SemesterInfo = SemesterInfo,
                    Periods = Periods
                };
            }

            public List<Mail> Messages { get; set; }

            public List<CalendarEvent> Calendar { get; set; }

            public Dictionary<string, List<Subject>> SubjectsPerSemester { get; set; }

            public Dictionary<string, List<Exam>> ExamsPerSemester { get; set; }

            public IReadOnlyCollection<SemesterData> SemesterInfo { get; set; }

            public IReadOnlyCollection<Period> Periods { get; set; }
        }
    }
}