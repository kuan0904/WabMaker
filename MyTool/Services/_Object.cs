using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Services
{
    public static class _Object
    {
        /// <summary>
        /// null轉為空字串
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>string</returns>
        public static string FieldToString(this object field)
        {
            return field == null ? string.Empty : field.ToString().Trim();
        }

        public static List<T> ToListObject<T>(this T obj)
        {
            return new List<T> { obj };
        }
    }
}
