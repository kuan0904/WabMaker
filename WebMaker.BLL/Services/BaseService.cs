using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMaker.Entity.Models;

namespace WebMaker.BLL.Services
{
    public class BaseService : IDisposable
    {
        protected WebMakerDBEntities Db = new WebMakerDBEntities();

        public Guid ClientID { get; set; }
        public string ClientCode { get; set; }
        public bool IsSuperManager { get; set; }
        //public LanguageType DefaultLanguage = LanguageType.Chinese;

        #region 系統編號
        /// <summary>
        /// 取得系統編號
        /// </summary>
        /// <returns></returns>
        protected CiResult<string> CreateSystemNumber(DataTableCode dataTable)
        {
            //系統編號=客戶(3碼)_資料表(3碼)_日期(6碼)流水號(6碼)
            var result = new CiResult<string>();

            //客戶(3碼) 
            //null return
            if (string.IsNullOrEmpty(ClientCode))
            {
                result.Message = SystemMessage.ClientError;
                return result;
            }

            try
            {
                var date = DateTime.Now;

                //資料表(3碼)
                var tableCode = ((int)dataTable).ToString().PadLeft(3, '0');
                //水號(6碼)
                var count = GetCodeCount(dataTable, date);
                var number = (count + 1).ToString().PadLeft(6, '0');
                var dateCode = date.ToString("yyMMdd");
                var systemNumber = $"{ClientCode}_{tableCode}_{dateCode}{number}";

                //repeat
                if (!CheckCode(dataTable, systemNumber))
                {
                    long maxNumberInt = 0;
                    var maxNumber = GetCodeMax(dataTable, date).Substring(8);

                    if (long.TryParse(maxNumber, out maxNumberInt))
                    {
                        systemNumber = $"{ClientCode}_{tableCode}_{(maxNumberInt + 1).ToString()}";
                    }

                    //still repeat
                    if (!CheckCode(dataTable, systemNumber))
                    {
                        systemNumber = "";
                    }
                }

                //check
                if (systemNumber.ToTrim().Length == 20)
                {
                    result.Data = systemNumber;
                    result.IsSuccess = true;
                }
                else
                {
                    _Log.CreateText("CreateSystemNumber: error " + systemNumber);
                }
            }
            catch (Exception ex)
            {
                _Log.CreateText("CreateSystemNumber:" + _Json.ModelToJson(ex));
            }

            if (!result.IsSuccess)
            {
                result.Message = SystemMessage.SystemNumberError;
            }

            return result;
        }

        /// <summary>
        /// SystemNumber-取得資料數量 同Client、同日期 
        /// </summary>
        /// <returns></returns>
        private int GetCodeCount(DataTableCode dataTable, DateTime date)
        {
            var sqlStr = $"select count(*) from {dataTable.ToString()} "
                       + $"where ClientID = '{ClientID}' "
                       + $"and convert(varchar(10),CreateTime,111)='{date.ToString("yyyy/MM/dd")}'";

            return Db.Database.SqlQuery<int>(sqlStr).First();
        }

        private string GetCodeMax(DataTableCode dataTable, DateTime date)
        {
            var sqlStr = $"select Max(SystemNumber) from {dataTable.ToString()} "
                       + $"where ClientID = '{ClientID}' "
                       + $"and convert(varchar(10),CreateTime,111)='{date.ToString("yyyy/MM/dd")}'";

            return Db.Database.SqlQuery<string>(sqlStr).First();
        }

        /// <summary>
        /// SystemNumber-確認無重複
        /// </summary>
        /// <param name="systemNumber">The system number.</param>
        /// <returns></returns>
        private bool CheckCode(DataTableCode dataTable, string systemNumber)
        {
            var sqlStr = $"select count(*) from {dataTable.ToString()} "
                       + $"where SystemNumber = '{systemNumber} '";

            var data = Db.Database.SqlQuery<int>(sqlStr).First();
            return data == 0;
        }

        #endregion

        protected void EntityUpdate<T>(T model, List<string> notUpdateColumn = null) where T : class
        {
            var entry = Db.Entry(model);

            //傳進來的實體物件是否有在 context 裡
            if (entry.State == EntityState.Detached)
            {
                //判斷目前 context 的 DbSet<T> 裡是否有相同索引鍵的物件                
                var set = Db.Set<T>();
                var attachedEntity = set.Local.SingleOrDefault(e => (Guid)e.GetPropValue("ID") == (Guid)model.GetPropValue("ID"));

                if (attachedEntity != null)
                {
                    //把傳進來的實體物件資料放進已經存在 context 的相同索引鍵物件裡
                    entry = Db.Entry(attachedEntity);
                    entry.CurrentValues.SetValues(model);
                }
                else
                {
                    //將 entry 的狀態設定為 EntityState.Modified
                    entry.State = EntityState.Modified; // This should attach entity                          
                }
            }

            //not update  
            if (notUpdateColumn != null)
            {
                foreach (var item in notUpdateColumn)
                {
                    entry.Property(item).IsModified = false;
                }
            }

            Db.SaveChanges();
        }

        /// <summary>
        /// Dipose
        /// </summary>
        public void Dispose()
        {
            Db.Dispose();
        }
    }
}
