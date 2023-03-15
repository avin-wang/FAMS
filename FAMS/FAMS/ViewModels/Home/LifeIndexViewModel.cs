using System.ComponentModel;

namespace FAMS.ViewModels.Home
{
    /// <summary>
    /// Life indexes.
    /// </summary>
    class LifeIndexViewModel : INotifyPropertyChanged
    {
        private string _morningExerciseIndex; // 晨练指数
        private string _comfortIndex;         // 舒适度指数
        private string _dressingIndex;        // 穿衣指数
        private string _catchingColdIndex;    // 感冒指数
        private string _sunDryingIndex;       // 晾晒指数
        private string _travelIndex;          // 旅游指数
        private string _UVIndex;              // 紫外线指数
        private string _carWashingIndex;      // 洗车指数
        private string _sportIndex;           // 运动指数
        private string _datingIndex;          // 约会指数
        private string _umbrellaIndex;        // 雨伞指数

        public string MorningExerciseIndex
        {
            get { return _morningExerciseIndex; }
            set
            {
                _morningExerciseIndex = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MorningExerciseIndex"));
                }
            }
        }

        public string ComfortIndex
        {
            get { return _comfortIndex; }
            set
            {
                _comfortIndex = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ComfortIndex"));
                }
            }
        }

        public string DressingIndex
        {
            get { return _dressingIndex; }
            set
            {
                _dressingIndex = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DressingIndex"));
                }
            }
        }

        public string CatchingColdIndex
        {
            get { return _catchingColdIndex; }
            set
            {
                _catchingColdIndex = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CatchingColdIndex"));
                }
            }
        }

        public string SunDryingIndex
        {
            get { return _sunDryingIndex; }
            set
            {
                _sunDryingIndex = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SunDryingIndex"));
                }
            }
        }

        public string TravelIndex
        {
            get { return _travelIndex; }
            set
            {
                _travelIndex = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TravelIndex"));
                }
            }
        }

        public string UVIndex
        {
            get { return _UVIndex; }
            set
            {
                _UVIndex = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UVIndex"));
                }
            }
        }

        public string CarWashingIndex
        {
            get { return _carWashingIndex; }
            set
            {
                _carWashingIndex = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CarWashingIndex"));
                }
            }
        }

        public string SportIndex
        {
            get { return _sportIndex; }
            set
            {
                _sportIndex = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SportIndex"));
                }
            }
        }

        public string DatingIndex
        {
            get { return _datingIndex; }
            set
            {
                _datingIndex = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DatingIndex"));
                }
            }
        }

        public string UmbrellaIndex
        {
            get { return _umbrellaIndex; }
            set
            {
                _umbrellaIndex = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UmbrellaIndex"));
                }
            }
        }

        private string _morningExerciseDetail; // 晨练指数详情
        private string _comfortDetail;         // 舒适度指数详情
        private string _dressingDetail;        // 穿衣指数详情
        private string _catchingColdDetail;    // 感冒指数详情
        private string _sunDryingDetail;       // 晾晒指数详情
        private string _travelDetail;          // 旅游指数详情
        private string _UVDetail;              // 紫外线指数详情
        private string _carWashingDetail;      // 洗车指数详情
        private string _sportDetail;           // 运动指数详情
        private string _datingDetail;          // 约会指数详情
        private string _umbrellaDetail;        // 雨伞指数详情

        public string MorningExerciseDetail
        {
            get { return _morningExerciseDetail; }
            set
            {
                _morningExerciseDetail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MorningExerciseDetail"));
                }
            }
        }

        public string ComfortDetail
        {
            get { return _comfortDetail; }
            set
            {
                _comfortDetail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ComfortDetail"));
                }
            }
        }

        public string DressingDetail
        {
            get { return _dressingDetail; }
            set
            {
                _dressingDetail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DressingDetail"));
                }
            }
        }

        public string CatchingColdDetail
        {
            get { return _catchingColdDetail; }
            set
            {
                _catchingColdDetail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CatchingColdDetail"));
                }
            }
        }

        public string SunDryingDetail
        {
            get { return _sunDryingDetail; }
            set
            {
                _sunDryingDetail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SunDryingDetail"));
                }
            }
        }

        public string TravelDetail
        {
            get { return _travelDetail; }
            set
            {
                _travelDetail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TravelDetail"));
                }
            }
        }

        public string UVDetail
        {
            get { return _UVDetail; }
            set
            {
                _UVDetail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UVDetail"));
                }
            }
        }

        public string CarWashingDetail
        {
            get { return _carWashingDetail; }
            set
            {
                _carWashingDetail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CarWashingDetail"));
                }
            }
        }

        public string SportDetail
        {
            get { return _sportDetail; }
            set
            {
                _sportDetail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SportDetail"));
                }
            }
        }

        public string DatingDetail
        {
            get { return _datingDetail; }
            set
            {
                _datingDetail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DatingDetail"));
                }
            }
        }

        public string UmbrellaDetail
        {
            get { return _umbrellaDetail; }
            set
            {
                _umbrellaDetail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UmbrellaDetail"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
