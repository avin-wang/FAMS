using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FAMS.Commons.AttachedProperties
{
    /// <summary>
    /// Public control attached properties.
    /// </summary>
    public static class ControlAttachedProperty
    {
        #region FontIcon
        public static string GetFontIcon(DependencyObject obj)
        {
            return (string)obj.GetValue(FontIconProperty);
        }

        public static void SetFontIcon(DependencyObject obj, string value)
        {
            obj.SetValue(FontIconProperty, value);
        }

        // Using a DependencyProperty as the backing store for FontIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontIconProperty =
            DependencyProperty.RegisterAttached("FontIcon", typeof(string), typeof(ControlAttachedProperty), new PropertyMetadata(""));
        #endregion

        #region FontIconSize
        public static double GetFontIconSize(DependencyObject obj)
        {
            return (double)obj.GetValue(FontIconSizeProperty);
        }

        public static void SetFontIconSize(DependencyObject obj, double value)
        {
            obj.SetValue(FontIconSizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for FontIconSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontIconSizeProperty =
            DependencyProperty.RegisterAttached("FontIconSize", typeof(double), typeof(ControlAttachedProperty), new PropertyMetadata(12D));
        #endregion

        #region FontIconRotateAnimation
        public static bool GetFontIconRotateAnimation(DependencyObject obj)
        {
            return (bool)obj.GetValue(FontIconRotateAnimationProperty);
        }

        public static void SetFontIconRotateAnimation(DependencyObject obj, bool value)
        {
            obj.SetValue(FontIconRotateAnimationProperty, value);
        }

        // Using a DependencyProperty as the backing store for FontIconRotateAnimation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontIconRotateAnimationProperty =
            DependencyProperty.RegisterAttached("FontIconRotateAnimation", typeof(bool), typeof(ControlAttachedProperty), new PropertyMetadata(false, FontIconRotateAnimationPropertyChanged));


        /// <summary>
        /// Rotation animation of font icon.
        /// </summary>
        private static DoubleAnimation _rotationAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(200)));
        private static void FontIconRotateAnimationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var icon = sender as FrameworkElement;
            if (icon == null) return;
            if (icon.RenderTransformOrigin == new Point(0, 0))
            {
                icon.RenderTransformOrigin = new Point(0.5, 0.5);
                RotateTransform rt = new RotateTransform(0);
                icon.RenderTransform = rt;
            }
            var value = (bool)e.NewValue;
            if (value)
            {
                _rotationAnimation.To = 90;
                icon.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, _rotationAnimation);
            }
            else
            {
                _rotationAnimation.To = 0;
                icon.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, _rotationAnimation);
            }
        }
        #endregion

        /// <summary>
        /// Static constructor.
        /// </summary>
        static ControlAttachedProperty()
        {
        }
    }
}
