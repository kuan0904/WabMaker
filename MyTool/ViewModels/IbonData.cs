using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MyTool.ViewModels
{
    /// <summary>
    /// 繳費資料傳送 (安源 -> 業者端) 所有欄位均為必填
    /// </summary>
    [XmlRoot("SENDDATA")]
    public class IbonSendData
    {
        /// <summary>
        /// 處理動作 (6碼; 079001:資料交換送出, 079002:資料交換業者回覆)
        /// </summary>
        public string BUSINESS { get; set; }

        /// <summary>
        /// 門市店號 (6碼; 原封不動回傳)
        /// </summary>
        public string STOREID { get; set; }

        /// <summary>
        /// 業者代碼 (2碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string SHOPID { get; set; }

        /// <summary>
        /// 交易序號 (14碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string DETAILED_NUM { get; set; }

        /// <summary>
        /// 商品代號 (20碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string PRODUCT_CODE { get; set; }

        /// <summary>
        /// 狀態代碼 (4碼; 0000:成功, 1003:資料格式不對)
        /// </summary>
        public string STATUS_CODE { get; set; }

        /// <summary>
        /// 狀態描述 (100碼)
        /// </summary>
        public string STATUS_DESC { get; set; }

        /// <summary>
        /// 資料一 (50碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string KEY1 { get; set; }

        /// <summary>
        /// 資料二 (50碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string KEY2 { get; set; }

        /// <summary>
        /// 資料三 (50碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string KEY3 { get; set; }

        /// <summary>
        /// 資料四 (50碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string KEY4 { get; set; }

        /// <summary>
        /// 資料五 (50碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string KEY5 { get; set; }
    }

    /// <summary>
    /// 繳費資料回傳 (業者端 -> 安源) 
    /// </summary>
    public class IbonShowData : IbonSendData
    {
        public IbonShowData() { }
        public IbonShowData(IbonSendData baseData)
        {
            this.BUSINESS = baseData.BUSINESS;
            this.STOREID = baseData.STOREID;
            this.SHOPID = baseData.SHOPID;
            this.DETAILED_NUM = baseData.DETAILED_NUM;
            this.PRODUCT_CODE = baseData.PRODUCT_CODE;
            this.STATUS_CODE = baseData.STATUS_CODE;
            this.STATUS_DESC = baseData.STATUS_DESC;
            this.KEY1 = baseData.KEY1;
            this.KEY2 = baseData.KEY2;
            this.KEY3 = baseData.KEY3;
            this.KEY4 = baseData.KEY4;
            this.KEY5 = baseData.KEY5;

            LISTDATAs = new List<IbonListData>();
        }

        /// <summary>
        /// 總金額
        /// </summary>
        public int TOTALAMOUNT { get; set; }

        /// <summary>
        /// 總筆數 (不包含第一列的 TITLE 資料)
        /// </summary>
        public int TOTALCOUNT { get; set; }

        [XmlElement("LISTDATA")]
        public List<IbonListData> LISTDATAs { get; set; }
    }

    /// <summary>
    /// 繳費資料明細
    /// </summary>
    public class IbonListData
    {
        /// <summary>
        /// 序號 (10碼; 第一個須為 00 裡面傳的為繳費項目的 TITLE )
        /// </summary>
        public string SERIALNO { get; set; }

        /// <summary>
        /// 是否可繳 (1碼; Y 即會讓使用者勾選, N 則不讓使用者勾選)
        /// </summary>
        public string PRINT { get; set; }

        /// <summary>
        /// 訂單編號 (16碼英數字; 當有結帳通知流程時，會以此通知業者該筆訂單已結帳)
        /// </summary>
        public string CP_ORDER { get; set; }

        /// <summary>
        /// 資料_1 (50碼; 必填, 特殊符號只允許：（）_-)
        /// </summary>
        public string DATA_1 { get; set; }

        /// <summary>
        /// 資料_2 (50碼; 必填, 特殊符號只允許：（）_-)
        /// </summary>
        public string DATA_2 { get; set; }

        /// <summary>
        /// 資料_3 (50碼; 必填, 特殊符號只允許：（）_-)
        /// </summary>
        public string DATA_3 { get; set; }

        /// <summary>
        ///  資料_4 (50碼; 必填, 該筆的繳費金額)
        /// </summary>
        public string DATA_4 { get; set; }

        /// <summary>
        ///  資料_5 (50碼; 必填, 特殊符號只允許：（）_-)
        /// </summary>
        public string DATA_5 { get; set; }

        /// <summary>
        /// 資料_6 (50碼; 特殊符號只允許：（）_-)
        /// </summary>
        public string DATA_6 { get; set; }

        /// <summary>
        /// 資料_7 (50碼; 特殊符號只允許：（）_-)
        /// </summary>
        public string DATA_7 { get; set; }

        /// <summary>
        /// 資料_8 (50碼; 特殊符號只允許：（）_-)
        /// </summary>
        public string DATA_8 { get; set; }
    }


    /// <summary>
    /// 通知結帳 (安源 -> 業者端)  所有欄位均為必填
    /// </summary>
    [XmlRoot("PAYMONEY")]
    public class IbonPayMoney
    {
        /// <summary>
        /// 傳送次數
        /// </summary>
        public int SENDTIME { get; set; }

        /// <summary>
        /// 門市店號 (6碼; 原封不動回傳)
        /// </summary>
        public string STOREID { get; set; }

        /// <summary>
        /// 業者代碼 (2碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string SHOPID { get; set; }

        /// <summary>
        /// 交易序號 (14碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string DETAIL_NUM { get; set; }

        /// <summary>
        /// 狀態代碼 (4碼; 0000:成功, 1003:資料格式不對)
        /// </summary>
        public string STATUS_CODE { get; set; }

        /// <summary>
        /// 狀態描述 (100碼)
        /// </summary>
        public string STATUS_DESC { get; set; }

        /// <summary>
        /// 第一段條碼 (9碼, 由安源產生並提供)
        /// </summary>
        public string BARCODE1 { get; set; }

        /// <summary>
        /// 第二段條碼 (16碼, 由安源產生並提供)
        /// </summary>
        public string BARCODE2 { get; set; }

        /// <summary>
        /// 第三段條碼 (15碼, 由安源產生並提供)
        /// </summary>
        public string BARCODE3 { get; set; }

        /// <summary>
        /// 付款金額
        /// </summary>
        public string AMOUNT { get; set; }

        /// <summary>
        /// 付款日期 15碼；西元年(4碼)＋月(2碼)＋日(2碼)＋時(2碼)＋分(2碼)＋秒(2碼) 
        /// </summary>
        public string PAYDATE { get; set; }

        /// <summary>
        /// 使用者資料1 (30碼)
        /// </summary>
        public string USERDATA1 { get; set; }

        /// <summary>
        ///  使用者資料2 (30碼)
        /// </summary>
        public string USERDATA2 { get; set; }

        /// <summary>
        ///  使用者資料3 (30碼)
        /// </summary>
        public string USERDATA3 { get; set; }

        /// <summary>
        ///  使用者資料4 (30碼)
        /// </summary>
        public string USERDATA4 { get; set; }

        /// <summary>
        ///  使用者資料5 (30碼)
        /// </summary>
        public string USERDATA5 { get; set; }
    }

    /// <summary>
    /// 通知結帳回應 (業者端 -> 安源)
    /// </summary>
    [XmlRoot("PAYMONEY_R")]
    public class IbonPayReturn
    {
        /// <summary>
        /// 業者代碼 (2碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string SHOPID { get; set; }

        /// <summary>
        /// 交易序號 (14碼; 由安源提供,原封不動回傳)
        /// </summary>
        public string DETAIL_NUM { get; set; }

        /// <summary>
        /// 狀態代碼 (4碼; 0000:成功, 1003:資料格式不對)
        /// </summary>
        public string STATUS_CODE { get; set; }

        /// <summary>
        /// 狀態描述 (100碼)
        /// </summary>
        public string STATUS_DESC { get; set; }

        /// <summary>
        /// 確認收到 (5碼；OK:表示收到資料；失敗:FAIL)
        /// </summary>
        public string CONFIRM { get; set; }
    }

}
