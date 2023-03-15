using System.ComponentModel;

namespace FAMS.ViewModels.Home
{
    /// <summary>
    /// Environment data.
    /// </summary>
    class EnvironmentViewModel : INotifyPropertyChanged
    {
        private string _airQuality;      // 空气质量
        private string _suggest;         // 建议
        private string _majorPollutants; // 主要污染物
        private string _aqi;             // AQI（空气质量指数）
        private string _pm25;            // PM2.5（细颗粒物，即粒径<=2.5um的颗粒物）
        private string _pm10;            // PM10（可吸入颗粒物，即粒径<=10um的颗粒物）
        private string _o3;              // O3(臭氧)
        private string _co;              // CO(一氧化碳)
        private string _so2;             // SO2(二氧化硫)
        private string _no2;             // NO2(二氧化氮)
        private string _updateTime;      // 更新时间

        public string AirQuality
        {
            get { return _airQuality; }
            set
            {
                _airQuality = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AirQuality"));
                }
            }
        }

        public string Suggest
        {
            get { return _suggest; }
            set
            {
                _suggest = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Suggest"));
                }
            }
        }

        public string MajorPollutants
        {
            get { return _majorPollutants; }
            set
            {
                _majorPollutants = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MajorPollutants"));
                }
            }
        }

        public string AQI
        {
            get { return _aqi; }
            set
            {
                _aqi = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AQI"));
                }
            }
        }

        public string PM25
        {
            get { return _pm25; }
            set
            {
                _pm25 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PM25"));
                }
            }
        }

        public string PM10
        {
            get { return _pm10; }
            set
            {
                _pm10 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PM10"));
                }
            }
        }

        public string O3
        {
            get { return _o3; }
            set
            {
                _o3 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("O3"));
                }
            }
        }

        public string CO
        {
            get { return _co; }
            set
            {
                _co = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CO"));
                }
            }
        }

        public string SO2
        {
            get { return _so2; }
            set
            {
                _so2 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SO2"));
                }
            }
        }

        public string NO2
        {
            get { return _no2; }
            set
            {
                _no2 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("NO2"));
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
