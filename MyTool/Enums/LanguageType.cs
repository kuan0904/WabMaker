using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Enums
{
    /// <summary>
    /// 語系 (in Client, Item)
    /// </summary>
    public enum LanguageType
    {        
        [Display(Name = "繁體中文", ShortName = ("繁"))]
        Chinese = 1,
            
        [Display(Name = "English", ShortName = ("EN"))]
        English = 2,
              
        [Display(Name = "简体中文", ShortName = ("简"))]
        Japanese = 3
    }

    public enum LanguageStatus
    {
        None = 0,
        Show = 1,
        Hide = 2
    }
}
