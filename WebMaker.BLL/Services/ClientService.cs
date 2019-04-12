using MyTool.Commons;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using WebMaker.Entity.Models;

namespace WebMaker.BLL.Services
{
    /// <summary>
    /// 客戶管理
    /// </summary>
    public class ClientService : BaseService
    {
        private IQueryable<mgt_Client> Query
        {
            get
            {
                return Db.mgt_Client
                    .Where(x => !x.IsDelete);
            }
        }

        public mgt_Client Get(Guid id)
        {
            return Query.FirstOrDefault(x => x.ID == id);
        }

        public CiResult<mgt_Client> GetByAdminRoute(string AdminRoute)
        {
            CiResult<mgt_Client> result = new CiResult<mgt_Client>();

            //路由名稱不分大小寫
            var query = Query.FirstOrDefault(x => x.AdminRoute.ToLower() == AdminRoute.ToLower() && x.IsEnabled);
            if (query == null)
            {
                result.Message = SystemMessage.AdminRouteError;
            }
            else
            {
                result.Data = query;
                result.IsSuccess = true;
            }

            return result;
        }

    }
}
