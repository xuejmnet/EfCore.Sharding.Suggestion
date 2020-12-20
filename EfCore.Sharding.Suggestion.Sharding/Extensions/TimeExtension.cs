using System;

namespace EfCore.Sharding.Suggestion.Sharding.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 23:13:27
* @Email: 326308290@qq.com
*/
    public static class TimeExtension
    {
        /// <summary>  
        /// 将DateTime时间格式转换为本地时间戳格式
        /// </summary>
        /// Author  : Napoleon
        /// Created : 2018/4/8 14:19
        public static long ConvertTimeToLong(this DateTime time)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long) (time.AddHours(-8) - start).TotalMilliseconds;
        }

        /// <summary>        
        /// 本地时间戳转为C#格式时间
        /// </summary>
        /// Author  : Napoleon
        /// Created : 2018/4/8 14:19
        public static DateTime ConvertLongToTime(this long timeStamp)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddMilliseconds(timeStamp).AddHours(8);
        }
        /// <summary>
        /// 获取周一
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetMonday(this DateTime dateTime)
        {
            DateTime temp = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
            int count = dateTime.DayOfWeek - DayOfWeek.Monday;
            if (count == -1) count = 6;
            var monday = temp.AddDays(-count);
            return monday;
        }

        /// <summary>
        /// 获取周日
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetSunday(this DateTime dateTime)
        {
            DateTime now = DateTime.Now;
            DateTime temp = new DateTime(now.Year, now.Month, now.Day);
            int count = now.DayOfWeek - DayOfWeek.Sunday;
            if (count != 0) count = 7 - count;

            var sunday = temp.AddDays(count);
            return sunday;
        }
        
    }
}