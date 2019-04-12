using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Enums
{
    /// <summary>
    /// 結構包含功能
    /// </summary>
    public enum ToolType
    {
        [Display(Name = "只允許最高權限")]
        OnlyHighestAuthority = 1
    }
}
