using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Enums
{
    /// <summary>
    /// 資料表編號 (系統編號用)
    /// </summary>
    public enum DataTableCode
    {
        //mgt_Client = 1,   
        mgt_SystemSetting = 2,//OK
        //mgt_Menu = 3,   
        mgt_Department = 4,
        mgt_Position = 5,            
        mgt_Role = 6,
        //mgt_RoleMenuRelation = 7,           
        mgt_User = 8, //OK
        //mgt_UserLog = 9,
        //mgt_UserRoleRelation = 10,
        //mgt_UserPositionRelation = 11,
        //mgt_UserExternalLogin = 12,

        cms_Structure = 20,
        cms_Item = 31,
        //cms_ItemFiles = 32,
        //cms_ItemLanguage = 33,
        //cms_ItemRelation = 34,

    }
}
