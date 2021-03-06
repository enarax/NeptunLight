﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using DynamicData;
using JetBrains.Annotations;
using NeptunLight.Models;
using NeptunLight.Services;
using Newtonsoft.Json;
using ReactiveUI;

namespace NeptunLight.Droid.Services
{
    public class FileDataStorage : ReactiveObject, IDataStorage
    {
        private static readonly SemaphoreSlim FileLock = new SemaphoreSlim(1, 1);

        private static string FileLocation => Path.Combine(Application.Context.FilesDir.Path, "neptunData.json");

        private NeptunData _currentData;

        public NeptunData CurrentData
        {
            get => _currentData;
            set => this.RaiseAndSetIfChanged(ref _currentData, value);
        }
        public async Task LoadDataAsync(bool forceReload = false)
        {
            try
            {
                await FileLock.WaitAsync();
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
            finally
            {
                FileLock.Release();
            }
            
        }

        public async Task SaveDataAsync()
        {
            try
            {
                await FileLock.WaitAsync();
                await Task.Run(() =>
                {
                    if (CurrentData != null)
                        File.WriteAllText(FileLocation, JsonConvert.SerializeObject(NeptunDataProxy.FromNeptunData(CurrentData)));
                    else
                        File.Delete(FileLocation);
                });
            }
            finally
            {
                FileLock.Release();
            }
            
        }

        public async Task ClearDataAsync()
        {
            try
            {
                await FileLock.WaitAsync();
                await Task.Run(() =>
                {
                    if (File.Exists(FileLocation))
                    {
                        File.Delete(FileLocation);
                    }
                    CurrentData = null;
                });
            }
            finally
            {
                FileLock.Release();
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private class NeptunDataProxy
        {
            public static NeptunDataProxy FromNeptunData(NeptunData d)
            {
                return new NeptunDataProxy
                {
                    BasicData = d.BasicData,
                    Calendar = d.Calendar.ToList(),
                    Messages = d.Messages.Items.ToList(),
                    SubjectsPerSemester = d.SubjectsPerSemester.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToList()),
                    ExamsPerSemester = d.ExamsPerSemester.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToList()),
                    SemesterInfo = d.SemesterInfo.ToList(),
                    Periods = d.Periods.ToList()
                };
            }

            public NeptunData ToNeptunData()
            {
                NeptunData data = new NeptunData()
                {
                    BasicData = BasicData,
                    Calendar = Calendar,
                    SubjectsPerSemester =  SubjectsPerSemester.ToDictionary(kvp => Semester.Parse(kvp.Key), kvp => (IReadOnlyCollection<Subject>)kvp.Value),
                    ExamsPerSemester = ExamsPerSemester.ToDictionary(kvp => Semester.Parse(kvp.Key), kvp => (IReadOnlyCollection<Exam>)kvp.Value),
                    SemesterInfo = SemesterInfo,
                    Periods = Periods
                };
                data.Messages.AddRange(Messages);
                return data;
            }

            public BasicNeptunData BasicData { get; set; }

            public List<Mail> Messages { get; set; }

            public List<CalendarEvent> Calendar { get; set; }

            public Dictionary<string, List<Subject>> SubjectsPerSemester { get; set; }

            public Dictionary<string, List<Exam>> ExamsPerSemester { get; set; }

            public IReadOnlyCollection<SemesterData> SemesterInfo { get; set; }

            public IReadOnlyCollection<Period> Periods { get; set; }
        }
    }
}