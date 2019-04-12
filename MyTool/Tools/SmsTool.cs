using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MyTool.Tools
{
    /// <summary>
    /// 三竹簡訊
    /// </summary>
    public class SmsTool
    {
        public SmsServiceViewModel setting { get; set; }
        public string message { get; set; }

        /// <summary>
        /// 單筆簡訊發送
        /// </summary>      
        /// <param name="mobileNumber">手機號碼</param>      
        /// <param name="allowLogNessage">允許長訊息</param>
        /// <returns></returns>
        public async Task<CiResult<SMSViewModel>> Send(Guid userID, string phoneNumber, bool allowLogNessage = false)
        {
            //判斷字元數，如果超過70個字則使用長簡訊發送
            //簡訊發送 http://smexpress.mitake.com.tw:9600/SmSendGet.asp
            //單筆長簡訊 http://smexpress.mitake.com.tw:7002/SpLmGet.asp

            var result = new CiResult<SMSViewModel>();
            if (!_Check.IsPhone(phoneNumber) || !setting.IsEnabled)
            {
                return result;
            }

            if (!allowLogNessage && message.Length >= 70)
            {
                _Log.CreateText("SendSms 訊息過長: " + message);
                return result;
            }

            var bData = Encoding.GetEncoding("big5");
            string messageEn = HttpUtility.UrlEncode(message, bData);

            string url = message.Length >= 70 ?
                "http://smexpress.mitake.com.tw:7002/SpLmGet" :
                "http://smexpress.mitake.com.tw:9600/SmSendGet.asp"
                + $"?username={setting.Username}&password={setting.Password}"
                + $"&DestName={userID}&dstaddr={phoneNumber}&smbody={messageEn}";

            //+"&response=http://192.168.1.200/smreply.asp" //狀態通知網址

            try
            {
                var isTest = false;
                var urlResult = isTest ?
                    "[1]\r\nmsgid=1472692279\r\nstatuscode=1\r\nAccountPoint=1415\r\n" :
                    _Web.RequestUrlGet(url);

                var data = ConverResult(urlResult);
                result.Data = data.FirstOrDefault();
                result.IsSuccess = true;
            }

            catch (Exception e)
            {
                _Log.CreateText($"SendSms : " + _Json.ModelToJson(e));
            }

            return result;
        }


        /// <summary>
        /// 多筆簡訊發送
        /// </summary>


        /// <summary>
        /// 簡訊狀態查詢(建議一次查詢以100筆為限)
        /// </summary>
        /// <param name="msgidList">The msgid list.</param>
        /// <returns></returns>
        public string QueryStatus(List<string> msgidList)
        {
            string url = "http://smexpress.mitake.com.tw:9600/SmQueryGet.asp"
             + $"?username={setting.Username}&password={setting.Password}&msgid=";


            if (msgidList.Count > 0)
            {
                string msgidStr = string.Empty;
                foreach (var msgid in msgidList)
                {
                    if (!string.IsNullOrEmpty(msgidStr)) msgidStr += ",";
                    msgidStr += msgid;
                }
                url += msgidStr;

                string result = _Web.RequestUrlGet(url);
                // 回覆 (msgid + tab + statuscode + statustime)

                return result;
            }

            return "";
        }

        /// <summary>
        /// 帳號餘額查詢
        /// </summary>
        /// <returns></returns>
        public int QueryAccount()
        {
            int count = -1;
            string url = "http://smexpress.mitake.com.tw:9600/SmQueryGet.asp"
                        + $"?username={setting.Username}&password={setting.Password}&msgid=";
            string result = _Web.RequestUrlGet(url);
            // 回覆 (AccountPoint=110)

            result = result.Replace("AccountPoint=", "")
                           .Replace("\r\n", "");

            if (!string.IsNullOrWhiteSpace(result))
            {
                int.TryParse(result, out count);
            }

            return count;
        }

        /// <summary>
        /// 解析發送回覆
        /// </summary>
        /// <param name="modelList">The model list.</param>
        /// <param name="resultMsg">The result MSG.</param>
        /// <returns></returns>
        public static List<SMSViewModel> ConverResult(string resultMsg)
        {
            #region 格式
            /* 回復格式*//*
            [101]
            msgid=0311216969
            statuscode=1
            [102]
            msgid=0311216970
            statuscode=1
            [103]
            msgid=0311216971
            statuscode=1
            AccountPoint=123

            /* txt*//*
            [101]\r\nmsgid=0311216969\r\nstatuscode=1\r\n [102]\r\nmsgid=0311216970\r\nstatuscode=1\r\n [103]\r\nmsgid=0311216971\r\nstatuscode=1\r\nAccountPoint=123\r\n
          
            /*轉json*//*
            [
                {
                    "index": "101",
                    "msgid": "0311216969",
                    "statuscode": "1"
                },
                {
                    "index": "102",
                    "msgid": "0311216970",
                    "statuscode": "1"
                },
                {
                    "index": "103",
                    "msgid": "0311216971",
                    "statuscode": "1"
                }
            ]
             
            */
            #endregion

            if (!string.IsNullOrEmpty(resultMsg))
            {
                // 轉json格式
                resultMsg = Regex.Replace(resultMsg, @"\[(\d+)\]\r\n", "{\"index\": \"$1\",");
                resultMsg = Regex.Replace(resultMsg, @"msgid=(\d+)\r\n", "\"msgid\": \"$1\",");
                resultMsg = Regex.Replace(resultMsg, @"statuscode=(\d+)\r\n", "\"statuscode\": \"$1\"},");
                resultMsg = Regex.Replace(resultMsg, @"AccountPoint=(\d+)\r\n", "");

                if (resultMsg.Length > resultMsg.LastIndexOf(","))
                {
                    resultMsg = resultMsg.Remove(resultMsg.LastIndexOf(","));
                }
                resultMsg = "[" + resultMsg + "]";


                // 轉model
                var resultModel = JsonConvert.DeserializeObject<List<SMSViewModel>>(resultMsg);

                foreach (var model in resultModel)
                {
                    model.ResultType = StatusCodeToEnum(model.StatusCode);
                }

                return resultModel;
            }

            return null;
        }

        /// <summary>
        /// 解析查詢回覆
        /// </summary>
        ///// <param name="modelList">The model list.</param>
        ///// <param name="resultMsg">The result MSG.</param>
        ///// <returns></returns>
        //public static List<VoteMember> ConverQuery(List<VoteMember> modelList, string resultMsg)
        //{
        //    #region 格式
        //    /* 回復格式*//*
        //    0311216947	6	20060623103807
        //    0311216948	4	20060623103810
        //    0311216949	4	20060623103809

        //    /* txt*//*
        //    0311216947\t6\t20060623103807\r\n0311216948\t4\t20060623103810\r\n0311216949\t4\t20060623103809

        //    /*轉json*//*
        //    [
        //        {                   
        //            "msgid": "0311216947",
        //            "statuscode": "6"
        //        },
        //        {                    
        //            "msgid": "0311216948",
        //            "statuscode": "4"
        //        },
        //        {                  
        //            "msgid": "0311216949",
        //            "statuscode": "4"
        //        }
        //    ]

        //    */
        //    #endregion

        //    if (!string.IsNullOrEmpty(resultMsg))
        //    {
        //        // 單行時，沒有換行字元
        //        bool MultiLine = (resultMsg.IndexOf("\r\n") > 0);

        //        // 轉json格式
        //        resultMsg = Regex.Replace(resultMsg, @"(\d+)\t(\d+)\t(\d+)", "{\"msgid\": \"$1\",\"statuscode\": \"$2\"}");
        //        resultMsg = resultMsg.Replace("\r\n", ",");

        //        if (resultMsg.LastIndexOf(",") > 0 && MultiLine)
        //        {
        //            resultMsg = resultMsg.Remove(resultMsg.LastIndexOf(","));

        //        }
        //        resultMsg = "[" + resultMsg + "]";

        //        // 轉model
        //        var resultModel = JsonConvert.DeserializeObject<List<SMSViewModel>>(resultMsg);

        //        // mapping (根據msgid)
        //        if (modelList != null)
        //        {
        //            foreach (var model in modelList)
        //            {
        //                var r = resultModel.FirstOrDefault(x => x.MsgId == model.Msgid);
        //                if (r != null)
        //                {
        //                    model.SendResult = (int)ConvertStatusCode(r.StatusCode);
        //                }
        //            }
        //        }

        //        return modelList;
        //    }

        //    return null;
        //}

        /// <summary>
        /// (發送結果回覆)狀態碼轉Emum
        /// </summary>
        /// <param name="statuscode">The statuscode.</param>
        /// <returns></returns>
        public static SmsResultType StatusCodeToEnum(string statuscode)
        {
            // 狀態預設Fail
            SmsResultType result = SmsResultType.Fail;

            switch (statuscode)
            {
                case "*": //系統發生錯誤，請聯絡三竹資訊窗口人員
                case "a": //簡訊發送功能暫時停止服務，請稍候再試
                case "b": //簡訊發送功能暫時停止服務，請稍候再試
                case "c": //請輸入帳號
                case "d": //請輸入密碼
                case "e": //帳號、密碼錯誤
                case "f": //帳號已過期
                case "h": //帳號已被停用
                case "k": //無效的連線位址
                case "m": //必須變更密碼，在變更密碼前，無法使用簡訊發送服務
                case "n": //密碼已逾期，在變更密碼前，將無法使用簡訊發送服務
                case "p": //沒有權限使用外部Http程式
                case "r": //系統暫停服務，請稍後再試
                case "s": //帳務處理失敗，無法發送簡訊
                case "t": //簡訊已過期
                case "u": //簡訊內容不得為空白
                case "v": //無效的手機號碼
                    result = SmsResultType.Fail;
                    break;

                case "0": //預約傳送中
                case "1": //已送達業者
                case "2": //已送達業者
                case "3": //已送達業者
                    result = SmsResultType.Progress;
                    break;

                case "4": //已送達手機
                    result = SmsResultType.Done;
                    break;

                case "5": //內容有錯誤
                case "6": //門號有錯誤
                case "7": //簡訊已停用
                case "8": //逾時無送達
                case "9": //預約已取消
                    result = SmsResultType.Fail;
                    break;
            }

            return result;
        }
    }
}
