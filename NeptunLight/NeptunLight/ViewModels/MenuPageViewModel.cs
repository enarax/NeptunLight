﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using NeptunLight.Services;
using ReactiveUI;

namespace NeptunLight.ViewModels
{
    public class MenuPageViewModel : PageViewModel
    {
        public MenuPageViewModel(IDataStorage storage, INavigator navigator, IRefreshManager refreshManager)
        {
            EnsureDataAccessible = ReactiveCommand.CreateFromTask(async () =>
            {
                if (storage.CurrentData == null)
                    await storage.LoadDataAsync();
                // see if there is saved data available
                if (storage.CurrentData == null)
                {
                    // need to refresh
                    navigator.NavigateTo<InitialSyncPageViewModel>(false);
                }
            });

            IObservable<bool> menuAvailable = EnsureDataAccessible.IsExecuting.Select(x => !x);
            GoToMessages = ReactiveCommand.Create(() => navigator.NavigateTo<MessagesPageViewModel>(), menuAvailable);
            GoToCalendar = ReactiveCommand.Create(() => navigator.NavigateTo<CalendarPageViewModel>(), menuAvailable);
            GoToCourses = ReactiveCommand.Create(() => navigator.NavigateTo<CoursesPageViewModel>(), menuAvailable);
            GoToExams = ReactiveCommand.Create(() => navigator.NavigateTo<ExamsPageViewModel>(), menuAvailable);
            GoToSemesters = ReactiveCommand.Create(() => navigator.NavigateTo<SemestersPageViewModel>(), menuAvailable);
            GoToPeriods = ReactiveCommand.Create(() => navigator.NavigateTo<PeriodsPageViewModel>(), menuAvailable);

            storage.WhenAnyValue(x => x.CurrentData.BasicData.Name).ToProperty(this, x => x.Name, out _name);
            storage.WhenAnyValue(x => x.CurrentData.BasicData.NeptunCode).ToProperty(this, x => x.InfoLine, out _infoLine);

            refreshManager.WhenAnyValue(x => x.IsRefreshing).ObserveOn(RxApp.MainThreadScheduler).ToProperty(this, x => x.IsRefreshing, out _isRefreshing);
            refreshManager.WhenAnyValue(x => x.LastRefreshTime).ObserveOn(RxApp.MainThreadScheduler).ToProperty(this, x => x.LastRefreshTime, out _lastRefreshTime);
            Refresh = ReactiveCommand.CreateFromTask((_) => refreshManager.RefreshAsync(), refreshManager.WhenAnyValue(x => x.IsRefreshing).Select(b => !b));
        }

        private readonly ObservableAsPropertyHelper<bool> _isRefreshing;
        public bool IsRefreshing => _isRefreshing.Value;

        private readonly ObservableAsPropertyHelper<DateTime> _lastRefreshTime;
        public DateTime LastRefreshTime => _lastRefreshTime.Value;

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        private readonly ObservableAsPropertyHelper<string> _name;
        public string Name => _name.Value;

        private readonly ObservableAsPropertyHelper<string> _infoLine;
        public string InfoLine => _infoLine.Value;

        public ReactiveCommand<Unit, Unit> EnsureDataAccessible { get; }

        public ReactiveCommand<Unit, Unit> GoToMessages { get; }
        public ReactiveCommand<Unit, Unit> GoToCalendar { get; }
        public ReactiveCommand<Unit, Unit> GoToCourses { get; }
        public ReactiveCommand<Unit, Unit> GoToExams { get; }
        public ReactiveCommand<Unit, Unit> GoToSemesters { get; }
        public ReactiveCommand<Unit, Unit> GoToPeriods { get; }

        public override string Title { get; } = "Neptun Lite";
    }
}