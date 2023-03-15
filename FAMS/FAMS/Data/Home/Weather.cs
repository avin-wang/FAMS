using System.Collections.Generic;

namespace FAMS.Data.Home
{
    // Weather info response from server http://wthrcdn.etouch.cn/WeatherApi?citykey=
    public class resp
    {
        public string city;             // 城市
        public string updatetime;       // 更新时间
        public string wendu;            // 温度
        public string fengli;           // 风力
        public string shidu;            // 湿度(%)
        public string fengxiang;        // 风向
        public string sunrise_1;        // 日出1
        public string sunset_1;         // 日落1
        public string sunrise_2;        // 日出2
        public string sunset_2;         // 日落2
        public environment environment; // 环境信息
        public yesterday yesterday;     // 昨天天气
        public List<weather> forecast;  // 未来5天天气预报
        public List<zhishu> zhishus;    // 一些指数
    }

    // Environment info.
    public class environment
    {
        public string aqi;             // AQI（空气质量指数）
        public string pm25;            // PM2.5（细颗粒物，即粒径<=2.5um的颗粒物）
        public string suggest;         // 建议
        public string quality;         // 空气质量
        public string MajorPollutants; // 主要污染物
        public string o3;              // O3(臭氧)
        public string co;              // CO(一氧化碳)
        public string pm10;            // PM10（可吸入颗粒物，即粒径<=10um的颗粒物）
        public string so2;             // SO2(二氧化硫)
        public string no2;             // NO2(二氧化氮)
        public string time;            // 更新时间
    }

    // Yesterday weather.
    public class yesterday
    {
        public string date_1;   // 日期（e.g., "3日星期六"）
        public string high_1;   // 最高气温
        public string low_1;    // 最低气温
        public brief_1 day_1;   // 白天气候
        public brief_1 night_1; // 晚上气候
    }

    // Brief info of yesterday's weather.
    public class brief_1
    {
        public string type_1; // 天气描述，如小雨、多云等
        public string fx_1;   // 风向
        public string fl_1;   // 风力
    }

    // Weather forecasting info.
    public class weather
    {
        public string date;   // 日期（e.g., "3日星期六"）
        public string high;   // 最高气温
        public string low;    // 最低气温
        public brief day;     // 白天气候
        public brief night;   // 晚上气候
    }

    // Brief info of future weather.
    public class brief
    {
        public string type;      // 天气描述，如小雨、多云等
        public string fengxiang; // 风向
        public string fengli;    // 风力
    }

    // Indexes' info.
    public class zhishu
    {
        public string name;   // 指数名称
        public string value;  // 指数值
        public string detail; // 详情
    }
}
