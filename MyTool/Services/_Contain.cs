using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Services
{
    /// <summary>
    /// 是否包含值 000,001,..999
    /// </summary>
    public static class _Contain
    {
        /// <summary>
        /// 是否有此值
        /// </summary>
        /// <param name="sourceValue">來源值</param>
        /// <param name="value">檢查值</param>
        /// <returns></returns>
        public static bool HasValue(this string sourceValue, int value)
        {
            return !string.IsNullOrEmpty(sourceValue) && sourceValue.Contains(value.ToContainStr());
        }

        public static string ToContainStr(this int value)
        {

            return value.ToString("000");
        }
    }
}
