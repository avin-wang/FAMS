using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;

namespace FAMS.Services
{
    /// <summary>
    /// Converter of bool to image source.
    /// </summary>
    class BoolToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value == "有" ? @"\Resources\Icons\attachment_32px.png" : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? "无" : "有";
        }
    }

    /// <summary>
    /// Converter of length to bool.
    /// </summary>
    class LengthToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 0 ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter of string to bool.
    /// </summary>
    /// <history time="2018/02/27">create this method</history>
    class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value == "有" ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter of file names to file count.
    /// </summary>
    /// <history time="2018/02/27">create this method</history>
    class FileNameToFileCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strAttachedFileNames = (string)value;

            // No attached files.
            if (strAttachedFileNames == null || strAttachedFileNames == "")
            {
                return "";
            }

            // Only one attached file.
            if (!strAttachedFileNames.Contains("|") && !strAttachedFileNames.Contains("@"))
            {
                return "1";
            }

            // Multi attached files.
            if (strAttachedFileNames.StartsWith("@"))
            {
                strAttachedFileNames = strAttachedFileNames.Substring(1, strAttachedFileNames.Length - 1);
            }
            char[] separators = { '|', '@' };
            return strAttachedFileNames.Split(separators).Length.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter of file list to file count.
    /// </summary>
    /// <history time="2020/03/04">create this method</history>
    class FileListToFileCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as List<string>).Count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter of weather to icon source.
    /// </summary>
    class WeatherToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string w = (string)value;
            switch (w)
            {
                case "晴":
                    return @"\Resources\Icons\Weather\sunny_m.gif";
                case "多云":
                    return @"\Resources\Icons\Weather\cloudy_m.gif";
                case "阴":
                    return @"\Resources\Icons\Weather\overcast_m.gif";
                case "阵雨":
                    return @"\Resources\Icons\Weather\shower_m.gif";
                case "雷阵雨":
                    return @"\Resources\Icons\Weather\thundery_shower_m.gif";
                case "雷阵雨并伴有冰雹":
                    return @"\Resources\Icons\Weather\thundery_shower_with_hail_m.gif";
                case "雨夹雪":
                    return @"\Resources\Icons\Weather\sleet_m.gif";
                case "小雨":
                    return @"\Resources\Icons\Weather\drizzle_m.gif";
                case "中雨":
                    return @"\Resources\Icons\Weather\moderate_rain_m.gif";
                case "大雨":
                    return @"\Resources\Icons\Weather\pour_m.gif";
                case "暴雨":
                    return @"\Resources\Icons\Weather\storm_m.gif";
                case "大暴雨":
                    return @"\Resources\Icons\Weather\heavy_storm_m.gif";
                case "特大暴雨":
                    return @"\Resources\Icons\Weather\severe_storm_m.gif";
                case "阵雪":
                    return @"\Resources\Icons\Weather\snow_shower_m.gif";
                case "小雪":
                    return @"\Resources\Icons\Weather\light_snow_m.gif";
                case "中雪":
                    return @"\Resources\Icons\Weather\moderate_snow_m.gif";
                case "大雪":
                    return @"\Resources\Icons\Weather\heavy_snow_m.gif";
                case "暴雪":
                    return @"\Resources\Icons\Weather\severe_snow_m.gif";
                case "雾":
                    return @"\Resources\Icons\Weather\foggy_m.gif";
                case "冻雨":
                    return @"\Resources\Icons\Weather\freezing_rain_m.gif";
                case "沙尘暴":
                    return @"\Resources\Icons\Weather\sandstorm_m.gif";
                case "小到中雨":
                    return @"\Resources\Icons\Weather\light_to_moderate_rain_m.gif";
                case "中到大雨":
                    return @"\Resources\Icons\Weather\moderate_to_heavy_rain_m.gif";
                case "大到暴雨":
                    return @"\Resources\Icons\Weather\heavy_rain_to_storm_m.gif";
                case "暴雨到大暴雨":
                    return @"\Resources\Icons\Weather\storm_to_heavy_storm_m.gif";
                case "大暴雨到特大暴雨":
                    return @"\Resources\Icons\Weather\heavy_to_severe_storm_m.gif";
                case "小到中雪":
                    return @"\Resources\Icons\Weather\slight_to_moderate_snow_m.gif";
                case "中到大雪":
                    return @"\Resources\Icons\Weather\moderate_to_heavy_snow_m.gif";
                case "大到暴雪":
                    return @"\Resources\Icons\Weather\heavy_to_severe_snow_m.gif";
                case "浮尘":
                    return @"\Resources\Icons\Weather\floating_dust_m.gif";
                case "扬沙":
                    return @"\Resources\Icons\Weather\dust_blowing_m.gif";
                case "强沙尘暴":
                    return @"\Resources\Icons\Weather\heavy_sandstorm_m.gif";
                default:
                    return @"\Resources\Icons\Weather\unknown_m.gif";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter of bool to visibility.
    /// </summary>
    /// <history time="2018/03/10">create this method</history>
    class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }

    /// <summary>
    /// String reconstructing converter.
    /// </summary>
    /// <history time="2020/03/04">create this method</history>
    class StringReconstructConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = (string)value; // string sturcture: "str1|str2|str3"

            if (str == "" || str == null)
            {
                return str;
            }

            str = str.Replace("|", ";\r\n");

            return str; // string sturcture: "str1;\r\nstr2;\r\nstr3"
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = (string)value; // string sturcture: "str1;\r\nstr2;\r\nstr3"

            if (str == "" || str == null)
            {
                return str;
            }

            str = str.Replace(";\r\n", "|");

            return str; // string sturcture: "str1|str2|str3"
        }
    }
}
