using MyTool.Enums;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyTool.Tools
{
    /// <summary>
    /// 訂單
    /// </summary>
    public class OrderTool
    {
        /// <summary>
        /// 運行中狀態(計算銷售量用)
        /// </summary>
        public static List<OrderStatus> EnabledStatus = new List<OrderStatus> {
            OrderStatus.New,
            OrderStatus.NonPayment,
            OrderStatus.Processing,
            OrderStatus.Shipment,
            OrderStatus.TeamEdit,
            OrderStatus.TeamEditConfirm,
            OrderStatus.TeamEditDone,
            OrderStatus.Refuse,
            OrderStatus.Abandon,
            OrderStatus.Done
        };

        /// <summary>
        /// 下一步允許的流程
        /// </summary>    
        /// <param name="nowStatus">現在訂單狀態</param>
        /// <param name="user">登入者身分(Member/Admin)</param>
        /// <param name="OrderStatuses">包含訂單流程</param>
        /// <returns></returns>
        public static List<OrderStatus> AllowSteps(OrderStatus nowStatus, AccountType user, List<OrderStatus> OrderStatuses)
        {
            //允許的流程
            var model = new List<OrderStatus>();
            //允許編輯的身分
            var editor = AccountType.None;

            switch (nowStatus)
            {
                //編輯中
                case OrderStatus.Editing:
                    model = new List<OrderStatus> { OrderStatus.New, OrderStatus.NonPayment, OrderStatus.Delete };
                    editor = AccountType.Member;
                    break;

                //待確認
                case OrderStatus.New:
                    model = new List<OrderStatus> { OrderStatus.Processing, OrderStatus.NonPayment, OrderStatus.Combine, OrderStatus.Refuse, OrderStatus.Cancel, OrderStatus.Done };
                    editor = AccountType.Admin;
                    break;

                //待付款
                case OrderStatus.NonPayment:
                    if (user == AccountType.Admin)
                    {
                        model = new List<OrderStatus> { OrderStatus.OverduePayment, OrderStatus.Processing, OrderStatus.Refuse, OrderStatus.Cancel, OrderStatus.Done };
                        editor = user;
                    }
                    else if (user == AccountType.Member) //會員-可退回
                    {
                        model = new List<OrderStatus> { OrderStatus.Editing };
                        editor = user;
                    }
                    break;

                //付款過期
                case OrderStatus.OverduePayment:
                    model = new List<OrderStatus> { OrderStatus.NonPayment, OrderStatus.Processing, OrderStatus.Refuse, OrderStatus.Cancel, OrderStatus.Done };
                    editor = AccountType.Admin;
                    break;

                //處理中
                case OrderStatus.Processing:
                    model = new List<OrderStatus> { OrderStatus.NonPayment, OrderStatus.Shipment, OrderStatus.Combine, OrderStatus.Refuse, OrderStatus.Cancel, OrderStatus.Done };
                    editor = AccountType.Admin;
                    break;

                //已出貨
                case OrderStatus.Shipment:
                    model = new List<OrderStatus> { OrderStatus.Done };
                    editor = AccountType.Member;
                    break;

                //編輯團隊-未完成
                case OrderStatus.TeamEdit:
                    if (user == AccountType.Admin)
                    {
                        model = new List<OrderStatus> { OrderStatus.Abandon };
                        editor = user;
                    }
                    else if (user == AccountType.Member)
                    {
                        model = new List<OrderStatus> { OrderStatus.TeamEditConfirm, OrderStatus.Abandon };
                        editor = user;
                    }                 
                    break;

                //編輯團隊-待確認
                case OrderStatus.TeamEditConfirm:
                    model = new List<OrderStatus> { OrderStatus.TeamEditDone };
                    editor = user;//皆可
                    break;

                //編輯完成
                case OrderStatus.TeamEditDone:
                    model = new List<OrderStatus> { OrderStatus.TeamEdit };
                    editor = AccountType.Member;
                    break;

                //退回編輯
                case OrderStatus.Refuse:
                    model = new List<OrderStatus> { OrderStatus.Editing };
                    editor = AccountType.Member;
                    break;

                //放棄
                case OrderStatus.Abandon: //從程式排程作廢
                    model = new List<OrderStatus> { OrderStatus.TeamEdit };
                    editor = AccountType.Admin;
                    break;

                //已合併
                case OrderStatus.Combine:
                    break;

                //取消
                case OrderStatus.Cancel:
                    break;

                //完成
                case OrderStatus.Done:
                    if (user == AccountType.Member) //會員-可作廢
                    {
                        model = new List<OrderStatus> { OrderStatus.Invalid };
                        editor = user;
                    }
                    break;

                //截止未完成
                case OrderStatus.NotDone:
                    break;

                //作廢
                case OrderStatus.Invalid:
                    break;
            }

            //編輯身分不符合 return null
            if (editor != user)
            {
                return new List<OrderStatus>();
            }
            //允許next步驟 && 訂單包含流程
            else
            {
                return model.Where(x => OrderStatuses.Contains(x)).ToList();
            }
        }

    }
}
