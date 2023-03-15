using System.ComponentModel;

namespace FAMS.ViewModels.Home
{
    /// <summary>
    /// Weather data for forecasting.
    /// </summary>
    class WeatherViewModel : INotifyPropertyChanged
    {
        private string _week;             // 星期
        private string _date;             // 阳历日期
        private string _temperatureRange; // 温度范围
        private string _dayWeather;       // 白天天气
        private string _dayWind;          // 白天风况
        private string _nightWeather;     // 夜间天气
        private string _nightWind;        // 夜间风况

        public string Week
        {
            get { return _week; }
            set
            {
                _week = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Week"));
                }
            }
        }

        public string Date
        {
            get { return _date; }
            set
            {
                _date = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Date"));
                }
            }
        }

        public string TemperatureRange
        {
            get { return _temperatureRange; }
            set
            {
                _temperatureRange = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TemperatureRange"));
                }
            }
        }

        public string DayWeather
        {
            get { return _dayWeather; }
            set
            {
                _dayWeather = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DayWeather"));
                }
            }
        }

        public string DayWind
        {
            get { return _dayWind; }
            set
            {
                _dayWind = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DayWind"));
                }
            }
        }

        public string NightWeather
        {
            get { return _nightWeather; }
            set
            {
                _nightWeather = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("NightWeather"));
                }
            }
        }

        public string NightWind
        {
            get { return _nightWind; }
            set
            {
                _nightWind = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("NightWind"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
