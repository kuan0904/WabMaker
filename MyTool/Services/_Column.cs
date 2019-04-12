using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Tools
{
    public static class _Column
    {
        /// <summary>
        /// 取得欄位 by ColumnName
        /// </summary>
        /// <example>
        ///  .Select(e => GetPropValue(e, columnName))
        /// </example>
        public static object GetPropValue(this object obj, string propName)
        {
            var value = obj.GetType().GetProperty(propName).GetValue(obj, null);
            return value;
        }


    }
}
