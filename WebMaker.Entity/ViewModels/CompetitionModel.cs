using MyTool.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMaker.Entity.ViewModels
{
    /// <summary>
    /// 單位清單
    /// </summary>
    public class CompetitionUnitsModel
    {
        [DisplayName("建立人")]
        public string Creater { get; set; }

        [DisplayName("電話")]
        public string Phone { get; set; }

        [DisplayName("E-mail")]
        public string Email { get; set; }


        [DisplayName("單位編號")]
        public int TempNo { get; set; }

        [DisplayName("縣市")]
        public string County { get; set; }

        [DisplayName("單位")]
        public string Unit { get; set; }

        [DisplayName("單位簡寫")]
        public string UnitShort { get; set; }

        [DisplayName("領隊")]
        public string Leader { get; set; }

        [DisplayName("教練")]
        public string Coach { get; set; }

        [DisplayName("管理")]
        public string Manager { get; set; }

        [DisplayName("訂單編號")]
        public string OrderNumber { get; set; }

        //[DisplayName("金額")]
        //public decimal TotalPrice { get; set; }

        [DisplayName("選手數量")]
        public int MemberCount { get; set; }

        //成員名單
        public List<CompetitionItemTeamMember> Members { get; set; }
    }

    public class CompetitionUnitsModelAll
    {
        [DisplayName("建立人")]
        public string Creater { get; set; }

        [DisplayName("電話")]
        public string Phone { get; set; }

        [DisplayName("E-mail")]
        public string Email { get; set; }

        [DisplayName("單位編號")]
        public int UnitTempNo { get; set; }

        [DisplayName("縣市")]
        public string County { get; set; }

        [DisplayName("單位")]
        public string Unit { get; set; }

        [DisplayName("單位簡寫")]
        public string UnitShort { get; set; }

        [DisplayName("教練")]
        public string Coach { get; set; }

        [DisplayName("領隊")]
        public string Leader { get; set; }

        [DisplayName("管理")]
        public string Manager { get; set; }

        [DisplayName("訂單編號")]
        public string OrderNumber { get; set; }

        //[DisplayName("金額")]
        //public decimal TotalPrice { get; set; }

        [DisplayName("選手數量")]
        public int MemberCount { get; set; }


        [DisplayName("選手編號")]
        public int MemberTempNo { get; set; }

        [DisplayName("姓名")]
        public string MemberName { get; set; }
    }

    /// <summary>
    /// 比賽項目人數統計
    /// </summary>
    public class CompetitionItemsModel
    {
        //項目ID
        public Guid ID { get; set; }

        [DisplayName("項目")]
        public string Subject { get; set; }

        [DisplayName("組別")]
        public string Option { get; set; }

        [DisplayName("團隊數量")]
        public int TeamCount { get; set; }

        //參賽名單
        public List<CompetitionItemTeam> Teams { get; set; }
    }

    public class CompetitionItemTeam
    {
        public CompetitionItemTeam()
        {
            Members = new List<CompetitionItemTeamMember>();
        }

        [DisplayName("團隊名稱")]
        public string DetailTeamName { get; set; }

        [DisplayName("上傳檔案")]
        public string FilePath { get; set; }

        [DisplayName("價格")]
        public decimal DetailPrice { get; set; }

        [DisplayName("優惠")]
        public decimal DiscountPrice { get; set; }

        public List<CompetitionItemTeamMember> Members { get; set; }
    }

    public class CompetitionItemTeamMember
    {
        [DisplayName("選手編號")]
        public int TempNo { get; set; }

        [DisplayName("姓名")]
        public string MemberName { get; set; }

        [DisplayName("單位")]
        public string Unit { get; set; }
    }

    public class CompetitionItemsModelAll
    {
        [DisplayName("項目")]
        public string Subject { get; set; }

        [DisplayName("組別")]
        public string Option { get; set; }

        [DisplayName("團隊數量")]
        public int TeamCount { get; set; }


        [DisplayName("選手編號")]
        public int TempNo { get; set; }

        [DisplayName("姓名")]
        public string MemberName { get; set; }

        [DisplayName("單位")]
        public string Unit { get; set; }


        [DisplayName("參賽團隊")]
        public string DetailTeamName { get; set; }

        [DisplayName("檔案")]
        public string FilePath { get; set; }

        [DisplayName("價格")]
        public decimal DetailPrice { get; set; }

        [DisplayName("優惠")]
        public decimal DiscountPrice { get; set; }

    }

    /// <summary>
    /// 選手參加項目統計
    /// </summary>
    public class CompetitionMembersModel
    {
        [DisplayName("編號")]
        public int TempNo { get; set; }

        [DisplayName("姓名")]
        public string MemberName { get; set; }

        [DisplayName("性別")]
        public int Gender { get; set; }

        [DisplayName("生日")]
        public DateTime? Birthday { get; set; }

        [DisplayName("身分證")]
        public string IdentityCard { get; set; }


        [DisplayName("縣市")]
        public string County { get; set; }

        [DisplayName("單位編號")]
        public int UnitTempNo { get; set; }

        [DisplayName("單位")]
        public string Unit { get; set; }

        [DisplayName("教練")]
        public string Coach { get; set; }

        [DisplayName("數量")]
        public int ItemCount { get; set; }

        //參加項目
        public List<CompetitionMemberItem> Items { get; set; }
    }

    public class CompetitionMemberItem
    {
        [DisplayName("項目")]
        public string Subject { get; set; }

        [DisplayName("組別")]
        public string Option { get; set; }

        [DisplayName("價格")]
        public decimal DetailPrice { get; set; }

        [DisplayName("優惠")]
        public decimal DiscountPrice { get; set; }
    }

    public class CompetitionMemberItemAll
    {
        [DisplayName("編號")]
        public int TempNo { get; set; }

        [DisplayName("姓名")]
        public string MemberName { get; set; }

        [DisplayName("性別")]
        public int Gender { get; set; }

        [DisplayName("生日")]
        public DateTime? Birthday { get; set; }

        [DisplayName("身分證")]
        public string IdentityCard { get; set; }


        [DisplayName("縣市")]
        public string County { get; set; }

        [DisplayName("單位編號")]
        public int UnitTempNo { get; set; }

        [DisplayName("單位")]
        public string Unit { get; set; }

        [DisplayName("教練")]
        public string Coach { get; set; }

        [DisplayName("數量")]
        public int ItemCount { get; set; }


        [DisplayName("項目")]
        public string Subject { get; set; }

        [DisplayName("組別")]
        public string Option { get; set; }

        [DisplayName("價格")]
        public decimal DetailPrice { get; set; }

        [DisplayName("優惠")]
        public decimal DiscountPrice { get; set; }

    }

    public class MyChildrenRecordModel
    {

        [DisplayName("姓名")]
        public string MemberName { get; set; }

        [DisplayName("單位")]
        public string Unit { get; set; }

        [DisplayName("教練")]//建立人
        public string Coach { get; set; }


        [DisplayName("盃賽")]
        public string Subject { get; set; }
        public string ParentSubject { get; set; }

        [DisplayName("組別")]
        public string Option { get; set; }

        public string OrderNumber { get; set; }

        public int OrderStatus { get; set; }

        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 比賽銷售和狀態統計
    /// </summary>
    public class CompetitionCountModel
    {
        [DisplayName("盃賽")]
        public string ParentSubject { get; set; }

        [DisplayName("分區")]
        public string ArticleSubject { get; set; }

        [DisplayName("組別")]
        public string OptionSubject { get; set; }

        [DisplayName("販售數量")]
        public int StockCount { get; set; }

        [DisplayName("已售數量")]
        public int SaleCount { get; set; }

        [DisplayName("參與選手數")]
        public int MemberCount { get; set; }

        public Guid? ItemID { get; set; }
               
        public int? Status50 { get; set; }

        public int? Status54 { get; set; }

        public int? Status55 { get; set; }
        
        public int? Status80 { get; set; }
        
        public int? Status99 { get; set; }

        public int? Status100 { get; set; }

        public int? Status110 { get; set; }

        public int? Status300 { get; set; }
    }


}
