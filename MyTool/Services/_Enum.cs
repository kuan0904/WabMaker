using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyTool.Services
{
    public static class _Enum
    {
        /// <summary>
        /// 取得列舉顯示名稱 [Display(Name = "")]
        /// </summary>
        /// <typeparam name="T">列舉類型</typeparam>
        /// <param name="i">列舉值</param>
        /// <returns></returns>
        public static string GetDisplayName<T>(this int i)
        {
            try
            {
                return GetAttribute<T, DisplayAttribute>(i).Name;

            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetDisplayName(this Enum obj)
        {
            try
            {
                return GetAttribute<DisplayAttribute>(obj).Name;

            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 取得列舉描述  [Description("")]
        /// </summary>
        /// <typeparam name="T">列舉類型</typeparam>
        /// <param name="i">列舉值</param>
        /// <returns></returns>
        public static string GetDescription<T>(this int i)
        {
            try
            {
                return GetAttribute<T, DescriptionAttribute>(i).Description;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetDescription(this Enum obj)
        {
            try
            {
                return GetAttribute<DescriptionAttribute>(obj).Description;
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 取得列舉短名稱
        /// </summary>
        /// <typeparam name="T">列舉類型</typeparam>
        /// <param name="i">列舉值</param>
        /// <returns></returns>
        public static string GetShortName<T>(this int i)
        {
            try
            {
                return GetAttribute<T, DisplayAttribute>(i).ShortName;

            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetShortName(this Enum obj)
        {
            try
            {
                return GetAttribute<DisplayAttribute>(obj).ShortName;

            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// DisplayName轉為int (字串包含Enum名稱)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str">The string.</param>
        /// <param name="isEqual">是否完全相等</param>
        /// <returns></returns>
        public static int EnumNameToInt<T>(string str, bool isEqual = false)
        {
            foreach (int i in Enum.GetValues(typeof(T)))
            {
                string name = i.GetDisplayName<T>();
                if (isEqual && str.Equals(name) || !isEqual && str.Contains(name))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// ShortName轉為int (字串包含Enum名稱)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static int ShortNameToInt<T>(string str)
        {
            foreach (int i in Enum.GetValues(typeof(T)))
            {
                string name = i.GetShortName<T>();
                if (str.Equals(name))
                {
                    return i;
                }
            }
            return -1;
        }

        #region  取得列舉自訂屬性

        /// <summary>
        /// 取得列舉自訂屬性
        /// </summary>
        /// <typeparam name="T">列舉類型</typeparam>
        /// <typeparam name="TAttribute">屬性</typeparam>
        /// <param name="i">列舉值</param>
        /// <returns></returns>
        public static TAttribute GetAttribute<T, TAttribute>(this int i)
        {
            var e = Enum.Parse(typeof(T), Convert.ToString(i));

            return ((Enum)e).GetAttribute<TAttribute>();
        }

        public static TAttribute GetAttribute<TAttribute>(this Enum obj)
        {
            string objName = obj.ToString();
            var t = obj.GetType();
            FieldInfo fi = t.GetField(objName);
            TAttribute[] attributes = fi.GetCustomAttributes(typeof(TAttribute), false) as TAttribute[];

            return attributes[0];
        }

        #endregion

        /// <summary>
        /// int to EnumString
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public static string ToEnumString<T>(this int i)
        {
            if (Enum.IsDefined(typeof(T), i))
            {
                return Enum.ToObject(typeof(T), i).ToString();
            }
            return "";
        }

        /// <summary>
        /// 列舉的下拉選單: Name (DisplayName)
        /// </summary>     
        /// <typeparam name="T">列舉類別</typeparam>
        /// <param name="value">selected value</param>
        /// <param name="remove">排除值</param>
        /// <returns></returns>
        public static List<SelectListItem> EnumtoSelectAndName<T>(int value = -1, List<int> remove = null)
        {
            // 系統內建 EnumHelper.GetSelectList
            var selectList = new List<SelectListItem>();

            foreach (int i in Enum.GetValues(typeof(T)))
            {
                if (remove != null && remove.Contains(i)) continue;

                selectList.Add(
                    new SelectListItem()
                    {
                        Text = $"{Enum.GetName(typeof(T), i)} ({GetDisplayName<T>(i)})",
                        Value = i.ToString(),
                        Selected = (i == value)
                    });
            }

            return selectList;
        }

        public static List<SelectListItem> EnumtoSelect<T>(int value = -1)
        {
            // 系統內建 EnumHelper.GetSelectList
            var selectList = new List<SelectListItem>();

            foreach (int i in Enum.GetValues(typeof(T)))
            {
                selectList.Add(
                    new SelectListItem()
                    {
                        Text = GetDisplayName<T>(i),
                        Value = i.ToString(),
                        Selected = (i == value)
                    });
            }

            return selectList;
        }

        public static List<SelectListItem> EnumtoSelect<T>(List<T> list, int value = -1)
        {
            var selectList = new List<SelectListItem>();

            foreach (var i in list)
            {
                int x = Convert.ToInt32(i);
                selectList.Add(
                    new SelectListItem()
                    {
                        Text = GetDisplayName<T>(x),
                        Value = x.ToString(),
                        Selected = (x == value)
                    });
            }

            return selectList;
        }

        /// <summary>
        /// 是否包含Check
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sum">The sum.</param>
        /// <returns></returns>
        public static List<SelectListItem> ToContainCheck<T>(string sum)
        {
            var selectList = new List<SelectListItem>();

            foreach (int type in Enum.GetValues(typeof(T)))
            {
                var value = type.ToContainStr();
                var isChecked = sum.HasValue(type);

                selectList.Add(
                  new SelectListItem()
                  {
                      Text = GetDisplayName<T>(type),
                      Value = value.ToString(),
                      Selected = isChecked
                  });
            }

            return selectList;
        }

        public static List<SelectListItem> ToRadioList<T>(long value = -1)
        {
            var selectList = new List<SelectListItem>();

            foreach (int type in Enum.GetValues(typeof(T)))
            {
                var isChecked = type == value;

                selectList.Add(
                  new SelectListItem()
                  {
                      Text = GetDisplayName<T>(type),
                      Value = type.ToString(),
                      Selected = isChecked
                  });
            }

            return selectList;
        }

        /// <summary>
        /// List Contain (包含的項目名稱)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> ToContainStrList<T>(this string sum)
        {
            var list = new List<string>();

            foreach (int type in Enum.GetValues(typeof(T)))
            {
                var isChecked = sum.HasValue(type);

                if (isChecked)
                {
                    list.Add(GetDisplayName<T>(type));
                }
            }

            return list;
        }
        public static List<T> ToContainList<T>(this string sum)
        {
            var list = new List<T>();

            foreach (var type in Enum.GetValues(typeof(T)))
            {
                var isChecked = sum.HasValue((int)type);

                if (isChecked)
                {
                    list.Add((T)type);
                }
            }

            return list;
        }
    }
}
