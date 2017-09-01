﻿using System;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using NeptunLight.ViewModels;
using ReactiveUI;

namespace NeptunLight.Droid.Pages
{
    public class MessageDetailsFragment : ReactiveFragment<MessageViewModel>
    {
        private TextView DateTextView { get; set; }
        private TextView SenderTextView { get; set; }
        private TextView SubjectTextView { get; set; }
        private TextView ContentTextView { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View layout = inflater.Inflate(Resource.Layout.MessageDetails, container, false);

            this.WireUpControls(layout);

            this.Bind(ViewModel, x => x.Date, x => x.DateTextView.Text);
            this.Bind(ViewModel, x => x.Sender, x => x.SenderTextView.Text);
            this.Bind(ViewModel, x => x.Title, x => x.SubjectTextView.Text);
            this.WhenAnyValue(x => x.ViewModel.HtmlContent).Subscribe(html =>
            {
                if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    ContentTextView.TextFormatted = Html.FromHtml(html, FromHtmlOptions.ModeLegacy);
                }
                else
                {
                    ContentTextView.TextFormatted = Html.FromHtml(html);
                }
            });

            return layout;
        }
    }
}