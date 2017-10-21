﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MikePhil.Charting.Animation;
using MikePhil.Charting.Charts;
using MikePhil.Charting.Components;
using MikePhil.Charting.Data;
using MikePhil.Charting.Formatter;
using NeptunLight.ViewModels;
using ReactiveUI;

namespace NeptunLight.Droid.Views
{
    public class SemestersCreditsTab : ReactiveFragment<SemestersCreditsTabViewModel>
    {
        public BarChart BarChart { get; set; }

        public LineChart LineChart { get; set; }

        public override View OnCreateView([NotNull] LayoutInflater inflater, [CanBeNull] ViewGroup container, [CanBeNull] Bundle savedInstanceState)
        {
            View layout = inflater.Inflate(Resource.Layout.SemestersCreditsTab, container, false);

            this.WireUpControls(layout);

            #region BarChart

            BarChart.SetTouchEnabled(false);
            BarChart.AxisLeft.AxisMinimum = -.1f;
            LimitLine targetLine = new LimitLine(30) { LineColor = Color.ParseColor("#2962ff"), LineWidth = 1f };
            targetLine.EnableDashedLine(20f, 8f, 0);
            BarChart.AxisLeft.AddLimitLine(targetLine);
            BarChart.XAxis.AxisMinimum = 0.5f;
            BarChart.XAxis.Granularity = 1f;
            BarChart.XAxis.GranularityEnabled = true;
            BarChart.XAxis.SetDrawGridLines(false);
            BarChart.XAxis.ValueFormatter = new SemesterValueFormater();
            BarChart.Description.Enabled = false;

            this.WhenAnyValue(x => x.ViewModel.BarChartData).Subscribe(bcd =>
            {
                float groupSpace = 0.06f;
                float barSpace = 0.02f; // x2 dataset
                float barWidth = 0.45f; // x2 dataset
                // (0.02 + 0.45) * 2 + 0.06 = 1.00 -> interval per "group"

                List<BarEntry> entriesTaken = new List<BarEntry>();
                List<BarEntry> entriesAccomplished = new List<BarEntry>();
                for (int i = 0; i < bcd.Count; i++)
                {
                    entriesTaken.Add(new BarEntry(i + 0.5f, bcd[i].Taken));
                    entriesAccomplished.Add(new BarEntry(i + 0.5f, bcd[i].Accomplished));
                }
                BarDataSet setTaken = new BarDataSet(entriesTaken, "Felvett") { Color = Color.ParseColor("#90caf9"), ValueTextSize = 10f};
                BarDataSet setAccomplished = new BarDataSet(entriesAccomplished, "Teljesített") { Color = Color.ParseColor("#1e88e5"), ValueTextSize = 10f };
                BarData barData = new BarData(setTaken, setAccomplished);
                barData.BarWidth = barWidth;
                BarChart.Data = barData;
                BarChart.GroupBars(0.5f, groupSpace, barSpace);
                BarChart.XAxis.AxisMaximum = bcd.Count + 0.5f;

                BarChart.AnimateY(2000, Easing.EasingOption.EaseInOutCubic);
            });

            #endregion

            #region LineChart


            LineChart.XAxis.AxisMinimum = -0.1f;
            LineChart.XAxis.Granularity = 1f;
            LineChart.XAxis.GranularityEnabled = true;
            LineChart.SetTouchEnabled(false);
            LineChart.Description.Enabled = false;
            LineChart.XAxis.ValueFormatter = new SemesterValueFormater();
            this.WhenAnyValue(x => x.ViewModel.LineChartData).Subscribe(lcd =>
            {
                List<Entry> entriesActual = new List<Entry>();
                List<Entry> entriesTrend = new List<Entry>();
                for (int i = 0; i < lcd.Count; i++)
                {
                    entriesActual.Add(new Entry(i, lcd[i].CumulativeCredits));
                    entriesTrend.Add(new Entry(i, 30*i));
                }
                LineDataSet setActual = new LineDataSet(entriesActual, "Megszerzett") {Color = Color.ParseColor("#673ab7"), LineWidth = 2f, CircleRadius = 5f, CircleHoleRadius = 3.5f, ValueTextSize = 10f};
                LineDataSet setTrend = new LineDataSet(entriesTrend, "Cél") {Color = Color.ParseColor("#66666666"), LineWidth = 1.5f };
                setTrend.EnableDashedLine(10f, 5f, 0);
                setTrend.SetDrawCircles(false);
                setTrend.SetDrawValues(false);
                LineData data = new LineData(setTrend, setActual);
                LineChart.Data = data;
                LineChart.AnimateY(2000, Easing.EasingOption.EaseInOutCubic);
            });

            #endregion



            return layout;
        }

        private class SemesterValueFormater : Java.Lang.Object, IAxisValueFormatter
        {
            public string GetFormattedValue(float value, [NotNull] AxisBase axis)
            {
                return $"{value}.";
            }
        }
    }
}