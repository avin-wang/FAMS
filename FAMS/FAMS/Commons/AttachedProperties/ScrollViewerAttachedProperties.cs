﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace FAMS.Commons.AttachedProperties
{
    /// <summary>
    /// Attention: this class is only used for ScrollViewer of ListView, mainly for setting the ListView always scroll to end
    /// </summary>
    public class ScrollViewerAttachedProperties
    {
        private static bool _autoScroll;

        public static readonly DependencyProperty AlwaysScrollToEndProperty =
            DependencyProperty.RegisterAttached("AlwaysScrollToEnd", typeof(bool), typeof(ScrollViewerAttachedProperties), new PropertyMetadata(false, AlwaysScrollToEndChanged));        


        private static void AlwaysScrollToEndChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scroll = sender as ScrollViewer;
            if (scroll != null)
            {
                bool alwaysScrollToEnd = (e.NewValue != null) && (bool)e.NewValue;
                if (alwaysScrollToEnd)
                {
                    scroll.ScrollToEnd();
                    scroll.ScrollChanged += ScrollChanged;
                }
                else
                {
                    scroll.ScrollChanged -= ScrollChanged;
                }
            }
            else
            {
                throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to ScrollViewer instances.");
            }
        }

        public static bool GetAlwaysScrollToEnd(ScrollViewer scroll)
        {
            if (scroll == null)
            {
                throw new ArgumentNullException("scroll");
            }

            return (bool)scroll.GetValue(AlwaysScrollToEndProperty);
        }

        public static void SetAlwaysScrollToEnd(ScrollViewer scroll, bool alwaysScrollToEnd)
        {
            if (scroll == null)
            {
                throw new ArgumentNullException("scroll");
            }

            scroll.SetValue(AlwaysScrollToEndProperty, alwaysScrollToEnd);
        }

        private static void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scroll = sender as ScrollViewer;
            if (scroll == null)
            {
                throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to ScrollViewer instances.");
            }

            if (e.ExtentHeightChange == 0)
            {
                _autoScroll = scroll.VerticalOffset == scroll.ScrollableHeight;
            }

            if (_autoScroll && e.ExtentHeightChange != 0)
            {
                scroll.ScrollToVerticalOffset(scroll.ExtentHeight);
            }
        }
    }
}
