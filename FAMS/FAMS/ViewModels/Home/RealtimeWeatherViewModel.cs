using System.ComponentModel;

namespace FAMS.ViewModels.Home
{
    /// <summary>
    /// Real-time weather data.
    /// </summary>
    class RealtimeWeatherViewModel : INotifyPropertyChanged
    {
        private string _district;          // 城市
        private string _updateTime;        // 更新时间
        private string _lunarCalendarDate; // 农历日期
        private string _temperature;       // 温度
        private string _temperatureRange;  // 温度范围
        private string _weatherChange;     // 天气变化
        private string _wind;              // 风况
        private string _humidity;          // 湿度(%)
        private string _sunrise;           // 日出
        private string _sunset;            // 日落

        public string District
        {
            get { return _district; }
            set
            {
                _district = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("District"));
                }
            }
        }

        public string UpdateTime
        {
            get { return _updateTime; }
            set
            {
                _updateTime = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UpdateTime"));
                }
            }
        }

        public string LunarCalendarDate
        {
            get { return _lunarCalendarDate; }
            set
            {
                _lunarCalendarDate = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LunarCalendarDate"));
                }
            }
        }

        public string Temperature
        {
            get { return _temperature; }
            set
            {
                _temperature = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Temperature"));
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

        public string WeatherChange
        {
            get { return _weatherChange; }
            set
            {
                _weatherChange = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("WeatherChange"));
                }
            }
        }

        public string Wind
        {
            get { return _wind; }
            set
            {
                _wind = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Wind"));
                }
            }
        }

        public string Humidity
        {
            get { return _humidity; }
            set
            {
                _humidity = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Humidity"));
                }
            }
        }

        public string Sunrise
        {
            get { return _sunrise; }
            set
            {
                _sunrise = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Sunrise"));
                }
            }
        }

        public string Sunset
        {
            get { return _sunset; }
            set
            {
                _sunset = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Sunset"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
