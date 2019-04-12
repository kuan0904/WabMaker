using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyTool.ViewModels;
using MyTool.Services;
using System.Reflection;
using MyTool.Commons;

namespace MyTool.Tools
{
    public static class PageTool
    {
        public static PageModel<T> CreatePage<T>(IEnumerable<T> datas, PageParameter param)
        {
            var model = new PageModel<T>
            {
                // 現在頁數
                CurrentPage = param.CurrentPage,
                Data = new List<T>()
            };

            try
            {
                // 排序
                if (param.SortColumn == SortColumn.Random)
                {
                    datas = datas.OrderBy(x => Guid.NewGuid());
                }
                else if(param.SortColumn != SortColumn.Custom)
                {
                    datas = orders(datas, param.SortColumn.ToString(), param.IsDescending);
                }


                // 資料筆數
                model.TotalCount = datas.Count();
                model.PageSize = param.PageSize;

                // 總頁數=總筆數/每頁筆數 (無條件進位)
                model.PageCount = Convert.ToInt32(Math.Ceiling((float)model.TotalCount / param.PageSize));

                // 現在資料顯示範圍
                if (model.TotalCount > 0)
                {
                    model.DataStart = ((model.CurrentPage - 1) * param.PageSize) + 1;
                    model.DataEnd = model.CurrentPage * param.PageSize;
                    if (model.TotalCount < model.DataEnd)
                    {
                        model.DataEnd = model.TotalCount;
                    }
                }

                //不分頁
                if (!param.IsPaged || model.TotalCount <= param.PageSize)
                    model.Data = datas.ToList();

                // 本頁資料 (跳過 每頁筆數*現在頁數)
                else
                    model.Data = datas.Skip(param.PageSize * (model.CurrentPage - 1)).Take(param.PageSize).ToList();

            }
            catch (Exception ex)
            {
                _Log.CreateText("PageTool:" + _Json.ModelToJson(ex));
            }

            return model;
        }

        private static IEnumerable<TSource> orders<TSource>(IEnumerable<TSource> sources, string propertyName, bool isDescending)
        {
            //PropertyInfo propertyInfo = typeof(TSource).GetProperty(propertyName);

            try
            {
                //sources = sources.Where(x => x.GetPropValue(propertyName) != null);

                if (isDescending)
                {
                    return sources.OrderByDescending(x => x.GetPropValue(propertyName) == null)
                                  .ThenByDescending(x => x.GetPropValue(propertyName));
                }
                else
                {
                    return sources.OrderBy(x => x.GetPropValue(propertyName) == null)
                                  .ThenBy(x => x.GetPropValue(propertyName));
                }
            }
            catch (Exception)
            {
                _Log.CreateText("PageTool_orders:" + propertyName);
                throw new Exception(SystemMessage.DataTableSortError);
            }

        }


        /// <summary>
        /// 產生頁碼
        /// </summary>
        /// <param name="model">頁數資料</param>
        /// <param name="buttonCount">按鈕最大數量</param>
        /// <param name="pageType">分頁類型</param>
        /// <returns></returns>
        public static List<PageButtonResultModel> SetPageButton(PageButtonModel model)
        {
            var resut = new List<PageButtonResultModel>();

            //button size
            int size = model.PageCount > model.ButtonMaxSize ? model.ButtonMaxSize : model.PageCount;
            //let current page inside the list
            int i = 1;


            //display all page
            if (model.PageType == PageType.Number)
            {
                size = model.PageCount;
            }
            //move range << >>
            else if (model.PageCount > model.ButtonMaxSize)
            {
                int move = size / 2; // 5/3=2

                // -5 -4 -3 -2 -1 end
                if (model.CurrentPage + move >= model.PageCount)
                {
                    i = model.PageCount - model.ButtonMaxSize + 1;
                }
                // -2 -1 curent +1 +2
                else if (model.CurrentPage - move >= 1)
                {
                    i = model.CurrentPage - move;
                }
            }

            //---------------

            //first
            if (model.PageType == PageType.All)
            {
                resut.Add(new PageButtonResultModel { Title = "First", Name = model.FirstText, Page = 1 });
            }

            //previous
            if (model.PageType == PageType.PreNext
             || model.PageType == PageType.Number_PreNext
             || model.PageType == PageType.All
             || (model.PageType == PageType.Number_PreNextAuto && model.CurrentPage != 1))
            {
                int page = model.CurrentPage - 1;
                page = (page < 1 ? 1 : page); //min = 1
                resut.Add(new PageButtonResultModel { Title = "Previous", Name = model.PreviousText, Page = page });
            }

            //page
            if (model.PageType != PageType.PreNext && model.PageType != PageType.Next)
            {
                while (size-- > 0)
                {
                    resut.Add(new PageButtonResultModel
                    {
                        Title = i.ToString(),
                        Name = i.ToString(),
                        Page = i,
                        IsCurrent = i == model.CurrentPage
                    });
                    i++;
                }
            }

            //next
            if (model.PageType == PageType.PreNext
             || (model.PageType == PageType.Next && model.PageCount > 1 && model.CurrentPage != model.PageCount)
             || model.PageType == PageType.Number_PreNext
             || model.PageType == PageType.All
             || (model.PageType == PageType.Number_PreNextAuto && model.PageCount > 1 && model.CurrentPage != model.PageCount))
            {
                int page = model.CurrentPage + 1;
                page = (page > model.PageCount ? model.PageCount : page); //max = pageCount
                resut.Add(new PageButtonResultModel { Title = "Next", Name = model.NextText, Page = page });
            }

            //last
            if (model.PageType == PageType.All)
            {
                resut.Add(new PageButtonResultModel { Title = "Last", Name = model.LastText, Page = model.PageCount });
            }

            return resut;
        }
    }
}
