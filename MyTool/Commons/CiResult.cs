using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyTool.Enums;

namespace MyTool.Commons
{
    /// <summary>
    /// 執行結果
    /// </summary>
    public class CiResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 執行訊息
        /// </summary>
        public string Message { get; set; }


        public Guid ID { get; set; }
    }

    /// <summary>
    /// 執行結果
    /// </summary>
    public class CiResult<T> : CiResult
    {
        /// <summary>
        /// 結果物件
        /// </summary>
        public T Data { get; set; }
    }

}