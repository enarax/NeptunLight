﻿using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using NeptunLight.ViewModels;
using ReactiveUI;

namespace NeptunLight.Droid.Views
{
    public class MenuPage : ReactiveFragment<MenuPageViewModel>, IActionBarProvider
    {
        public TextView NameLabel { get; set; }
        public TextView InfoLabel { get; set; }

        public MenuButton MessagesButton { get; set; }
        public MenuButton CalendarButton { get; set; }
        public MenuButton CoursesButton { get; set; }
        public MenuButton ExamsButton { get; set; }
        public MenuButton SemestersButton { get; set; }
        public MenuButton PeriodsButton { get; set; }

        public ImageButton SettingsButton { get; set; }

        public override View OnCreateView(LayoutInflater inflater, [CanBeNull] ViewGroup container, [CanBeNull] Bundle savedInstanceState)
        {
            View layout = inflater.Inflate(Resource.Layout.MenuPage, container, false);

            Activated.InvokeCommand(this, x=> x.ViewModel.EnsureDataAccessible);
            this.WireUpControls(layout);
            
            this.BindCommand(ViewModel, x => x.GoToMessages, x => x.MessagesButton);
            this.BindCommand(ViewModel, x => x.GoToCalendar, x => x.CalendarButton);
            this.BindCommand(ViewModel, x => x.GoToCourses, x => x.CoursesButton);
            this.BindCommand(ViewModel, x => x.GoToExams, x => x.ExamsButton);
            this.BindCommand(ViewModel, x => x.GoToSemesters, x => x.SemestersButton);
            this.BindCommand(ViewModel, x => x.GoToPeriods, x => x.PeriodsButton);

            this.OneWayBind(ViewModel, x => x.Name, x => x.NameLabel.Text);
            this.OneWayBind(ViewModel, x => x.InfoLine, x => x.InfoLabel.Text);

            SettingsButton.Click += (sender, args) =>
            {
                StartActivity(new Intent(Context, typeof(SettingsActivity)));
            };

            return layout;
        }
    }
}