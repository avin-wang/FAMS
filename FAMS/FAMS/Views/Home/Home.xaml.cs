using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Globalization;
using FAMS.Views.Accounts;
using FAMS.Models.Home;
using FAMS.ViewModels.Home;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media;

namespace FAMS.Views.Home
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        private HomeModel _model = new HomeModel();

        // Weather fields.
        private DispatcherTimer _weatherRefreshTimer = new DispatcherTimer();
        private string _province = "广东";
        private string _city = "深圳";
        private string _country = "深圳";

        // Analog clock fields
        private double _radius = 250; // dial radius of the analog clock
        private double _angle = 360;
        private Point _pointClkCenter;  // center point of the analog clock
        private Label _numericClk = new Label(); // numeric clock

        private DispatcherTimer _timer1 = new DispatcherTimer();
        private System.Timers.Timer _timer2 = new System.Timers.Timer(1000);
        private DateTime _currTime = DateTime.Now;  // current time

        public Home()
        {
            InitializeComponent();
            InitializeContext();
        }

        private void InitializeContext()
        {
            // Start up analog clock
            StartUpAnalogClock();

            // Load districts
            LoadDistricts();

            // Start weather refreshing
            StartWeatherRefreshing(1800); // Refreshing cycle = 1800s (i.e., 30min)

            // Weather forecast
            _model.GetWeatherInfo(_province, _city, _country); // Get weather info from server
            RealtimeWeatherViewModel rw = _model.GetRealtimeWeatherInfo();
            EnvironmentViewModel ev = _model.GetEnvironmentInfo();
            LifeIndexViewModel li = _model.GetLifeIndexInfo();
            List<WeatherViewModel> wl = _model.GetWeatherForecastInfo();
            this.grdRW.DataContext = rw;
            this.grdEv.DataContext = ev;
            this.grdLi.DataContext = li;
            if (wl != null)
            {
                this.grdYesterday.DataContext = wl[0];
                this.grdToday.DataContext = wl[1];
                this.grdTomorrow.DataContext = wl[2];
                this.grdAfterTomorrow.DataContext = wl[3];
                this.grdThreeDays.DataContext = wl[4];
                this.grdFourDays.DataContext = wl[5];
            }
            else
            {
                WeatherViewModel w = new WeatherViewModel();
                this.grdYesterday.DataContext = w;
                this.grdToday.DataContext = w;
                this.grdTomorrow.DataContext = w;
                this.grdAfterTomorrow.DataContext = w;
                this.grdThreeDays.DataContext = w;
                this.grdFourDays.DataContext = w;
            }

            this.grdRW.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(District_Click));
            this.miLifeIndexDetail.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(MiLifeIndexDetail_Click));
        }

        /// <summary>
        /// Load districts
        /// </summary>
        private void LoadDistricts()
        {
            List<MenuItem> districtList = _model.LoadDistricts();

            foreach (MenuItem mi in districtList)
            {
                this.miDistrict.Items.Add(mi);
            }
        }

        #region Analog clock
        private void StartUpAnalogClock()
        {
            _pointClkCenter = new Point(250, 250); // coordinate of the analog center

            // Draw analog clock
            DrawDigit();
            DrawGridLine();
            DrawNumericClock();

            // Initialize timer
            _timer1.Interval = TimeSpan.FromMilliseconds(100);
            _timer1.Tick += Timer_Tick;
            _timer1.IsEnabled = true;
            _timer1.Start();

            // Initialize numeric clock
            _timer2.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            _timer2.Enabled = true;
        }

        /// <summary>
        /// Draw digit of the analog clock
        /// </summary>
        private void DrawDigit()
        {
            double x, y;
            for (int i = 1; i < 13; i++)
            {
                _angle = WrapAngle(i * 360.0 / 12.0) - 90.0;
                _angle = ConvertToRadian(_angle);

                x = _pointClkCenter.X + Math.Cos(_angle) * (_radius - 36) - 8;
                y = _pointClkCenter.Y + Math.Sin(_angle) * (_radius - 36) - 15;

                TextBlock digit = new TextBlock();
                digit.FontSize = 26;
                digit.Text = i.ToString();

                // calibrate the digit '12'
                if (i == 12)
                {
                    Canvas.SetLeft(digit, x - 8);
                }
                else
                {
                    Canvas.SetLeft(digit, x);
                }
                Canvas.SetTop(digit, y);
                _analogClk.Children.Add(digit);
            }
        }

        /// <summary>
        /// Draw grid line of the ananlog clock
        /// </summary>
        private void DrawGridLine()
        {
            double x1 = 0, y1 = 0;
            double x2 = 0, y2 = 0;

            for (int i = 0; i < 60; i++)
            {
                double angle1 = WrapAngle(i * 360.0 / 60.0) - 90;
                angle1 = ConvertToRadian(angle1);

                if (i % 5 == 0)
                {
                    x1 = Math.Cos(angle1) * (_radius - 20);
                    y1 = Math.Sin(angle1) * (_radius - 20);
                }
                else
                {
                    x1 = Math.Cos(angle1) * (_radius - 10);
                    y1 = Math.Sin(angle1) * (_radius - 10);
                }

                x2 = Math.Cos(angle1) * (_radius - 5);
                y2 = Math.Sin(angle1) * (_radius - 5);

                Line line = new Line();
                line.X1 = x1;
                line.Y1 = y1;
                line.X2 = x2;
                line.Y2 = y2;
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 3;

                Canvas.SetLeft(line, _pointClkCenter.X);
                Canvas.SetTop(line, _pointClkCenter.Y);
                _analogClk.Children.Add(line);

            }
        }

        /// <summary>
        /// 360 degrees angle system
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private double WrapAngle(double angle)
        {
            return angle % 360;
        }

        /// <summary>
        /// Convert angle degree to radian
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        private double ConvertToRadian(double degree)
        {
            double radian = (Math.PI / 180) * degree;
            return radian;
        }

        ///<summary>
        /// Draw numeric clock
        ///</summary>
        private void DrawNumericClock()
        {
            _numericClk.Content = "00:00:00";
            _numericClk.FontSize = 26;
            _numericClk.FontWeight = FontWeights.Light;
            _numericClk.FontFamily = new FontFamily("Tahoma,仿宋");  // Helvetica
            _numericClk.Foreground = new SolidColorBrush(Colors.Gray);
            Canvas.SetLeft(_numericClk, 199);
            Canvas.SetTop(_numericClk, 380);
            _analogClk.Children.Add(_numericClk);
        }

        /// <summary>
        /// Update time and refresh the clock
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            this.rtSecHand.Angle = DateTime.Now.Second * 6; // refresh angle of the second hand
            this.rtMinHand.Angle = DateTime.Now.Minute * 6 + (DateTime.Now.Second / 60.0) * 6; // refresh angle of the minute hand

            int hour = DateTime.Now.Hour;
            if (hour >= 12)
            {
                hour -= 12;
            }
            this.rtHourHand.Angle = hour * 30 + (DateTime.Now.Minute / 2.0); // refresh angle of the hour hand
        }

        /// <summary>
        /// Update numeric clock time
        /// </summary>
        /// <history time="2020/02/26">create this method</history>
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                this._numericClk.Content = DateTime.Now.ToString("HH:mm:ss");
            }));
        }
        #endregion Analog clock

        /// <summary>
        /// Start weather refreshing
        /// </summary>
        /// <param name="cycle">cycle in seconds</param>
        private void StartWeatherRefreshing(double cycle)
        {
            _weatherRefreshTimer.Interval = TimeSpan.FromSeconds(cycle);
            _weatherRefreshTimer.IsEnabled = true;
            _weatherRefreshTimer.Tick += WeatherRefreshTimer_Tick;
            _weatherRefreshTimer.Start();
        }

        /// <summary>
        /// Run this method in every time interval
        /// </summary>
        private void WeatherRefreshTimer_Tick(object sender, EventArgs e)
        {
            _model.GetWeatherInfo(_province, _city, _country); // Get weather info from server
            RealtimeWeatherViewModel rw = _model.GetRealtimeWeatherInfo();
            EnvironmentViewModel ev = _model.GetEnvironmentInfo();
            LifeIndexViewModel li = _model.GetLifeIndexInfo();
            List<WeatherViewModel> wl = _model.GetWeatherForecastInfo();

            this.grdRW.DataContext = rw;
            this.grdEv.DataContext = ev;
            this.grdLi.DataContext = li;
            if (wl != null)
            {
                this.grdYesterday.DataContext = wl[0];
                this.grdToday.DataContext = wl[1];
                this.grdTomorrow.DataContext = wl[2];
                this.grdAfterTomorrow.DataContext = wl[3];
                this.grdThreeDays.DataContext = wl[4];
                this.grdFourDays.DataContext = wl[5];
            }
            else
            {
                WeatherViewModel w = new WeatherViewModel();
                this.grdYesterday.DataContext = w;
                this.grdToday.DataContext = w;
                this.grdTomorrow.DataContext = w;
                this.grdAfterTomorrow.DataContext = w;
                this.grdThreeDays.DataContext = w;
                this.grdFourDays.DataContext = w;
            }
        }

        /// <summary>
        /// (Do not)Show life indexes' details
        /// </summary>
        private void MiLifeIndexDetail_Click(object sender, RoutedEventArgs e)
        {
            bool b = this.miLifeIndexDetail.IsChecked;

            GridLengthAnimation glaHeight1 = new GridLengthAnimation();
            GridLengthAnimation glaHeight2 = new GridLengthAnimation();
            GridLengthAnimation glaHeight3 = new GridLengthAnimation();
            GridLengthAnimation glaHeight4 = new GridLengthAnimation();
            GridLengthAnimation glaHeight5 = new GridLengthAnimation();
            GridLengthAnimation glaHeight6 = new GridLengthAnimation();
            GridLengthAnimation glaHeight7 = new GridLengthAnimation();
            GridLengthAnimation glaHeight8 = new GridLengthAnimation();
            GridLengthAnimation glaHeight9 = new GridLengthAnimation();
            GridLengthAnimation glaHeight10 = new GridLengthAnimation();
            GridLengthAnimation glaHeight11 = new GridLengthAnimation();

            // Specify start and end points
            if (b) // Unfold.
            {
                glaHeight1.From = new GridLength(0);
                glaHeight1.To = GridLength.Auto;
                glaHeight2.From = new GridLength(0);
                glaHeight2.To = GridLength.Auto;
                glaHeight3.From = new GridLength(0);
                glaHeight3.To = GridLength.Auto;
                glaHeight4.From = new GridLength(0);
                glaHeight4.To = GridLength.Auto;
                glaHeight5.From = new GridLength(0);
                glaHeight5.To = GridLength.Auto;
                glaHeight6.From = new GridLength(0);
                glaHeight6.To = GridLength.Auto;
                glaHeight7.From = new GridLength(0);
                glaHeight7.To = GridLength.Auto;
                glaHeight8.From = new GridLength(0);
                glaHeight8.To = GridLength.Auto;
                glaHeight9.From = new GridLength(0);
                glaHeight9.To = GridLength.Auto;
                glaHeight10.From = new GridLength(0);
                glaHeight10.To = GridLength.Auto;
                glaHeight11.From = new GridLength(0);
                glaHeight11.To = GridLength.Auto;
            }
            else // Fold
            {
                glaHeight1.From = new GridLength(this.hidedRow1.ActualHeight);
                glaHeight1.To = new GridLength(0);
                glaHeight2.From = new GridLength(this.hidedRow2.ActualHeight);
                glaHeight2.To = new GridLength(0);
                glaHeight3.From = new GridLength(this.hidedRow3.ActualHeight);
                glaHeight3.To = new GridLength(0);
                glaHeight4.From = new GridLength(this.hidedRow4.ActualHeight);
                glaHeight4.To = new GridLength(0);
                glaHeight5.From = new GridLength(this.hidedRow5.ActualHeight);
                glaHeight5.To = new GridLength(0);
                glaHeight6.From = new GridLength(this.hidedRow6.ActualHeight);
                glaHeight6.To = new GridLength(0);
                glaHeight7.From = new GridLength(this.hidedRow7.ActualHeight);
                glaHeight7.To = new GridLength(0);
                glaHeight8.From = new GridLength(this.hidedRow8.ActualHeight);
                glaHeight8.To = new GridLength(0);
                glaHeight9.From = new GridLength(this.hidedRow9.ActualHeight);
                glaHeight9.To = new GridLength(0);
                glaHeight10.From = new GridLength(this.hidedRow10.ActualHeight);
                glaHeight10.To = new GridLength(0);
                glaHeight11.From = new GridLength(this.hidedRow11.ActualHeight);
                glaHeight11.To = new GridLength(0);
            }

            // Specify time duration
            glaHeight1.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            glaHeight2.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            glaHeight3.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            glaHeight4.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            glaHeight5.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            glaHeight6.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            glaHeight7.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            glaHeight8.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            glaHeight9.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            glaHeight10.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            glaHeight11.Duration = new Duration(TimeSpan.FromMilliseconds(300));

            // Apply animation to specified row
            this.hidedRow1.BeginAnimation(RowDefinition.HeightProperty, glaHeight1);
            this.hidedRow2.BeginAnimation(RowDefinition.HeightProperty, glaHeight2);
            this.hidedRow3.BeginAnimation(RowDefinition.HeightProperty, glaHeight3);
            this.hidedRow4.BeginAnimation(RowDefinition.HeightProperty, glaHeight4);
            this.hidedRow5.BeginAnimation(RowDefinition.HeightProperty, glaHeight5);
            this.hidedRow6.BeginAnimation(RowDefinition.HeightProperty, glaHeight6);
            this.hidedRow7.BeginAnimation(RowDefinition.HeightProperty, glaHeight7);
            this.hidedRow8.BeginAnimation(RowDefinition.HeightProperty, glaHeight8);
            this.hidedRow9.BeginAnimation(RowDefinition.HeightProperty, glaHeight9);
            this.hidedRow10.BeginAnimation(RowDefinition.HeightProperty, glaHeight10);
            this.hidedRow11.BeginAnimation(RowDefinition.HeightProperty, glaHeight11);
        }

        /// <summary>
        /// Switch district for weather forecast
        /// </summary>
        private void District_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = e.OriginalSource as MenuItem;
            _country = mi.Header.ToString();
            _city = (mi.Parent as MenuItem).Header.ToString();
            _province = ((mi.Parent as MenuItem).Parent as MenuItem).Header.ToString();
            if (_country == "全市")
            {
                _country = _city;
            }

            _model.GetWeatherInfo(_province, _city, _country); // Get weather info from server
            RealtimeWeatherViewModel rw = _model.GetRealtimeWeatherInfo();
            EnvironmentViewModel ev = _model.GetEnvironmentInfo();
            LifeIndexViewModel li = _model.GetLifeIndexInfo();
            List<WeatherViewModel> wl = _model.GetWeatherForecastInfo();

            this.grdRW.DataContext = rw;
            this.grdEv.DataContext = ev;
            this.grdLi.DataContext = li;
            if (wl != null)
            {
                this.grdYesterday.DataContext = wl[0];
                this.grdToday.DataContext = wl[1];
                this.grdTomorrow.DataContext = wl[2];
                this.grdAfterTomorrow.DataContext = wl[3];
                this.grdThreeDays.DataContext = wl[4];
                this.grdFourDays.DataContext = wl[5];
            }
            else
            {
                WeatherViewModel w = new WeatherViewModel();
                this.grdYesterday.DataContext = w;
                this.grdToday.DataContext = w;
                this.grdTomorrow.DataContext = w;
                this.grdAfterTomorrow.DataContext = w;
                this.grdThreeDays.DataContext = w;
                this.grdFourDays.DataContext = w;
            }
        }

        /// <summary>
        /// Refresh current city's weather info
        /// </summary>
        private void BtnRefreshWeather_Click(object sender, RoutedEventArgs e)
        {
            _model.GetWeatherInfo(_province, _city, _country); // Get weather info from server
            RealtimeWeatherViewModel rw = _model.GetRealtimeWeatherInfo();
            EnvironmentViewModel ev = _model.GetEnvironmentInfo();
            LifeIndexViewModel li = _model.GetLifeIndexInfo();
            List<WeatherViewModel> wl = _model.GetWeatherForecastInfo();

            this.grdRW.DataContext = rw;
            this.grdEv.DataContext = ev;
            this.grdLi.DataContext = li;
            if (wl != null)
            {
                this.grdYesterday.DataContext = wl[0];
                this.grdToday.DataContext = wl[1];
                this.grdTomorrow.DataContext = wl[2];
                this.grdAfterTomorrow.DataContext = wl[3];
                this.grdThreeDays.DataContext = wl[4];
                this.grdFourDays.DataContext = wl[5];
            }
            else
            {
                WeatherViewModel w = new WeatherViewModel();
                this.grdYesterday.DataContext = w;
                this.grdToday.DataContext = w;
                this.grdTomorrow.DataContext = w;
                this.grdAfterTomorrow.DataContext = w;
                this.grdThreeDays.DataContext = w;
                this.grdFourDays.DataContext = w;
            }
        }
    }
}
