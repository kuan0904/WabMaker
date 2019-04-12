using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMaker.Entity.Models
{
    [MetadataType(typeof(mgt_SystemSettingMetaData))]
    public partial class mgt_SystemSetting
    {
        public partial class mgt_SystemSettingMetaData
        {
            /// <summary>
            /// 系統設定ID
            /// </summary>
            public int SeqNo { get; set; }

            /// <summary>
            /// 客戶ID
            /// </summary>
            public Guid ClientID { get; set; }

            /// <summary>
            /// 系統編號 char(25)
            /// </summary>
            public string SystemNumber { get; set; }

            /// <summary>
            /// 設定類型
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// 內容json
            /// </summary>
            public string SettingText { get; set; }

            /// <summary>
            /// 更新時間
            /// </summary>
            public DateTime UpdateTime { get; set; }
        }
    }
}
