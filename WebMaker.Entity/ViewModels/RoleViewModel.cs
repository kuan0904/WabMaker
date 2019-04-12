using MyTool.Enums;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMaker.Entity.Models;

namespace WebMaker.Entity.ViewModels
{
    /// <summary>
    /// 角色與權限列表
    /// </summary>
    public class RoleViewModel
    {
        public mgt_Role Role { get; set; }

        public List<TreeViewModel> MenuCheckList { get; set; }
    }

    /// <summary>
    /// 選單包含權限
    /// </summary>
    public class RolePermissionModel
    {
        public Guid MenuID { get; set; }

        public int ControllerType { get; set; }

        public string RoleActions { get; set; }
    }
}
