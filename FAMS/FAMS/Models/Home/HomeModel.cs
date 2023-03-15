using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.IO.Compression;
using System.Xml.Serialization;
using FAMS.Services;
using FAMS.Data.Home;
using FAMS.ViewModels.Home;
using System.Windows.Controls;

namespace FAMS.Models.Home
{
    class HomeModel
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper();
        private CLogWriter _logWriter = CLogWriter.GetInstance();
        private resp _weatherInfo = new resp();

        /// <summary>
        /// Constructor.
        /// </summary>
        public HomeModel()
        {
            // Initialize *.fams file helper (fix system config file path)
            _ffHelper.Init(Directory.GetCurrentDirectory() + "\\config\\system.fams");

            // Get log file info
            string logDir = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "log_dir");
            string logName = _ffHelper.GetData("config", "log_name");

            // Initialize log writer
            if (!System.IO.Directory.Exists(logDir))
            {
                System.IO.Directory.CreateDirectory(logDir);
            }
            _logWriter.Init(logDir + "\\", logName);
            _logWriter.SetCurrLevel(5);
        }

        /// <summary>
        /// Load districts
        /// </summary>
        /// <returns>province list</returns>
        public List<MenuItem> LoadDistricts()
        {
            string fileName = Directory.GetCurrentDirectory() + "\\Xmls\\WeatherForecastCityCodes.xml";
            if (!File.Exists(fileName))
            {
                _logWriter.WriteErrorLog("HomeModel::LoadDistricts >> XML file not found!");
                return new List<MenuItem>();
            }

            /*
             * Read xml file by XmlReader to prevent comments among xml codes being read in.
             * Conventional way of xml reading such an "doc.Load(fileName)" will read in all
             * InnerText including comment contents.
             */
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(fileName, settings);
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            List<MenuItem> districtList = new List<MenuItem>();

            foreach (XmlNode province in doc.SelectSingleNode("China").SelectNodes("province"))
            {
                MenuItem miProvince = new MenuItem();
                miProvince.Header = province.Attributes[1].Value;

                foreach (XmlNode city in province.SelectNodes("city"))
                {
                    MenuItem miCity = new MenuItem();
                    miCity.Header = city.Attributes[1].Value;

                    foreach (XmlNode country in city.SelectNodes("country"))
                    {
                        MenuItem miCountry = new MenuItem();
                        miCountry.Header = country.Attributes[1].Value;
                        miCity.Items.Add(miCountry);
                    }

                    miProvince.Items.Add(miCity);
                }

                districtList.Add(miProvince);
            }

            return districtList;
        }

        #region Event handlers
        /// <summary>
        /// Get weather info.
        /// </summary>
        /// <param name="cityCode"></param>
        /// <history time="2018/01/30">create this method</history>
        public void GetWeatherInfo(string province, string city, string country = null)
        {
            // Get city code.
            string cityCode = null;
            cityCode = GetCityCode(province, city, country);
            if (cityCode == null)
            {
                return;
            }

            // Get weather info.
            string url = "http://wthrcdn.etouch.cn/WeatherApi?citykey=" + cityCode;
            string weatherInfoStr = GetWeatherInfoStr(url);
            if (weatherInfoStr != null)
            {
                _weatherInfo = XmlDeserializer<resp>(weatherInfoStr);
            }
        }

        /// <summary>
        /// Get real-time weather info.
        /// </summary>
        /// <history time="2018/02/02">create this method</history>
        /// <returns>real-time weather view model</returns>
        public RealtimeWeatherViewModel GetRealtimeWeatherInfo()
        {
            RealtimeWeatherViewModel vm = new RealtimeWeatherViewModel();

            vm.District = " " + _weatherInfo.city;
            vm.UpdateTime = "     " + _weatherInfo.updatetime + " 更新";
            vm.LunarCalendarDate = ChineseCalendar.GetChinaDate(DateTime.Now);
            vm.Temperature = _weatherInfo.wendu + "℃";

            List<weather> forecast = _weatherInfo.forecast;
            if (forecast != null)
            {
                vm.TemperatureRange = forecast[0].low.Substring(3, forecast[0].low.Length - 4) + "~"
                + forecast[0].high.Substring(3, forecast[0].high.Length - 3);

                brief day = forecast[0].day;
                brief night = forecast[0].night;
                vm.WeatherChange = day.type == night.type ? day.type : (day.type + "转" + night.type);
            }

            vm.Wind = _weatherInfo.fengxiang + " " + _weatherInfo.fengli;
            vm.Humidity = "湿度" + _weatherInfo.shidu;
            vm.Sunrise = _weatherInfo.sunrise_1 + "日出";
            vm.Sunset = _weatherInfo.sunset_1 + "日落";

            return vm;
        }

        /// <summary>
        /// Get environment info.
        /// </summary>
        /// <history time="2018/02/02">create this method</history>
        /// <returns>environment info view model</returns>
        public EnvironmentViewModel GetEnvironmentInfo()
        {
            EnvironmentViewModel vm = new EnvironmentViewModel();

            environment e = _weatherInfo.environment;
            if (e == null)
            {
                return null;
            }

            vm.AirQuality = "  " + e.quality;
            vm.Suggest = "  " + e.suggest;

            if (e.MajorPollutants != null && e.MajorPollutants != "")
            {
                string[] p = e.MajorPollutants.Split(' ');
                for (int i = 0; i < p.Length; ++i)
                {
                    if (p[i] != "")
                    {
                        vm.MajorPollutants += i == 0 ? "  " : "";
                        vm.MajorPollutants += p[i] + ";";
                    }
                }
                vm.MajorPollutants = vm.MajorPollutants.Substring(0, vm.MajorPollutants.Length - 1);
            }

            vm.AQI = "  " + e.aqi;
            vm.PM25 = "  " + e.pm25;
            vm.PM10 = "  " + e.pm10;
            vm.O3 = "  " + e.o3;
            vm.CO = "  " + e.co;
            vm.SO2 = "  " + e.so2;
            vm.NO2 = "  " + e.no2;
            vm.UpdateTime = "  " + e.time;

            return vm;
        }

        /// <summary>
        /// Get life index info.
        /// </summary>
        /// <history time="2018/02/02">create this method</history>
        /// <returns>life index info view model</returns>
        public LifeIndexViewModel GetLifeIndexInfo()
        {
            LifeIndexViewModel vm = new LifeIndexViewModel();

            List<zhishu> indexes = _weatherInfo.zhishus;
            if (indexes == null)
            {
                return null;
            }

            if (indexes.Count > 0)
            {
                vm.MorningExerciseIndex = "  " + indexes[0].value;
                vm.MorningExerciseDetail = "  " + indexes[0].detail;
            }
            else
            {
                return vm;
            }

            if (indexes.Count > 1)
            {
                vm.ComfortIndex = "  " + indexes[1].value;
                vm.ComfortDetail = "  " + indexes[1].detail;
            }
            else
            {
                return vm;
            }

            if (indexes.Count > 2)
            {
                vm.DressingIndex = "  " + indexes[2].value;
                vm.DressingDetail = "  " + indexes[2].detail;
            }
            else
            {
                return vm;
            }

            if (indexes.Count > 3)
            {
                vm.CatchingColdIndex = "  " + indexes[3].value;
                vm.CatchingColdDetail = "  " + indexes[3].detail;
            }
            else
            {
                return vm;
            }

            if (indexes.Count > 4)
            {
                vm.SunDryingIndex = "  " + indexes[4].value;
                vm.SunDryingDetail = "  " + indexes[4].detail;
            }
            else
            {
                return vm;
            }

            if (indexes.Count > 5)
            {
                vm.TravelIndex = "  " + indexes[5].value;
                vm.TravelDetail = "  " + indexes[5].detail;
            }
            else
            {
                return vm;
            }

            if (indexes.Count > 6)
            {
                vm.UVIndex = "  " + indexes[6].value;
                vm.UVDetail = "  " + indexes[6].detail;
            }
            else
            {
                return vm;
            }

            if (indexes.Count > 7)
            {
                vm.CarWashingIndex = "  " + indexes[7].value;
                vm.CarWashingDetail = "  " + indexes[7].detail;
            }
            else
            {
                return vm;
            }

            if (indexes.Count > 8)
            {
                vm.SportIndex = "  " + indexes[8].value;
                vm.SportDetail = "  " + indexes[8].detail;
            }
            else
            {
                return vm;
            }

            if (indexes.Count > 9)
            {
                vm.DatingIndex = "  " + indexes[9].value;
                vm.DatingDetail = "  " + indexes[9].detail;
            }
            else
            {
                return vm;
            }

            if (indexes.Count > 10)
            {
                vm.UmbrellaIndex = "  " + indexes[10].value;
                vm.UmbrellaDetail = "  " + indexes[10].detail;
            }
            else
            {
                return vm;
            }

            return vm;
        }

        /// <summary>
        /// Get weather forecast info.
        /// </summary>
        /// <history time="2018/02/03">create this method</history>
        /// <returns>weather forecast info view model list</returns>
        public List<WeatherViewModel> GetWeatherForecastInfo()
        {
            List<WeatherViewModel> weatherList = new List<WeatherViewModel>();

            yesterday yd = _weatherInfo.yesterday;
            List<weather> wl = _weatherInfo.forecast;
            if (yd == null || wl == null)
            {
                return null;
            }

            // Yesterday.
            WeatherViewModel w = new WeatherViewModel();
            w.Week = "昨天（周" + yd.date_1.Substring(yd.date_1.Length - 1, 1) + "）";
            w.Date = DateTime.Now.AddDays(-1).ToString("MM月dd日");
            w.TemperatureRange = yd.low_1.Substring(3, yd.low_1.Length - 4) + "~" + yd.high_1.Substring(3, yd.high_1.Length - 3);
            w.DayWeather = yd.day_1.type_1;
            w.DayWind = yd.day_1.fx_1 + " " + yd.day_1.fl_1;
            w.NightWeather = yd.night_1.type_1;
            w.NightWind = yd.night_1.fx_1 + " " + yd.night_1.fl_1;
            weatherList.Add(w);

            // Today.
            w = new WeatherViewModel();
            w.Week = "今天（周" + wl[0].date.Substring(wl[0].date.Length - 1, 1) + "）";
            w.Date = DateTime.Now.ToString("MM月dd日");
            w.TemperatureRange = wl[0].low.Substring(3, wl[0].low.Length - 4) + "~" + wl[0].high.Substring(3, wl[0].high.Length - 3);
            w.DayWeather = wl[0].day.type;
            w.DayWind = wl[0].day.fengxiang + " " + wl[0].day.fengli;
            w.NightWeather = wl[0].night.type;
            w.NightWind = wl[0].night.fengxiang + " " + wl[0].night.fengli;
            weatherList.Add(w);

            // Tomorrow.
            w = new WeatherViewModel();
            w.Week = "明天（周" + wl[1].date.Substring(wl[1].date.Length - 1, 1) + "）";
            w.Date = DateTime.Now.AddDays(1).ToString("MM月dd日");
            w.TemperatureRange = wl[1].low.Substring(3, wl[1].low.Length - 4) + "~" + wl[1].high.Substring(3, wl[1].high.Length - 3);
            w.DayWeather = wl[1].day.type;
            w.DayWind = wl[1].day.fengxiang + " " + wl[1].day.fengli;
            w.NightWeather = wl[1].night.type;
            w.NightWind = wl[1].night.fengxiang + " " + wl[1].night.fengli;
            weatherList.Add(w);

            // The day after tomorrow.
            w = new WeatherViewModel();
            w.Week = "周" + wl[2].date.Substring(wl[2].date.Length - 1, 1);
            w.Date = DateTime.Now.AddDays(2).ToString("MM月dd日");
            w.TemperatureRange = wl[2].low.Substring(3, wl[2].low.Length - 4) + "~" + wl[2].high.Substring(3, wl[2].high.Length - 3);
            w.DayWeather = wl[2].day.type;
            w.DayWind = wl[2].day.fengxiang + " " + wl[2].day.fengli;
            w.NightWeather = wl[2].night.type;
            w.NightWind = wl[2].night.fengxiang + " " + wl[2].night.fengli;
            weatherList.Add(w);

            // Three days from now.
            w = new WeatherViewModel();
            w.Week = "周" + wl[3].date.Substring(wl[3].date.Length - 1, 1);
            w.Date = DateTime.Now.AddDays(3).ToString("MM月dd日");
            w.TemperatureRange = wl[3].low.Substring(3, wl[3].low.Length - 4) + "~" + wl[3].high.Substring(3, wl[3].high.Length - 3);
            w.DayWeather = wl[3].day.type;
            w.DayWind = wl[3].day.fengxiang + " " + wl[3].day.fengli;
            w.NightWeather = wl[3].night.type;
            w.NightWind = wl[3].night.fengxiang + " " + wl[3].night.fengli;
            weatherList.Add(w);

            // Four days from now.
            w = new WeatherViewModel();
            w.Week = "周" + wl[4].date.Substring(wl[4].date.Length - 1, 1);
            w.Date = DateTime.Now.AddDays(4).ToString("MM月dd日");
            w.TemperatureRange = wl[4].low.Substring(3, wl[4].low.Length - 4) + "~" + wl[4].high.Substring(3, wl[4].high.Length - 3);
            w.DayWeather = wl[4].day.type;
            w.DayWind = wl[4].day.fengxiang + " " + wl[4].day.fengli;
            w.NightWeather = wl[4].night.type;
            w.NightWind = wl[4].night.fengxiang + " " + wl[4].night.fengli;
            weatherList.Add(w);

            return weatherList;
        }
        #endregion

        #region Model functions
        /// <summary>
        /// Get city code used for weather forecast.
        /// </summary>
        /// <param name="province"></param>
        /// <param name="city"></param>
        /// <param name="country"></param>
        /// <history time="2018/01/30">create this method</history>
        /// <returns>city code</returns>
        private string GetCityCode(string province, string city, string country = null)
        {
            string fileName = Directory.GetCurrentDirectory() + "\\Xmls\\WeatherForecastCityCodes.xml";
            if (!File.Exists(fileName))
            {
                _logWriter.WriteErrorLog("HomeModel::GetCityCode >> XML file not found!");
                return null;
            }

            /*
             * Read xml file by XmlReader to prevent comments among xml codes being read in.
             * Conventional way of xml reading such an "doc.Load(fileName)" will read in all
             * InnerText including comment contents.
             */
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(fileName, settings);
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            foreach (XmlNode node in doc.SelectSingleNode("China").SelectNodes("province"))
            {
                if (node.Attributes["name"].Value == province)
                {
                    foreach (XmlNode child1 in node.SelectNodes("city"))
                    {
                        if (child1.Attributes["name"].Value == city)
                        {
                            foreach (XmlNode child2 in child1.SelectNodes("country"))
                            {
                                if (child2.Attributes["name"].Value == (country == null ? city : country))
                                {
                                    return child2.Attributes["cityCode"].Value;
                                }
                            }
                        }
                    }
                }
            }
            _logWriter.WriteInfoLog("HomeModel::GetCityCode >> No city code found!");

            return null;
        }

        /// <summary>
        /// Get weather info string
        /// </summary>
        /// <param name="url">url to server</param>
        /// <history time="2018/01/30">create this method</history>
        /// <history time="2018/02/11">solve the problem that occurs when this app disconnect to the Internet</history>
        /// <returns>weather info string</returns>
        private string GetWeatherInfoStr(string url)
        {
            StringBuilder sb = new StringBuilder(102400);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)wr.GetResponse();
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("HomeModel::GetWeatherInfoStr >> " + ex.Message);
                return null;
            }
            GZipStream g = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
            byte[] d = new byte[20480];
            int l = g.Read(d, 0, 20480);
            while (l > 0)
            {
                sb.Append(Encoding.UTF8.GetString(d, 0, l));
                l = g.Read(d, 0, 20480);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Deserialize XML string
        /// </summary>
        /// <typeparam name="T">specified type</typeparam>
        /// <param name="xmlStr">XML string</param>
        /// <history time="2018/01/30">create this method</history>
        /// <returns>data of specified type</returns>
        public T XmlDeserializer<T>(string xmlStr) where T : class, new()
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xmlStr))
            {
                return xs.Deserialize(reader) as T;
            }
        }
        #endregion Model functions
    }
}
