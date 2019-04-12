using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyTool.Tools
{
    public class MailTool
    {
        public SmtpServerViewModel setting { get; set; }
        public string email { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
        public string[] bccReceiver { get; set; }
        public string filePath { get; set; }

        public async Task<bool> SendAsync(string fromFn = "")
        {
            if (!_Check.IsEmail(email) || !setting.IsEnabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(setting.MailAddress)) {
                setting.MailAddress = setting.Account;
            }
            var formAddress = new MailAddress(setting.MailAddress, setting.DisplayName);
            var toAddress = new MailAddress(email);
            var mail = new MailMessage(formAddress, toAddress)
            {
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                Subject = subject,
                Body = content
            };

            //附件
            if (!string.IsNullOrEmpty(filePath))
            {
                Attachment attachment = new Attachment(HttpContext.Current.Server.MapPath(filePath));
                mail.Attachments.Add(attachment);
            }

            //副本
            if (bccReceiver != null)
            {
                foreach (string r in bccReceiver)
                {
                    mail.Bcc.Add(new MailAddress(r));
                }
            }

            var smtp = new SmtpClient();
            if (!string.IsNullOrEmpty(setting.Password))
            {
                smtp.Host = setting.MailServer;// "smtp.gmail.com";
                smtp.Credentials = new System.Net.NetworkCredential(setting.Account, setting.Password);
                smtp.Port = setting.Port;
                smtp.EnableSsl = setting.IsSsl;
            }
            else if (setting.MailServer == "localhost")
            {
                smtp.Host = setting.MailServer;
                smtp.ServicePoint.MaxIdleTime = 1;
            }

            //發送Email
            //smtp.Send(mail);

            //非同步發送Email
            try
            {
                await smtp.SendMailAsync(mail);
            }
            catch (System.Exception e)
            {
                smtp.Dispose();
                _Log.CreateText($"Send_MailError {fromFn}: " + _Json.ModelToJson(e));
                return false;
            }

            smtp.Dispose();
            //_Log.CreateText($"Send_Mail: {email}, {subject}");
            return true;
        }

        public bool Send(string fromFn = "")
        {
            if (!_Check.IsEmail(email) || !setting.IsEnabled)
            {
                return false;
            }

            var formAddress = new MailAddress(setting.Account, setting.DisplayName);
            var toAddress = new MailAddress(email);
            var mail = new MailMessage(formAddress, toAddress)
            {
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                Subject = subject,
                Body = content
            };

            //附件
            if (!string.IsNullOrEmpty(filePath))
            {
                Attachment attachment = new Attachment(HttpContext.Current.Server.MapPath(filePath));
                mail.Attachments.Add(attachment);
            }

            //副本
            if (bccReceiver != null)
            {
                foreach (string r in bccReceiver)
                {
                    mail.Bcc.Add(new MailAddress(r));
                }
            }

            var smtp = new SmtpClient();
            if (!string.IsNullOrEmpty(setting.Password))
            {
                smtp.Host = setting.MailServer;// "smtp.gmail.com";
                smtp.Credentials = new System.Net.NetworkCredential(setting.Account, setting.Password);
                smtp.Port = setting.Port;
                smtp.EnableSsl = setting.IsSsl;
            }
            else if (setting.MailServer == "localhost")
            {
                smtp.Host = setting.MailServer;
                smtp.ServicePoint.MaxIdleTime = 1;
            }
                        
            try
            {
                smtp.Send(mail);
            }
            catch (System.Exception e)
            {
                smtp.Dispose();
                _Log.CreateText($"Send_MailError NotAsync : " + _Json.ModelToJson(e));
                return false;
            }

            smtp.Dispose();          
            return true;
        }

        /// <summary>
        /// 置換郵件內容
        /// </summary>
        public class ReplaceMailContent
        {
            public string ReplaceContent(string content)
            {
                return content
                    .Replace("@" + MailTag.WebsiteName.ToString() + "@", WebsiteName)
                    .Replace("@" + MailTag.WebsiteUrl.ToString() + "@", WebsiteUrl)
                    .Replace("@" + MailTag.UserName.ToString() + "@", UserName)
                    .Replace("@" + MailTag.UserEmail.ToString() + "@", UserEmail)
                    .Replace("@" + MailTag.ConfirmUrl.ToString() + "@", ConfirmUrl)
                    .Replace("@" + MailTag.ValidCode.ToString() + "@", ValidCode)
                    .Replace("@" + MailTag.OrderContent.ToString() + "@", OrderContent)
                    .Replace("@" + MailTag.OrderATMInfo.ToString() + "@", OrderATMInfo)
                    .Replace("@" + MailTag.OrderAdminNote.ToString() + "@", OrderAdminNote)
                    .Replace("@" + MailTag.MemberName.ToString() + "@", MemberName)
                    .Replace("@" + MailTag.CompetitionCame.ToString() + "@", CompetitionCame);

            }

            /// <summary>
            /// 網站名稱
            /// </summary>
            public string WebsiteName { get; set; }

            /// <summary>
            /// 網址
            /// </summary>
            public string WebsiteUrl { get; set; }

            /// <summary>
            /// 會員名稱 (Fn傳入)
            /// </summary>
            public string UserName { get; set; }

            /// <summary>
            /// 會員電子信箱 (Fn傳入)
            /// </summary>
            public string UserEmail { get; set; }

            /// <summary>
            /// 會員電子信箱 (Fn傳入)
            /// </summary>
            public string PhoneNumber { get; set; }

            /// <summary>
            /// 驗證網址
            /// </summary>
            public string ConfirmUrl { get; set; }

            /// <summary>
            /// 簡訊驗證碼
            /// </summary>
            public string ValidCode { get; set; }

            /// <summary>
            /// 訂單內容
            /// </summary>
            public string OrderContent { get; set; }

            /// <summary>
            /// 訂單匯款帳號
            /// </summary>    
            public string OrderATMInfo { get; set; }

            /// <summary>
            /// 訂單管理員備註
            /// </summary>     
            public string OrderAdminNote { get; set; }

            /// <summary>
            /// 指派選手姓名
            /// </summary>
            public string MemberName { get; set; }

            /// <summary>
            /// 指派比賽名稱
            /// </summary>
            public string CompetitionCame { get; set; }
        }

        /// <summary>
        /// 開發者mail
        /// </summary>
        public bool IsTestingMail(string mail)
        {
            var developers = new List<string> { "annie2639service@gmail.com", "annie2639_6@hotmail.com", "annie.leverage@gmail.com" };

            return developers.Contains(mail);
        }
    }


}
