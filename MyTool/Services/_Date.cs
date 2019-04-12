using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Services
{
    public static class _Date
    {
        public static string Now()
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public static long NowTn()
        {
            return Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
        }

        public static string Today()
        {
            return DateTime.Now.ToString("yyyy/MM/dd");
        }

        public static int TodayDn()
        {
            return Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
        }

        public static string ToDateString(this DateTime date, string formatStr = "yyyy/MM/dd")
        {
            return date.ToString(formatStr) ?? "";
        }

        public static string ToDateString(this DateTime? date, string formatStr = "yyyy/MM/dd")
        {
            return date?.ToString(formatStr) ?? "";
        }

        /// <summary>
        /// 計算 2 個日期之間的天數
        /// </summary>
        public static int DateDiff(DateTime pd_start, DateTime pd_end)
        {
            if (pd_start == pd_end)
            {
                return 0;
            }
            else
            {
                TimeSpan t_ts = pd_end - pd_start;
                return t_ts.Days;
            }
        }

        public static int DateDiff(string ps_start, string ps_end)
        {
            if (ps_start == ps_end)
            {
                return 0;
            }

            DateTime td_start = DateTime.Parse(ps_start);
            DateTime td_end = DateTime.Parse(ps_end);
            return DateDiff(td_start, td_end);
        }

        public static int Age(DateTime? birthday)
        {
            return birthday == null ? -1 : (DateTime.Now.Year - birthday.Value.Year);
        }
    }
}
