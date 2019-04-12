using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;

namespace WebMaker.BLL.Services
{
    /// <summary>
    /// 登入驗證、管理員帳號管理
    /// </summary>
    public class UserService : BaseService
    {
        #region Get

        private IQueryable<mgt_User> Query
        {
            get
            {
                return Db.mgt_User
                   .Include(x => x.mgt_UserProfile)
                   .Include(x => x.mgt_UserRoleRelation)
                   .Include(x => x.mgt_UserLog)
                   .Where(x => x.UserStatus != (int)UserStatus.Delete && x.ClientID == ClientID);
            }
        }

        public PageModel<UserViewModel> GetList(PageParameter param, UserFilter filter, List<int> accountTypes)
        {
            var query = Query.Where(x => accountTypes.Contains(x.AccountType)).ToList();

            //普通使用者看不到超級管理員
            if (!IsSuperManager)
            {
                query = query.Where(x => x.ID != Setting.SuperManagerID).ToList();
            }

            //搜尋關鍵字
            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                query = query.Where(x => x.Name.Contains(filter.SearchString)
                                   || x.Email.Contains(filter.SearchString)).ToList();
            }

            //篩選登入類型
            if (filter.SelectLoginType != null && filter.SelectLoginType.Any())
            {
                query = query.Where(x => filter.SelectLoginType.Any(y => x.LoginTypes.HasValue((int)y))).ToList();
            }

            //篩選身分
            if (filter.SelectRoleID != null && filter.SelectRoleID.Any())
            {
                query = query.Where(x => filter.SelectRoleID.Any(y => x.mgt_UserRoleRelation.Any(z => y == z.RoleID && z.IsEnabled && !z.IsDelete))).ToList();
            }

            //篩選狀態
            if (filter.SelectUserStatus != null && filter.SelectUserStatus.Any())
            {
                query = query.Where(x => filter.SelectUserStatus.Any(y => x.UserStatus == (int)y)).ToList();
            }

            var pagedModel = PageTool.CreatePage(query, param);
            var model = new PageModel<UserViewModel>
            {
                CurrentPage = pagedModel.CurrentPage,
                PageSize = pagedModel.PageSize,
                TotalCount = pagedModel.TotalCount,
                PageCount = pagedModel.PageCount,
                DataStart = pagedModel.DataStart,
                DataEnd = pagedModel.DataEnd,
                Data = pagedModel.Data.Select(x => ToViewModel(x, DataLevel.Simple)).ToList()
            };

            return model;
        }

        public mgt_User Get(Guid id)
        {
            var query = Query.FirstOrDefault(x => x.ID == id);
            return query;
        }

        public UserViewModel GetView(Guid id, mgt_Role AddRole = null, DataLevel dataLevel = DataLevel.Normal)
        {
            var query = Query.FirstOrDefault(x => x.ID == id);
            return ToViewModel(query, dataLevel, AddRole);
        }

        /*todo 要避免已解密的資料直接被save*/
        private UserViewModel ToViewModel(mgt_User user, DataLevel dataLevel, mgt_Role AddRole = null)
        {
            var model = new UserViewModel
            {
                User = user,
                UserProfile = user.mgt_UserProfile == null ? new mgt_UserProfile() : user.mgt_UserProfile,
            };

            //身分列表
            model.RoleRelations = user.mgt_UserRoleRelation.Where(x => !x.mgt_Role.IsDelete && !x.IsDelete
            && (x.IsEnabled || dataLevel == DataLevel.All)) //完整連不啟用都顯示
            .OrderBy(x => x.CreateTime).ToList();

            //完整資料
            if (dataLevel >= DataLevel.Normal)
            {
                var userContentTypes = user.mgt_UserRoleRelation.Select(x => x.mgt_Role.UserContentTypes).ToList();
                var userRequiredTypes = user.mgt_UserRoleRelation.Select(x => x.mgt_Role.UserRequiredTypes).ToList();

                //加入身分(報名後自動加入身分)
                if (AddRole != null)
                {
                    userContentTypes.Add(AddRole.UserContentTypes);
                    userRequiredTypes.Add(AddRole.UserRequiredTypes);
                }

                model.UserContentTypes = string.Join(",", userContentTypes).ToContainList<UserContentType>();
                model.UserRequiredTypes = string.Join(",", userRequiredTypes).ToContainList<UserRequiredType>();


                //log 解密、轉Model
                if (dataLevel == DataLevel.All)
                {
                    model.mgt_UserLogs = user.mgt_UserLog.OrderByDescending(x => x.SeqNo).ToList();

                    foreach (var log in model.mgt_UserLogs)
                    {
                        log.DataContent = _Crypto.DecryptAES(log.DataContent, Setting.UserCryptoKey);

                        //Type myType = _Model.GetType(log.ModelName);
                        //var logData = Newtonsoft.Json.JsonConvert.DeserializeObject(log.DataContent, myType);
                        //var userModel = (UserSimpleModel)logData;
                    }
                }

                //解密         
                model.User.Phone = _Crypto.DecryptAES(model.User.Phone, Setting.UserCryptoKey);
                model.User.Address = _Crypto.DecryptAES(model.User.Address, Setting.UserCryptoKey);
                model.UserProfile.IdentityCard = _Crypto.DecryptAES(model.UserProfile.IdentityCard, Setting.UserCryptoKey);
                model.UserProfile.HomePhone = _Crypto.DecryptAES(model.UserProfile.HomePhone, Setting.UserCryptoKey);
                model.UserProfile.CompanyPhone = _Crypto.DecryptAES(model.UserProfile.CompanyPhone, Setting.UserCryptoKey);
                model.UserProfile.EmergencyPhone = _Crypto.DecryptAES(model.UserProfile.EmergencyPhone, Setting.UserCryptoKey);
            }

            if (!string.IsNullOrEmpty(model.User.Password))
            {
                model.User.Password = "*********";
            }
            //todo add mark

            return model;
        }

        private mgt_User GetEpaperUser(string email)
        {
            var query = Query.FirstOrDefault(x => x.Email == email && x.IsReceiveEpaper &&
                                x.AccountType == (int)AccountType.None && x.UserStatus == (int)UserStatus.None && string.IsNullOrEmpty(x.LoginTypes));
            return query;
        }

        private mgt_User GetByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;

            var query = Query.FirstOrDefault(x => x.Email == email && x.UserStatus == (int)UserStatus.Enabled);
            return query;
        }

        public mgt_User GetbyPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return null;

            var phoneEn = _Crypto.EncryptAES(phone, Setting.UserCryptoKey);

            //電話經驗證
            var query = Query.FirstOrDefault(x => x.Phone == phoneEn && x.PhoneIsVerify &&
                                  x.AccountType == (int)AccountType.Member && x.UserStatus == (int)UserStatus.Enabled);
            return query;
        }

        /// <summary>
        /// 取得自己的電話號碼
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public string GetMyPhoneNumber(Guid id)
        {
            var model = Get(id);
            return _Crypto.DecryptAES(model?.Phone, Setting.UserCryptoKey);
        }

        /// <summary>
        /// MenuID 包含權限Actions
        /// </summary>
        /// <param name="id">UserID</param>
        /// <returns></returns>
        public List<RolePermissionModel> GetRolePermissions(Guid id, AccountType accountType)
        {
            return Get(id).mgt_UserRoleRelation
                          .Where(x => !x.mgt_Role.IsDelete && x.mgt_Role.IsEnabled && x.mgt_Role.AccountType == (int)accountType)
                          .SelectMany(x => x.mgt_Role.mgt_RoleMenuRelation)
                          .Select(x => new RolePermissionModel { MenuID = x.MenuID, ControllerType = x.mgt_Menu.Controller, RoleActions = x.RoleActions })
                          .Distinct()
                          .ToList();
        }

        public MemberLevel? GetMemberLevel(Guid id, AccountType accountType)
        {
            return (MemberLevel?)Get(id).mgt_UserRoleRelation.Select(x => x.mgt_Role)
                          .Where(x => !x.IsDelete && x.IsEnabled && x.AccountType == (int)accountType)
                          .Max(x => x.MemberLevel);
        }

        public List<mgt_Role> GetRoleByLevel(MemberLevel level)
        {
            return Db.mgt_Role.Where(x => !x.IsDelete && x.IsEnabled && x.ClientID == ClientID
                                        && x.MemberLevel == (int)level && x.AccountType == (int)AccountType.Member).ToList();
        }

        /// <summary>
        /// 取得帳號所有角色 for Admin
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public List<CheckBoxListItem> GetRoleCheckList(Guid? id)
        {
            List<Guid> ids = new List<Guid>();
            if (id != null)
            {
                // 帳號所擁有角色
                ids = Get(id.Value)?.mgt_UserRoleRelation
                                    .Where(x => !x.mgt_Role.IsDelete).Select(x => x.RoleID).ToList();
            }

            // 全部角色
            var roleService = new RoleService { ClientID = ClientID };
            var roleList = roleService.GetList(new PageParameter(), AccountType.Admin);

            var checkList = new List<CheckBoxListItem>();

            // 帳號是否包含角色
            foreach (var role in roleList.Data.Where(x => x.IsEnabled))
            {
                var check = new CheckBoxListItem
                {
                    ID = role.ID,
                    Text = role.Name,
                    IsChecked = (id != null) && ids.Contains(role.ID)
                };
                checkList.Add(check);
            }

            return checkList;
        }

        #endregion

        #region Edit

        /// <summary>
        /// 新增Admin、Member (帳號/Email/外部登入) 無UserProfile
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="adminID">The admin identifier.</param>
        /// <param name="isExternalBinding">是否允許外部綁定</param>
        /// <returns></returns>
        public CiResult<Guid> Create(UserViewModel model, Guid? adminID = null, bool isExternalBinding = false)
        {
            CiResult<Guid> result = new CiResult<Guid>();
            mgt_User oldUser = null;

            try
            {
                #region check
                //-----必填-----

                //1.帳號登入                           
                if (model.LoginType == LoginType.Account)
                {
                    //field required
                    if (string.IsNullOrEmpty(model.User.Account) || string.IsNullOrEmpty(model.User.Password))
                    {
                        result.Message = string.Format(SystemMessage.FieldNull, "帳號");
                    }
                }
                //2.Email登入
                else if (model.LoginType == LoginType.Email)
                {
                    //field required
                    if (string.IsNullOrEmpty(model.User.Email) || string.IsNullOrEmpty(model.User.Password))
                    {
                        result.Message = string.Format(SystemMessage.FieldNull, "Email");
                    }
                }
                //3.外部登入
                else if (model.LoginType == LoginType.Facebook)
                {
                    if (model.UserExternalLogin == null || string.IsNullOrEmpty(model.UserExternalLogin.ExternalKey))
                    {
                        result.Message = SystemMessage.ErrorAndHelp;
                    }
                }

                //-----格式-----

                //帳號不重複
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.User.Account))
                    {
                        var exist = CheckAccount(model.User.Account, null);
                        if (exist)
                            result.Message = string.Format(SystemMessage.AccountExist, model.User.Account);
                    }
                }
                //Email格式正確
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.User.Email) && !_Check.IsEmail(model.User.Email))
                    {
                        result.Message = string.Format(SystemMessage.FormatError, "Email");
                    }
                }
                //Email不重複
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.User.Email))
                    {
                        //允許舊帳戶
                        //1.Epaper訂閱者
                        oldUser = GetEpaperUser(model.User.Email);

                        //2.外部登入綁定:無同樣外部登入,但Email相同
                        if (isExternalBinding)
                        {
                            oldUser = Query.FirstOrDefault(x => x.Email == model.User.Email && x.AccountType == (int)AccountType.Member
                                        && !x.mgt_UserExternalLogin.Any(y => y.ExternalType == (int)model.LoginType));
                            isExternalBinding = oldUser != null;
                        }

                        //check Email不重複
                        if (oldUser == null)
                        {
                            var exist = CheckEmail(model.User.Email, null);
                            if (exist)
                                result.Message = string.Format(SystemMessage.EmailExist, model.User.Email);
                        }
                    }
                }
                //電話格式正確
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.User.Phone) && !_Check.IsPhone(model.User.Phone))
                    {
                        result.Message = string.Format(SystemMessage.FormatError, "電話"); ;
                    }
                }

                //get SystemNumber
                string systemNumber = "";
                if (string.IsNullOrEmpty(result.Message))
                {
                    var sysResult = CreateSystemNumber(DataTableCode.mgt_User);
                    if (!sysResult.IsSuccess)
                        result.Message = sysResult.Message;

                    systemNumber = sysResult.Data;
                }

                #endregion

                // create mgt_User  
                var data = new mgt_User();
                if (string.IsNullOrEmpty(result.Message))
                {
                    //1.舊會員加入(Epaper訂閱者/FB綁定同Email)
                    if (oldUser != null)
                    {
                        data = oldUser;
                    }
                    //2.新帳戶
                    else
                    {
                        data = new mgt_User
                        {
                            ID = Guid.NewGuid(),
                            ClientID = ClientID,
                            SystemNumber = systemNumber,
                            CreateTime = DateTime.Now,
                        };
                        Db.mgt_User.Add(data);
                    }

                    if (!isExternalBinding)
                    {
                        //基本資料(加密:身分證、地址、電話)
                        data.Account = model.User.Account;
                        data.Password = _Crypto.HashSHA256(model.User.Password);
                        data.DepartmentID = model.User.DepartmentID;
                        data.Name = model.User.Name.ToTrim();
                        data.Email = model.User.Email;
                        data.Phone = _Crypto.EncryptAES(model.User.Phone, Setting.UserCryptoKey);
                        data.Address = _Crypto.EncryptAES(model.User.Address, Setting.UserCryptoKey);
                        data.Note = model.User.Note;
                        data.AccountType = model.User.AccountType;
                        data.UserStatus = model.User.UserStatus;
                        data.Sort = model.User.Sort;
                        data.UpdateTime = DateTime.Now;
                    }

                    //extral
                    if (model.UserExternalLogin != null)
                    {
                        data.mgt_UserExternalLogin.Add(model.UserExternalLogin);
                    }

                    //LoginType
                    UpdateLoginType(data, adminID);

                    //AddLog
                    var newModel = new UserSimpleModel
                    {
                        User = data,
                        UserProfile = new mgt_UserProfile(),
                    };
                    AddLog(data.ID, model.LoginType == LoginType.None ? UserLogType.SubEpaper : UserLogType.SignUp,
                            "UserSimpleModel", newModel, adminID == null ? data.ID : adminID.Value);

                    //role list
                    if (model.User.AccountType == (int)AccountType.Member)
                    {
                        //加入一般會員權限
                        var roles = GetRoleByLevel(MemberLevel.Normal);
                        if (roles != null && roles.Any())
                        {
                            AddUserMemberRoles(data, roles.Select(x => x.ID).ToList(), adminID: adminID);

                            //check必填
                            var requiredTypeList = roles.Select(x => x.UserRequiredTypes).ToList();
                            result.Message = CheckRequired(model, requiredTypeList);
                        }
                        else
                        {
                            _Log.CreateText("Account_Create: No Default MemberType");
                            result.Message = SystemMessage.ErrorAndHelp; //查無會員Role
                        }
                    }
                    else if (model.RoleIDs != null)
                    {
                        //admin role check
                        UpdateUserRoles(data, model.RoleIDs);
                    }
                }

                if (string.IsNullOrEmpty(result.Message))
                {

                    Db.SaveChanges();
                    result.Data = data.ID;
                    if (isExternalBinding)
                    {
                        result.Message = SystemMessage.BindingSuccess;
                    }
                    else if (model.User.AccountType == (int)AccountType.Member)
                    {
                        result.Message = SystemMessage.SignUpSuccess;
                    }
                    else
                    {
                        result.Message = SystemMessage.CreateSuccess;
                    }


                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("Account_Create:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 管理員修改 (只有帳號、姓名、密碼、權限角色, 無UserProfile)
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult UpdateAdmin(UserViewModel model, Guid adminID)
        {
            CiResult result = new CiResult();
            mgt_User data = Get(model.User.ID);

            try
            {
                //-----必填-----

                //1.帳號登入                           
                if (model.LoginType == LoginType.Account)
                {
                    //field required
                    if (string.IsNullOrEmpty(model.User.Account))
                    {
                        result.Message = string.Format(SystemMessage.FieldNull, "");
                    }
                }
                //2.Email登入
                else if (model.LoginType == LoginType.Email)
                {
                    //field required
                    if (string.IsNullOrEmpty(model.User.Email))
                    {
                        result.Message = string.Format(SystemMessage.FieldNull, "");
                    }
                }

                //-----格式-----

                //帳號不重複
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.User.Account))
                    {
                        var exist = CheckAccount(model.User.Account, model.User.ID);
                        if (exist)
                            result.Message = string.Format(SystemMessage.AccountExist, model.User.Account);
                    }
                }

                #region no Email
                //Email格式正確
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.User.Email) && !_Check.IsEmail(model.User.Email))
                    {
                        result.Message = string.Format(SystemMessage.FormatError, "Email");
                    }
                }
                //Email不重複 
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.User.Email))
                    {
                        var exist = CheckEmail(model.User.Email, model.User.ID);
                        if (exist)
                            result.Message = string.Format(SystemMessage.EmailExist, model.User.Email);

                    }
                }
                #endregion

                //update
                if (string.IsNullOrEmpty(result.Message))
                {
                    //update password (直接更改密碼)
                    if (!string.IsNullOrEmpty(model.User.Password))
                    {
                        var passHash = _Crypto.HashSHA256(model.User.Password);
                        data.Password = passHash;
                    }

                    data.Account = model.User.Account;
                    data.Name = model.User.Name.ToTrim();
                    data.DepartmentID = model.User.DepartmentID;
                    //data.Email = model.User.Email;
                    //data.Phone = _Crypto.EncryptAES(model.User.Phone, Setting.UserCryptoKey);
                    //data.Address = _Crypto.EncryptAES(model.User.Address, Setting.UserCryptoKey);
                    data.Note = model.User.Note;
                    data.UpdateTime = DateTime.Now;

                    //role list
                    UpdateUserRoles(data, model.RoleIDs);

                    //LoginType
                    UpdateLoginType(data, adminID);

                    Db.SaveChanges();

                    result.Message = SystemMessage.UpdateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("Account_UpdateAdmin" + _Json.ModelToJson(ex));
            }

            return result;
        }


        /// <summary>
        /// 會員修改 - 不更改Email、手機
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="allowUpdateEmail">是否修改Email</param>
        /// <param name="targetRoles">指定角色(檢查必填用)</param>        
        /// <returns></returns>
        public CiResult<bool> UpdateMember(UserViewModel model, bool updateEmail, bool updatePhone = true, List<mgt_Role> targetRoles = null, Guid? adminID = null)
        {
            var result = new CiResult<bool>();
            mgt_User data = Get(model.User.ID);

            var contentTypeList = new List<string>();
            var requiredTypeList = new List<string>();

            if (targetRoles == null)
            {
                //所有角色包含欄位       
                contentTypeList = data.mgt_UserRoleRelation.Select(x => x.mgt_Role.UserContentTypes).ToList();
                requiredTypeList = data.mgt_UserRoleRelation.Select(x => x.mgt_Role.UserRequiredTypes).ToList();
            }
            else
            {
                //指定角色(for 申請角色轉換時)
                contentTypeList = targetRoles.Select(x => x.UserContentTypes).ToList();
                requiredTypeList = targetRoles.Select(x => x.UserRequiredTypes).ToList();
            }

            try
            {
                #region 欄位必填

                result.Message = CheckRequired(model, requiredTypeList, updateEmail, updatePhone);

                #endregion

                #region 格式
                //Email格式正確
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.User.Email) && !_Check.IsEmail(model.User.Email))
                    {
                        result.Message = string.Format(SystemMessage.FormatError, "Email");
                    }
                }

                //Email不重複 
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.User.Email) && model.User.Email != data.Email)
                    {
                        var exist = CheckEmail(model.User.Email, model.User.ID);
                        if (exist)
                            result.Message = string.Format(SystemMessage.EmailExist, model.User.Email);
                        else
                        {
                            result.Data = true; //更改Email 重寄驗證信
                            data.EmailIsVerify = false;
                        }
                    }
                }

                //電話格式正確
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.User.Phone) && !_Check.IsPhone(model.User.Phone))
                    {
                        result.Message = string.Format(SystemMessage.FormatError, "電話"); ;
                    }
                }
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.UserProfile?.HomePhone) && !_Check.IsPhone(model.UserProfile.HomePhone, 1))
                    {
                        result.Message = string.Format(SystemMessage.FormatError, "電話"); ;
                    }
                }
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.UserProfile?.EmergencyPhone) && !_Check.IsPhone(model.UserProfile.EmergencyPhone, 1))
                    {
                        result.Message = string.Format(SystemMessage.FormatError, "緊急聯絡電話"); ;
                    }
                }

                #endregion

                //update 更新包含欄位
                //加密:身分證、地址、電話
                //不可直接更改密碼、可新增角色
                if (string.IsNullOrEmpty(result.Message))
                {
                    //----基本資料----                  
                    if (contentTypeList.Any(x => x.HasValue((int)UserContentType.Name)))
                    {
                        data.Name = model.User.Name.ToTrim();
                    }
                    if (updateEmail && contentTypeList.Any(x => x.HasValue((int)UserContentType.Email)))
                    {
                        data.Email = model.User.Email;
                    }
                    if (updatePhone && contentTypeList.Any(x => x.HasValue((int)UserContentType.Phone)))
                    {
                        data.Phone = _Crypto.EncryptAES(model.User.Phone, Setting.UserCryptoKey);
                    }
                    if (contentTypeList.Any(x => x.HasValue((int)UserContentType.Address)))
                    {
                        data.Address = _Crypto.EncryptAES(model.User.Address, Setting.UserCryptoKey);
                    }
                    //if (contentTypeList.Any(x => x.HasValue((int)UserContentType.Note)))
                    //{
                    //    data.Note = model.User.Note;
                    //}
                    data.UpdateTime = DateTime.Now;
                }

                using (TransactionScope trans = new TransactionScope())
                {
                    if (string.IsNullOrEmpty(result.Message)
                        && model.UserProfile != null)
                    {
                        //UserProfile
                        var profileResult = UpdateUserProfile(data.mgt_UserProfile?.ID, model.UserProfile, Setting.UserCryptoKey);
                        if (!profileResult.IsSuccess)
                        {
                            result.Message = profileResult.Message;
                        }
                        data.UserProfileID = profileResult.ID;
                    }

                    if (string.IsNullOrEmpty(result.Message))
                    {
                        //LoginType
                        UpdateLoginType(data);

                        //Role
                        //if (targetRoles != null && addRoleFromOrder != null)
                        //{
                        //    AddUserMemberRoles(data, targetRoles.Select(x => x.ID).ToList(), adminID: adminID);
                        //}

                        //AddLog
                        var newModel = new UserViewModel
                        {
                            User = data,
                            UserProfile = data.mgt_UserProfile
                        };
                        AddLog(data.ID, UserLogType.UpdateInfo, "UserViewModel", newModel, adminID == null ? data.ID : adminID.Value);

                        Db.SaveChanges();
                        trans.Complete();

                        result.Message = SystemMessage.UpdateSuccess;
                        result.IsSuccess = true;
                    }
                }//end using trans

            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("Account_UpdateMember" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 修改Email
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public CiResult UpdateEmail(Guid id, string email)
        {
            CiResult result = new CiResult();
            mgt_User data = Get(id);

            if (string.IsNullOrEmpty(email))
            {
                result.Message = string.Format(SystemMessage.FieldNull, "Email");
            }
            email = email.Trim();

            ////相同
            //if (string.IsNullOrEmpty(result.Message)
            //    && email == data.Email)
            //{
            //    result.Message = string.Format(SystemMessage.FieldSame, "Email");
            //}

            //Email格式正確
            if (string.IsNullOrEmpty(result.Message)
                && !_Check.IsEmail(email))
            {
                result.Message = string.Format(SystemMessage.FormatError, "Email");
            }

            //Email不重複 
            if (string.IsNullOrEmpty(result.Message))
            {
                var exist = CheckEmail(email, null);
                if (exist)
                    result.Message = string.Format(SystemMessage.EmailExist, email);
                else
                {
                    data.Email = email;
                    data.EmailIsVerify = false;
                    AddLog(data.ID, UserLogType.UpdateEmail, "string", email, id);

                    //重新確認LoginType
                    UpdateLoginType(data);

                    Db.SaveChanges();
                    result.Message = SystemMessage.UpdateSuccess;
                    result.IsSuccess = true;
                }
            }

            return result;
        }


        /// <summary>
        /// 修改電話號碼
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public CiResult UpdatePhone(Guid id, string phone)
        {
            CiResult result = new CiResult();
            mgt_User data = Get(id);

            if (string.IsNullOrEmpty(phone))
            {
                result.Message = string.Format(SystemMessage.FieldNull, "手機");
            }
            phone = phone.Trim();

            //電話格式正確
            if (string.IsNullOrEmpty(result.Message)
                && !_Check.IsPhone(phone))
            {
                result.Message = string.Format(SystemMessage.FormatError, "手機");
            }

            //電話不重複
            if (string.IsNullOrEmpty(result.Message))
            {
                var phoneEn = _Crypto.EncryptAES(phone, Setting.UserCryptoKey);
                var exist = CheckPhone(phoneEn, null);
                if (exist)
                    result.Message = string.Format(SystemMessage.PhoneExist, phone);
                else
                {
                    data.Phone = phoneEn;
                    data.PhoneIsVerify = false;
                    AddLog(data.ID, UserLogType.UpdatePhone, "string", phone, id);

                    //重新確認LoginType
                    UpdateLoginType(data);

                    Db.SaveChanges();
                    result.Message = SystemMessage.UpdateSuccess;
                    result.IsSuccess = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 修改密碼(直接重設)
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public CiResult UpdatePassword(Guid id, string password)
        {
            CiResult result = new CiResult();

            if (string.IsNullOrEmpty(password))
            {
                result.Message = string.Format(SystemMessage.FieldNull, "密碼");
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                var passHash = _Crypto.HashSHA256(password);
                mgt_User data = Get(id);

                try
                {
                    data.Password = passHash;
                    data.UpdateTime = DateTime.Now;

                    AddLog(data.ID, UserLogType.UpdatePassword, null, null, data.ID);

                    Db.SaveChanges();
                    result.Message = SystemMessage.PasswordResetSuccess;
                    result.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    result.Message = SystemMessage.UpdateFail;
                    _Log.CreateText("Account_UpdateSelf:" + _Json.ModelToJson(ex));
                }
            }

            return result;
        }

        /// <summary>
        /// 修改密碼(帶驗證)
        /// </summary>
        /// <returns></returns>
        public CiResult UpdatePassword(Guid id, UpdatePasswordViewModel model)
        {
            var result = new CiResult();
            var user = Get(id);

            //比對舊密碼 (原本有密碼才需輸入舊密碼,只有外部登入不用)
            bool existPassword = ExistPassword(id);
            if (existPassword)
            {
                var passHash = _Crypto.HashSHA256(model.OldPassword);
                if (string.IsNullOrEmpty(user.Password) || user.Password != passHash)
                {
                    result.Message = SystemMessage.PasswordWrong;
                }
            }

            //修改密碼
            if (string.IsNullOrEmpty(result.Message))
            {
                result = UpdatePassword(id, model.Password);
            }

            //重新確認LoginType
            if (result.IsSuccess)
            {
                UpdateLoginType(user);
            }

            return result;
        }

        /// <summary>
        /// 修改可登入方式 (when:Account建立、Email通過驗證、FB綁定)
        /// </summary>
        private void UpdateLoginType(mgt_User user, Guid? adminID = null)
        {
            List<string> sum = new List<string>();

            //帳號
            if (!string.IsNullOrEmpty(user.Account) && !string.IsNullOrEmpty(user.Password)
                && user.AccountType == (int)AccountType.Admin)
            {
                sum.Add(((int)LoginType.Account).ToContainStr());
                if (!user.LoginTypes.HasValue((int)LoginType.Account))
                {
                    AddLog(user.ID, UserLogType.AccountLogin, null, null, adminID == null ? user.ID : adminID.Value);
                }
            }

            //Email
            if (!string.IsNullOrEmpty(user.Email) && user.EmailIsVerify && !string.IsNullOrEmpty(user.Password))
            {
                sum.Add(((int)LoginType.Email).ToContainStr());
                if (!user.LoginTypes.HasValue((int)LoginType.Email))
                {
                    AddLog(user.ID, UserLogType.EmailLogin, null, null, adminID == null ? user.ID : adminID.Value);
                }
            }

            //Facebook
            if (user.mgt_UserExternalLogin.Any(y => y.ExternalType == (int)ExternalType.Facebook))
            {
                sum.Add(((int)LoginType.Facebook).ToContainStr());
                if (!user.LoginTypes.HasValue((int)LoginType.Facebook))
                {
                    AddLog(user.ID, UserLogType.FacebookLogin, null, null, adminID == null ? user.ID : adminID.Value);
                }
            }

            user.LoginTypes = string.Join(",", sum);
        }


        /// <summary>
        /// 共用更新UserProfile
        ///  更新個資 User > UpdateMember
        ///  我的孩子 User > CreateMyChild
        ///  人物文章 Item > UpdateItemUserProfile
        ///  訂單選手 Order > Update (OrderCryptoKey)      
        /// </summary>
        public CiResult UpdateUserProfile(Guid? id, mgt_UserProfile model, string cryptoKey, bool checkIdCard = true,
                                          bool contentDelete = false, Guid? CreateUser = null)
        {
            var result = new CiResult();
            //ItemID、OrderID、FromSourceID、CreateUser、IsMyChild 不可修改
            //todo AvatarPath
            //sort

            #region 格式
            if (checkIdCard)
            {
                //身分證格式正確 (非護照)
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.IdentityCard) && !model.IsPassportNumber && !_Check.IsIDcard(model.IdentityCard))
                    {
                        result.Message = string.Format(SystemMessage.FormatError, "身分證"); ;
                    }
                }
            }

            //生日格式
            if (string.IsNullOrEmpty(result.Message))
            {
                if (model.Birthday != null && !_Check.IsBirthday(model.Birthday.Value))
                {
                    result.Message = string.Format(SystemMessage.FormatError, "生日"); ;
                }
            }

            //身份證與性別
            if (string.IsNullOrEmpty(result.Message))
            {
                if (!string.IsNullOrEmpty(model.IdentityCard) && !model.IsPassportNumber && model.Gender != 0)
                {
                    if (model.IdentityCard.Substring(1, 1) != model.Gender.ToString())
                    {
                        result.Message = string.Format(SystemMessage.FormatError, "身分證性別");
                    }
                }
            }
            #endregion

            if (string.IsNullOrEmpty(result.Message))
            {
                if (id == null || id == Guid.Empty)
                {
                    //create
                    model.ID = Guid.NewGuid();
                    model.ClientID = ClientID;
                    model.CreateTime = DateTime.Now;
                    model.UpdateTime = DateTime.Now;
                    id = model.ID;

                    //remove space
                    model.NickName = model.NickName.ToTrim();
                    model.Unit = model.Unit.ToTrim();
                    model.UnitShort = model.UnitShort.ToTrim();

                    //加密                                
                    model.IdentityCard = _Crypto.EncryptAES(model.IdentityCard, cryptoKey);

                    Db.mgt_UserProfile.Add(model);
                }
                else
                {
                    var entity = Db.mgt_UserProfile.Find(id.Value);
                    //update
                    entity.LastName = model.LastName;
                    entity.FirstName = model.FirstName;
                    entity.EngName = model.EngName;
                    entity.NickName = model.NickName.ToTrim();
                    entity.Description = model.Description;

                    entity.HouseholdAddress = model.HouseholdAddress;
                    entity.IsPassportNumber = model.IsPassportNumber;
                    entity.IdentityCard = _Crypto.EncryptAES(model.IdentityCard, cryptoKey);
                    entity.Birthday = model.Birthday;
                    entity.Gender = model.Gender;
                    entity.Marriage = model.Marriage;

                    entity.HomePhone = _Crypto.EncryptAES(model.HomePhone, cryptoKey);
                    entity.CompanyPhone = _Crypto.EncryptAES(model.CompanyPhone, cryptoKey);
                    entity.SecondaryEmail = model.SecondaryEmail;
                    entity.EmergencyContact = model.EmergencyContact;
                    entity.EmergencyPhone = _Crypto.EncryptAES(model.EmergencyPhone, cryptoKey);
                    entity.Unit = model.Unit;
                    entity.UnitShort = model.UnitShort;
                    entity.UnitAddress = model.UnitAddress;

                    entity.Occupation = model.Occupation;
                    entity.Education = model.Education;
                    entity.School = model.School;
                    entity.Skill = model.Skill;
                    entity.Language = model.Language;
                    entity.SocialNetwork = model.SocialNetwork;
                    entity.Sports = model.Sports;
                    entity.Height = model.Height;
                    entity.Weight = model.Weight;
                    entity.Referrer = model.Referrer;

                    entity.Sort = model.Sort;
                    entity.UnitID = model.UnitID;
                    entity.UpdateTime = DateTime.Now;
                    //可刪除
                    if (contentDelete)
                    {
                        entity.IsDelete = model.IsDelete;
                    }
                    //可更換建立人
                    if (CreateUser != null)
                    {
                        entity.CreateUser = CreateUser;
                    }
                }

                Db.SaveChanges();

                result.Message = SystemMessage.UpdateSuccess;
                result.IsSuccess = true;
                result.ID = id.Value;
            }

            return result;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public CiResult Delete(Guid id)
        {
            CiResult result = new CiResult();
            mgt_User data = Get(id);

            try
            {
                data.UserStatus = (int)UserStatus.Delete;
                data.UpdateTime = DateTime.Now;
                Db.SaveChanges();

                result.Message = SystemMessage.DeleteSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.DeleteFail;
                _Log.CreateText("Account_Delete:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 訂閱電子報
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public CiResult SubEpaper(string email)
        {
            var result = new CiResult();

            if (string.IsNullOrEmpty(email))
            {
                result.Message = string.Format(SystemMessage.FieldNull, "");
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                var user = Query.FirstOrDefault(x => x.Email == email);
                //新增user
                if (user == null)
                {
                    var createResult = Create(new UserViewModel
                    {
                        User = new mgt_User
                        {
                            Email = email,
                            AccountType = (int)AccountType.None,
                            UserStatus = (int)UserStatus.None
                        },
                        LoginType = LoginType.None
                    });

                    result.IsSuccess = createResult.IsSuccess;
                }
                //舊user               
                else
                {
                    user.IsReceiveEpaper = true;
                    Db.SaveChanges();

                    result.IsSuccess = true;
                }
            }

            result.Message = result.IsSuccess ? SystemMessage.Success : string.Format(SystemMessage.FieldNull, "");

            return result;
        }


        /// <summary>
        /// 加入User變更紀錄
        /// </summary>
        /// <param name="orderID">The order identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="userID">The user identifier.</param>
        public void AddLog(Guid userID, UserLogType logType,
                            string modelName, object model, Guid creater)
        {
            var data = new mgt_UserLog
            {
                UserID = userID,
                UserLogType = (int)logType,
                ModelName = modelName,
                DataContent = _Crypto.EncryptAES(_Json.ModelToJson(model), Setting.UserCryptoKey),
                CreateTime = DateTime.Now,
                CreateUser = userID
            };
            Db.mgt_UserLog.Add(data);
        }

        #endregion

        #region MyChild     

        #region 後端管理
        private IQueryable<mgt_UserProfile> AllChildQuery
        {
            get
            {
                return Db.mgt_UserProfile
                    .Where(x => x.IsMyChild && x.CreateUser != null
                             && x.mgt_UserCreate.ClientID == ClientID);
            }
        }

        public PageModel<mgt_UserProfile> GetChildList(PageParameter param, UserFilter filter)
        {
            var query = AllChildQuery.ToList();

            //搜尋關鍵字
            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                //身分證    
                var IdentityCardEn = _Crypto.EncryptAES(filter.SearchString, Setting.UserCryptoKey);

                query = query.Where(x => x.NickName.Contains(filter.SearchString)
                                   || x.mgt_UserCreate.Name.Contains(filter.SearchString)
                                   || x.IdentityCard == IdentityCardEn
                                   ).ToList();
            }


            var pagedModel = PageTool.CreatePage(query, param);

            //身分證解密
            foreach (var data in pagedModel.Data)
            {
                data.IdentityCard = _Crypto.DecryptAES(data.IdentityCard, Setting.UserCryptoKey);
            }

            return pagedModel;
        }

        /// <summary>
        /// 取得孩子
        /// </summary>
        /// <returns></returns>
        public mgt_UserProfile GetChild(Guid id)
        {
            var query = AllChildQuery.FirstOrDefault(x => x.ID == id);

            //身分證解密            
            query.IdentityCard = _Crypto.DecryptAES(query.IdentityCard, Setting.UserCryptoKey);

            return query;
        }

        /// <summary>
        /// 修改孩子
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public CiResult UpdateChild(mgt_UserProfile model, string userMail = "")
        {
            var result = new CiResult();

            try
            {
                //不檢查必填

                var IdentityCardEn = _Crypto.EncryptAES(model.IdentityCard, Setting.UserCryptoKey);

                //身分證不重複 
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.IdentityCard))
                    {
                        var exist = CheckIdentityCard(IdentityCardEn, model.ID);
                        if (exist)
                            result.Message = string.Format(SystemMessage.IdCardExist, model.IdentityCard);
                    }
                }


                //更換建立人
                Guid? CreateUser = null;
                if (string.IsNullOrEmpty(result.Message)
                    && !string.IsNullOrEmpty(userMail))
                {
                    userMail = userMail.Trim();
                    var user = GetByEmail(userMail);
                    if (user == null)
                    {
                        result.Message = SystemMessage.EmailNotFound;
                    }
                    else
                    {
                        CreateUser = user?.ID;
                    }
                }

                if (string.IsNullOrEmpty(result.Message))
                {
                    //不檢查身分證
                    result = UpdateUserProfile(model.ID, model, Setting.UserCryptoKey, checkIdCard: false, contentDelete: true, CreateUser: CreateUser);
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("User_CreateMyChild" + _Json.ModelToJson(ex));
            }

            return result;
        }

        #endregion

        #region 建立
        private IQueryable<mgt_UserProfile> ChildQuery(Guid createUser)
        {
            return Db.mgt_UserProfile
                .Include(x => x.mgt_UserCreate)
                .Include(x => x.mgt_UserAssign)
                .Where(x => x.CreateUser == createUser && x.IsMyChild && !x.IsDelete);
        }

        /// <summary>
        /// 家長建立的孩子們
        /// </summary>  
        public List<mgt_UserProfile> GetMyChildList(Guid createUser)
        {
            var query = ChildQuery(createUser).OrderBy(x => x.CreateTime).ToList();

            //身分證解密
            foreach (var data in query)
            {
                data.IdentityCard = _Crypto.DecryptAES(data.IdentityCard, Setting.UserCryptoKey);
            }

            return query;
        }

        /// <summary>
        /// 取得孩子
        /// </summary>
        /// <returns></returns>
        public mgt_UserProfile GetMyChild(Guid createUser, Guid id, bool decrypt = false)
        {
            var query = ChildQuery(createUser).FirstOrDefault(x => x.ID == id);

            if (decrypt)
            {
                //身分證解密            
                query.IdentityCard = _Crypto.DecryptAES(query.IdentityCard, Setting.UserCryptoKey);
            }

            return query;
        }

        /// <summary>
        /// 新增孩子
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult CreateMyChild(mgt_UserProfile model)
        {
            var result = new CiResult();

            try
            {
                #region 必填

                if (string.IsNullOrEmpty(result.Message)
                    && string.IsNullOrEmpty(model.NickName))
                {
                    result.Message = string.Format(SystemMessage.FieldNull, UserRequiredType.Name.GetDisplayName());
                }

                if (string.IsNullOrEmpty(result.Message)
                    && string.IsNullOrEmpty(model.IdentityCard))
                {
                    result.Message = string.Format(SystemMessage.FieldNull, UserRequiredType.IdentityCard.GetDisplayName());
                }

                if (string.IsNullOrEmpty(result.Message)
                    && model.Birthday == null)
                {
                    result.Message = string.Format(SystemMessage.FieldNull, UserRequiredType.Birthday.GetDisplayName());
                }

                if (string.IsNullOrEmpty(result.Message)
                    && model.Gender == 0)
                {
                    result.Message = string.Format(SystemMessage.FieldNull, UserRequiredType.Gender.GetDisplayName());
                }

                #endregion

                var IdentityCardEn = _Crypto.EncryptAES(model.IdentityCard, Setting.UserCryptoKey);

                //身分證不重複 
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (!string.IsNullOrEmpty(model.IdentityCard))
                    {
                        var exist = CheckIdentityCard(IdentityCardEn, null);
                        if (exist)
                            result.Message = string.Format(SystemMessage.IdCardExist, model.IdentityCard);
                    }
                }

                if (string.IsNullOrEmpty(result.Message))
                {
                    model.CreateUser = model.CreateUser;
                    model.IsMyChild = true;

                    result = UpdateUserProfile(null, model, Setting.UserCryptoKey);

                    if (result.IsSuccess)
                    {
                        result.Message = SystemMessage.CreateSuccess;
                    }

                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("User_CreateMyChild" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 新增孩子
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult UpdateMyChild(mgt_UserProfile model)
        {
            var result = new CiResult();

            try
            {
                #region 必填

                if (string.IsNullOrEmpty(result.Message)
                    && string.IsNullOrEmpty(model.NickName))
                {
                    result.Message = string.Format(SystemMessage.FieldNull, UserRequiredType.Name.GetDisplayName());
                }

                //if (string.IsNullOrEmpty(result.Message)
                //    && string.IsNullOrEmpty(model.IdentityCard))
                //{
                //    result.Message = string.Format(SystemMessage.FieldNull, UserRequiredType.IdentityCard.GetDisplayName());
                //}

                if (string.IsNullOrEmpty(result.Message)
                    && model.Birthday == null)
                {
                    result.Message = string.Format(SystemMessage.FieldNull, UserRequiredType.Birthday.GetDisplayName());
                }

                if (string.IsNullOrEmpty(result.Message)
                    && model.Gender == 0)
                {
                    result.Message = string.Format(SystemMessage.FieldNull, UserRequiredType.Gender.GetDisplayName());
                }

                #endregion

                //var IdentityCardEn = _Crypto.EncryptAES(model.IdentityCard, Setting.UserCryptoKey);

                ////身分證不重複 
                //if (string.IsNullOrEmpty(result.Message))
                //{
                //    if (!string.IsNullOrEmpty(model.IdentityCard))
                //    {
                //        var exist = CheckIdentityCard(IdentityCardEn, model.ID);
                //        if (exist)
                //            result.Message = string.Format(SystemMessage.IdCardExist, model.IdentityCard);
                //    }
                //}

                if (string.IsNullOrEmpty(result.Message))
                {
                    //result = UpdateUserProfile(model.ID, model, Setting.UserCryptoKey);

                    var entity = Db.mgt_UserProfile.Find(model.ID);

                    //修改者必須為建立者
                    if (entity.CreateUser != model.CreateUser)
                    {
                        result.Message = SystemMessage.Error;
                    }
                    else
                    {
                        //update
                        entity.NickName = model.NickName.ToTrim();
                        entity.Birthday = model.Birthday;
                        entity.Gender = model.Gender;
                        entity.UpdateTime = DateTime.Now;

                        Db.SaveChanges();

                        result.Message = SystemMessage.UpdateSuccess;
                        result.IsSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("User_CreateMyChild" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 刪除孩子
        /// </summary>
        /// <param name="createUser">The create user.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public CiResult DeleteMyChild(Guid createUser, Guid id)
        {
            var result = new CiResult();
            try
            {
                var query = GetMyChild(createUser, id);
                query.NickName = "(Delete)" + query.NickName;
                query.IsDelete = true;

                //todo 有指派教練、參賽記錄不可刪除

                Db.SaveChanges();
                result.Message = SystemMessage.DeleteSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.DeleteFail;
                _Log.CreateText("User_DeleteMyChild" + _Json.ModelToJson(ex));
            }
            return result;
        }

        /// <summary>
        /// 選手身分證不重複
        /// </summary>
        /// <param name="numberEn">The number en.</param>
        /// <returns></returns>
        private bool CheckIdentityCard(string numberEn, Guid? id)
        {
            var data = Db.mgt_UserProfile
                .Where(x => x.ClientID == ClientID && x.IsMyChild);

            // 排除自己
            if (id != null)
            {
                data = data.Where(x => x.ID != id);
            }

            bool result = (data.FirstOrDefault(x => x.IdentityCard == numberEn)) != null;

            return result;
        }

        #endregion

        #region 指派
        /// <summary>
        /// 指派列表from
        /// </summary>     
        /// <param name="fromUserID">擁有人</param>
        /// <param name="memberID">選手</param>
        /// <returns></returns>
        public List<mgt_UserAssign> GetUserAssignFromList(Guid createUser, Guid memberID)
        {
            var query = Db.mgt_UserAssign.Where(x => !x.IsDelete
                            && x.UserProfileID == memberID
                            && x.mgt_UserProfile.CreateUser == createUser);

            query = query.OrderBy(x => x.CreateTime);

            return query.ToList();
        }

        /// <summary>
        /// 指派列表to
        /// </summary>     
        /// <param name="toUserID">被授權人</param>
        /// <param name="memberID">選手</param>
        /// <param name="startBirthday">篩選生日</param>
        /// <param name="noneItemID">篩選未在比賽中出現</param>
        /// <param name="orderDetailID">篩選明細</param>
        /// <returns></returns>
        public PageModel<UserAssignViewModel> GetUserAssignToList(PageParameter param, Guid toUserID)
        {
            var query = Db.mgt_UserAssign.Where(x => x.ToUser == toUserID && !x.IsDelete);

            param.SortColumn = SortColumn.CreateTime;
            var pagedModel = PageTool.CreatePage(query.ToList(), param);
            var model = new PageModel<UserAssignViewModel>
            {
                CurrentPage = pagedModel.CurrentPage,
                PageSize = pagedModel.PageSize,
                TotalCount = pagedModel.TotalCount,
                PageCount = pagedModel.PageCount,
                DataStart = pagedModel.DataStart,
                DataEnd = pagedModel.DataEnd,
                Data = pagedModel.Data.Select(x => ToViewModel(x, DataLevel.Normal)).ToList()
            };
            return model;
        }

        /// <summary>
        /// 指派列表to
        /// </summary>     
        /// <param name="toUserID">被授權人</param>
        /// <param name="memberID">選手</param>
        /// <param name="noneItemID">篩選未在比賽中出現</param>
        /// <param name="butOrderDetailID">篩選明細</param>
        /// <param name="startBirthday">篩選生日</param>
        /// <returns></returns>
        public List<mgt_UserProfile> GetUserAssignToList(Guid toUserID, Guid? noneItemID = null, Guid? butOrderDetailID = null, Guid? noneParentItemID = null, DateTime? startBirthday = null)
        {
            var query = Db.mgt_UserAssign.Where(x => x.ToUser == toUserID && !x.IsDelete)
                          .Select(x => x.mgt_UserProfile).Where(x => !x.IsDelete);

            //日期以後出生
            if (startBirthday != null)
            {
                query = query.Where(x => x.Birthday != null && x.Birthday >= startBirthday.Value);
            }

            //篩選可選擇的選手
            if (noneItemID != null && butOrderDetailID != null)
            {
                if (noneParentItemID != null)
                {
                    query = query.Where(x =>
                    //未在比賽中出現過(不分盃賽)
                    !(x.mgt_UserProfileCopies.Any(y => y.erp_Order.ItemID == noneItemID) ||
                      x.mgt_UserProfileCopies.Any(y => y.erp_Order.cms_Item.ParentItemRelations.Any(z => z.ParentItem.ID == noneParentItemID))
                    )
                    //已經在明細中的可
                    || x.mgt_UserProfileCopies.Any(z => z.erp_OrderDetail.Any(y => y.ID == butOrderDetailID)));
                }
                else
                {
                    query = query.Where(x =>
                   //未在比賽中出現過
                   !x.mgt_UserProfileCopies.Any(y => y.erp_Order.ItemID == noneItemID)
                   //已經在明細中的可
                   || x.mgt_UserProfileCopies.Any(z => z.erp_OrderDetail.Any(y => y.ID == butOrderDetailID)));
                }
            }

            query = query.OrderBy(x => x.CreateTime);

            return query.ToList();
        }


        /// <summary>
        /// 取得指派
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public UserAssignViewModel GetUserAssign(Guid id)
        {
            var query = Db.mgt_UserAssign.Find(id);
            return ToViewModel(query);
        }

        private UserAssignViewModel ToViewModel(mgt_UserAssign userAssign, DataLevel dataLevel = DataLevel.Simple)
        {
            var model = new UserAssignViewModel
            {
                UserAssign = userAssign,
                Member = userAssign.mgt_UserProfile,
                FromUser = userAssign.mgt_UserFrom,
                ToUser = userAssign.mgt_UserTo
            };

            //解密  
            if (dataLevel > DataLevel.Simple)
            {
                //聯絡人電話
                model.FromUserPhone = _Crypto.DecryptAES(model.FromUser.Phone, Setting.UserCryptoKey);
            }

            return model;
        }


        /// <summary>
        /// 建立指派
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult CreateAssign(mgt_UserAssign model)
        {
            var result = new CiResult();
            try
            {
                //check 已存在              
                var data = Db.mgt_UserAssign.FirstOrDefault(x => x.UserProfileID == model.UserProfileID
                                                 //&& x.FromUser == model.FromUser
                                                 && x.ToUser == model.ToUser
                                                 && !x.IsDelete);

                if (data != null)
                {
                    result.Message = string.Format(SystemMessage.FieldExist, "資料");
                }

                if (string.IsNullOrEmpty(result.Message))
                {
                    //check detail
                    var member = GetMyChild(model.FromUser, model.UserProfileID);
                    var fromUser = Get(model.FromUser);
                    var toUser = Get(model.ToUser);

                    if (member == null || fromUser.ClientID != toUser.ClientID)
                    {
                        result.Message = SystemMessage.Error;
                    }
                }

                if (string.IsNullOrEmpty(result.Message))
                {
                    model.ID = Guid.NewGuid();
                    model.CreateTime = DateTime.Now;

                    Db.mgt_UserAssign.Add(model);

                    Db.SaveChanges();
                    result.Message = SystemMessage.CreateSuccess;
                    result.IsSuccess = true;
                    result.ID = model.ID;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("User_CreateAssign" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 刪除指派 (已參加的比賽並不會一併刪除)
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult DeleteAssign(Guid createUser, Guid id)
        {
            var result = new CiResult();
            try
            {
                var data = Db.mgt_UserAssign.FirstOrDefault(x => x.ID == id && x.mgt_UserProfile.CreateUser == createUser
                                                 && !x.IsDelete);

                if (data != null)
                {
                    data.IsDelete = true;
                    data.DeleteTime = DateTime.Now;

                    Db.SaveChanges();
                    result.Message = SystemMessage.DeleteSuccess;
                    result.IsSuccess = true;
                }
                else
                {
                    result.Message = SystemMessage.DeleteFail;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.DeleteFail;
                _Log.CreateText("User_DeleteAssign" + _Json.ModelToJson(ex));
            }

            return result;
        }

        #endregion

        /// <summary>
        /// 通知家長報名完成 update IsSend (取得未寄送過的)
        /// </summary>
        /// <returns>原始選手</returns>
        public CiResult<List<mgt_UserProfile>> SendMemberEmails(Guid orderDetail)
        {
            var result = new CiResult<List<mgt_UserProfile>>();
            try
            {
                var query = Db.erp_OrderDetail.Include(x => x.TeamMembers)
                    .FirstOrDefault(x => x.ID == orderDetail)
                    .TeamMembers.Where(x => !x.IsDelete && !x.IsSend);

                var data = query.Select(x => x.mgt_UserProfileSource).ToList();
                result.Data = data;

                foreach (var item in query)
                {
                    item.IsSend = true;
                }

                Db.SaveChanges();
                result.IsSuccess = true;

            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.DeleteFail;
                _Log.CreateText("User_SendMemberEmails" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 我的孩子參賽紀錄
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="createUser">The create user.</param>
        /// <returns></returns>
        public PageModel<MyChildrenRecordModel> GetMyChildOrderList(PageParameter param, Guid fromUserID)
        {
            var query = Db.mgt_UserAssign.Where(x => x.FromUser == fromUserID && !x.IsDelete)
                .Select(x => x.mgt_UserProfile).Distinct()
                .SelectMany(x => x.mgt_UserProfileCopies);

            param.SortColumn = SortColumn.CreateTime;
            var pagedModel = PageTool.CreatePage(query.ToList(), param);
            var model = new PageModel<MyChildrenRecordModel>
            {
                CurrentPage = pagedModel.CurrentPage,
                PageSize = pagedModel.PageSize,
                TotalCount = pagedModel.TotalCount,
                PageCount = pagedModel.PageCount,
                DataStart = pagedModel.DataStart,
                DataEnd = pagedModel.DataEnd,
                Data = pagedModel.Data.Select(x => ToViewModel(x)).ToList()
            };

            return model;
        }

        private MyChildrenRecordModel ToViewModel(mgt_UserProfile userProfile)
        {
            var detail = userProfile.erp_OrderDetail.FirstOrDefault();
            var unit = detail.erp_OrderUnit.FirstOrDefault();

            var model = new MyChildrenRecordModel
            {
                MemberName = userProfile.NickName,
                Unit = unit?.Unit,
                Coach = detail.erp_Order.mgt_User.Name,//creater
                Subject = detail.erp_Order.cms_Item.cms_ItemLanguage.FirstOrDefault().Subject,
                ParentSubject = detail.erp_Order.cms_Item.ParentItemRelations.FirstOrDefault(x => x.IsCrumb)?.ParentItem?.cms_ItemLanguage.FirstOrDefault().Subject,

                Option = detail.ItemSubject,
                OrderNumber = detail.erp_Order.OrderNumber,
                OrderStatus = detail.OrderStatus,
                CreateTime = userProfile.CreateTime
            };

            return model;
        }
        #endregion

        #region Check

        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult<mgt_User> CheckLogin(LoginViewModel model)
        {
            var result = new CiResult<mgt_User>();
            var user = new mgt_User();
            var query = Db.mgt_User.Where(x => x.UserStatus != (int)UserStatus.Delete && x.UserStatus != (int)UserStatus.None
                                            && x.AccountType == (int)model.AccountType);//SuperManager可跨任系統

            //檢查必填 (Account=自訂帳號/Email/外部登入Key)
            if (string.IsNullOrWhiteSpace(model.Account)
               || ((model.LoginType == LoginType.Account || model.LoginType == LoginType.Email) && string.IsNullOrWhiteSpace(model.Password)))
            {
                result.Message = string.Format(SystemMessage.FieldNull, "");
                return result;
            }


            //1.帳號登入
            var passHash = _Crypto.HashSHA256(model.Password);
            if (model.AccountType == AccountType.Admin && model.LoginType == LoginType.Account)
            {
                user = query.FirstOrDefault(x => x.Account == model.Account && x.Password == passHash
                                             //mgt_User屬於Client or IsSuperManager
                                             && (x.ClientID == ClientID || x.ID == Setting.SuperManagerID));
            }
            //2.Email登入
            else if (model.AccountType == AccountType.Member && model.LoginType == LoginType.Email)
            {
                user = query.FirstOrDefault(x => x.Email == model.Account && x.Password == passHash
                                              && x.ClientID == ClientID);
                //Email驗證未通過
                if (user != null && !user.EmailIsVerify)
                {
                    result.Message = SystemMessage.LoginNotEnabled;
                }
            }
            //3.外部登入
            else if (model.AccountType == AccountType.Member && model.LoginType == LoginType.Facebook)
            {
                user = query.FirstOrDefault(x => x.mgt_UserExternalLogin
                .Any(y => y.ExternalType == (int)model.LoginType && y.ExternalKey == model.Account)
                                              && x.ClientID == ClientID);
                //Email驗證未通過
                if (user != null && !user.EmailIsVerify)
                {
                    result.Message = SystemMessage.LoginNotEnabled;
                }
            }


            try
            {
                if (user == null)
                {
                    result.Message = SystemMessage.LoginFail;
                }
                else if (user.UserStatus != (int)UserStatus.Enabled //啟用中
                      || !user.LoginTypes.HasValue((int)model.LoginType)) //可登入方式
                {
                    result.Message = SystemMessage.LoginNotEnabled;
                }
                else
                {
                    user.LastLoginTime = DateTime.Now;
                    user.LastLoginIP = model.IP;
                    Db.SaveChanges();

                    result.Data = user;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.LoginError;
                _Log.CreateText("Account_Login:" + _Json.ModelToJson(ex));
            }
            return result;
        }

        /// <summary>
        /// 外部登入存在
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public bool ExistExternalLogin(ExternalLoginViewModel model)
        {
            var query = Query.FirstOrDefault(x => x.mgt_UserExternalLogin
                            .Any(y => y.ExternalKey == model.ExternalKey && y.ExternalType == (int)model.ExternalType));

            return (query != null);

            #region 新增User
            //if (query == null)
            //{
            //var createResult = Create(new UserViewModel
            //{
            //    User = new mgt_User
            //    {
            //        AccountType = (int)AccountType.Member,
            //        UserStatus = (int)UserStatus.Enabled,
            //        Name = model.UserName,
            //        Email = model.Email,
            //    },
            //    UserExternalLogin = new mgt_UserExternalLogin
            //    {
            //        ExternalType = (int)model.ExternalType,
            //        ExternalKey = model.ExternalKey
            //    },
            //    LoginType = (LoginType)model.ExternalType
            //});
            //result = createResult;
            //}
            //舊user
            //else
            //{
            //    result.Data = query.ID;
            //    result.IsSuccess = true;
            //}

            //return result;

            #endregion
        }

        /// <summary>
        /// 是否有密碼
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public bool ExistPassword(Guid id)
        {
            var user = Get(id);
            return (user.LoginTypes.HasValue((int)LoginType.Email)
                || user.LoginTypes.HasValue((int)LoginType.Account));
        }

        /// <summary>
        /// 帳號是否重複 (全站不重複)
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private bool CheckAccount(string account, Guid? id)
        {
            //全站不重複
            var data = Db.mgt_User.Where(x => x.UserStatus != (int)UserStatus.Delete);

            // 排除自己
            if (id != null)
            {
                data = data.Where(x => x.ID != id);
                //// 查無帳號
                //if (Get(id.Value) == null)
                //{
                //    return true;
                //}
            }

            bool result = (data.FirstOrDefault(x => x.Account == account)) != null;

            return result;
        }

        /// <summary>
        /// Email是否重複 (站內不重複)
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private bool CheckEmail(string email, Guid? id)
        {
            //全站不重複
            var data = Query;

            // 排除自己
            if (id != null)
            {
                data = data.Where(x => x.ID != id);
            }

            bool result = (data.FirstOrDefault(x => x.Email == email)) != null;

            return result;
        }

        /// <summary>
        /// 手機是否重複 (站內不重複) 加密後
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private bool CheckPhone(string phoneEn, Guid? id)
        {
            //全站不重複
            var data = Query;

            // 排除自己
            if (id != null)
            {
                data = data.Where(x => x.ID != id);
            }

            bool result = (data.FirstOrDefault(x => x.Phone == phoneEn)) != null;

            return result;
        }

        /// <summary>
        /// Check 必填欄位
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="requiredTypeList">The required type list.</param>
        /// <param name="updateEmail">if set to <c>true</c> [update email].</param>
        /// <returns></returns>
        private string CheckRequired(UserViewModel model, List<string> requiredTypeList, bool updateEmail = true, bool updatePhone = true)
        {
            string message = "";

            if (string.IsNullOrEmpty(message)
           && requiredTypeList.Any(x => x.HasValue((int)UserRequiredType.Name))
           && string.IsNullOrEmpty(model.User.Name))
            {
                message = string.Format(SystemMessage.FieldNull, UserRequiredType.Name.GetDisplayName());
            }

            if (string.IsNullOrEmpty(message) && updateEmail
                && requiredTypeList.Any(x => x.HasValue((int)UserRequiredType.Email))
                && string.IsNullOrEmpty(model.User.Email))
            {
                message = string.Format(SystemMessage.FieldNull, UserRequiredType.Email.GetDisplayName());
            }

            if (string.IsNullOrEmpty(message) && updatePhone
                && requiredTypeList.Any(x => x.HasValue((int)UserRequiredType.Phone))
                && string.IsNullOrEmpty(model.User.Phone))
            {
                message = string.Format(SystemMessage.FieldNull, UserRequiredType.Phone.GetDisplayName());
            }

            if (string.IsNullOrEmpty(message)
                && requiredTypeList.Any(x => x.HasValue((int)UserRequiredType.Address))
                && string.IsNullOrEmpty(model.User.Address))
            {
                message = string.Format(SystemMessage.FieldNull, UserRequiredType.Address.GetDisplayName());
            }

            //UserProfile            

            if (string.IsNullOrEmpty(message)
                && requiredTypeList.Any(x => x.HasValue((int)UserRequiredType.IdentityCard))
                && string.IsNullOrEmpty(model.UserProfile.IdentityCard))
            {
                message = string.Format(SystemMessage.FieldNull, UserRequiredType.IdentityCard.GetDisplayName());
            }

            if (string.IsNullOrEmpty(message)
                && requiredTypeList.Any(x => x.HasValue((int)UserRequiredType.Birthday))
                && model.UserProfile.Birthday == null)
            {
                message = string.Format(SystemMessage.FieldNull, UserRequiredType.Birthday.GetDisplayName());
            }

            if (string.IsNullOrEmpty(message)
                && requiredTypeList.Any(x => x.HasValue((int)UserRequiredType.Gender))
                && model.UserProfile.Gender == 0)
            {
                message = string.Format(SystemMessage.FieldNull, UserRequiredType.Gender.GetDisplayName());
            }

            if (string.IsNullOrEmpty(message)
           && requiredTypeList.Any(x => x.HasValue((int)UserRequiredType.Education))
           && string.IsNullOrEmpty(model.UserProfile.Education))
            {
                message = string.Format(SystemMessage.FieldNull, UserRequiredType.Education.GetDisplayName());
            }

            if (string.IsNullOrEmpty(message)
                && requiredTypeList.Any(x => x.HasValue((int)UserRequiredType.EmergencyContact))
                && string.IsNullOrEmpty(model.UserProfile.EmergencyContact))
            {
                message = string.Format(SystemMessage.FieldNull, UserRequiredType.EmergencyContact.GetDisplayName());
            }

            return message;
        }

        /// <summary>
        /// 新增選手前檢查
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public CiResult UserChildPreCheck(Guid userID)
        {
            var result = new CiResult();
            var user = Get(userID);

            //user完成Email驗證
            if (string.IsNullOrEmpty(result.Message)
                && !user.EmailIsVerify)
            {
                result.Message = SystemMessage.EmailNotEnabled;
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                result.IsSuccess = true;
            }

            return result;
        }
        #endregion

        #region VaildCode

        /// <summary>
        /// 產生驗證碼
        /// </summary>
        /// <param name="userID">userID</param>
        /// <param name="type">驗證類型</param>
        /// <param name="minute">有效時間(default: 1天)</param>
        /// <param name="length">驗證碼長度</param>
        /// <returns>驗證碼</returns>
        public CiResult<string> CreateValidCode(Guid userID, ValidType type, int minute = 1440, int length = 0)
        {
            CiResult<string> result = new CiResult<string>();

            try
            {
                string validCode = Guid.NewGuid().ToString();

                if (length != 0)
                {
                    int min = (int)Math.Pow(10, length - 1);//1000
                    int max = (int)Math.Pow(10, length) - 1;//9999
                    validCode = new Random().Next(min, max).ToString();
                }

                var data = new mgt_UserValidCode
                {
                    UserID = userID,
                    ValidCode = validCode,
                    ValidType = (int)type,
                    CreateTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(minute),
                    IsValid = false
                };
                Db.mgt_UserValidCode.Add(data);

                Db.SaveChanges();

                result.Data = data.ValidCode;
                result.Message = SystemMessage.CreateSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("Account_CreateValidCode:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 重寄驗證信檢查 (先check Email存在系統再寄)
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public CiResult<mgt_User> SendValidCodeCheck(SendEmailViewModel model)
        {
            var result = new CiResult<mgt_User>();

            if (string.IsNullOrEmpty(model.Email) || !_Check.IsEmail(model.Email))
            {
                result.Message = SystemMessage.EmailNotFound;
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                var user = Query.FirstOrDefault(x => x.Email == model.Email);
                //查無Email
                if (user == null)
                {
                    result.Message = SystemMessage.EmailNotFound;
                }
                //不需驗證
                else if (model.SystemMailType == SystemMailType.ConfirmEmail && user.EmailIsVerify && user.UserStatus == (int)UserStatus.Enabled)
                {
                    result.Message = SystemMessage.EmailValidSuccess;
                }
                //不可重設密碼(帳號未啟用、沒有Email帳號)
                else if (model.SystemMailType == SystemMailType.ForgotPassword
                        && (user.UserStatus != (int)UserStatus.Enabled || !user.EmailIsVerify || !user.LoginTypes.HasValue((int)LoginType.Email)))
                {
                    if (user.LoginTypes.HasValue((int)LoginType.Facebook))
                    {
                        result.Message = SystemMessage.AccoundToFbLogin;
                    }
                    else
                    {
                        result.Message = SystemMessage.AccoundNotEnabled;
                    }
                }
                else
                {
                    AddLog(user.ID, model.SystemMailType == SystemMailType.ConfirmEmail ? UserLogType.SendConfirmEmail :
                                    model.SystemMailType == SystemMailType.ForgotPassword ? UserLogType.SendForgotPassword : UserLogType.SendValidCode
                            , null, null, user.ID);
                    Db.SaveChanges();

                    result.IsSuccess = true;
                    result.Data = user;
                }
            }

            return result;
        }

        /// <summary>
        /// 寄簡訊驗證碼
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult<mgt_User> SendSmsValidCodeCheck(string phone)
        {
            var result = new CiResult<mgt_User>();

            if (string.IsNullOrEmpty(phone) || !_Check.IsPhone(phone))
            {
                result.Message = SystemMessage.PhoneNotFound;
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                var phoneEn = _Crypto.EncryptAES(phone, Setting.UserCryptoKey);
                var user = Query.FirstOrDefault(x => x.Phone == phoneEn);
                //查無號碼
                if (user == null)
                {
                    result.Message = SystemMessage.PhoneNotFound;
                }
                //不需驗證
                else if (user.PhoneIsVerify)
                {
                    result.Message = SystemMessage.PhoneValidSuccess;
                }
                else
                {
                    AddLog(user.ID, UserLogType.SendConfirmPhone, null, null, user.ID);
                    Db.SaveChanges();

                    result.IsSuccess = true;
                    result.Data = user;
                }
            }

            return result;
        }

        /// <summary>
        /// 檢查驗證碼(mail link)
        /// </summary>
        /// <param name="code">guid</param>
        /// <param name="key">email or phone</param>
        /// <returns></returns>
        public CiResult<ValidCodeResultViewModel> CheckValidCode(ValidCodeViewModel model)
        {
            var result = new CiResult<ValidCodeResultViewModel> { Data = new ValidCodeResultViewModel() };
            var validType = ValidType.ConfirmEmail;
            var user = new mgt_User();
            var validCode = new mgt_UserValidCode();
            bool isEmail = _Check.IsEmail(model.Key);
            bool isPhone = _Check.IsPhone(model.Key);

            //格式錯誤
            if (string.IsNullOrWhiteSpace(model.Code) || string.IsNullOrWhiteSpace(model.Key) ||
               !_Check.NobadCharacters(model.Code) || !(isEmail || isPhone) || !Enum.TryParse(model.Type, true, out validType))//enum ignoreCase 比對大小寫不同
            {
                result.Message = SystemMessage.ValidCodeWrong;
            }
            else
            {
                result.Data.ValidType = validType;
            }

            //User           
            if (string.IsNullOrEmpty(result.Message))
            {
                if (isEmail)
                {
                    user = Query.FirstOrDefault(x => x.Email == model.Key);
                }
                else if (isPhone)
                {
                    var phoneEn = _Crypto.EncryptAES(model.Key, Setting.UserCryptoKey);
                    user = Query.FirstOrDefault(x => x.Phone == phoneEn);
                }
                //user email 不存在
                if (user == null)
                {
                    result.Message = SystemMessage.ValidCodeWrong;
                }
                //Email已驗證過，不再重複驗證
                else if (validType == ValidType.ConfirmEmail && user.EmailIsVerify && user.UserStatus == (int)UserStatus.Enabled)
                {
                    result.Message = SystemMessage.EmailValidSuccess;
                    result.IsSuccess = true;
                }
                else if (validType == ValidType.ConfirmPhone && user.PhoneIsVerify)
                {
                    result.IsSuccess = true;
                }
                else
                {
                    result.Data.UserID = user.ID;
                }
            }

            //驗證碼           
            if (string.IsNullOrEmpty(result.Message))
            {
                var nowtime = DateTime.Now;
                validCode = Db.mgt_UserValidCode.Where(x =>
                                     //驗證碼符合
                                     x.ValidType == (int)validType && x.ValidCode == model.Code && x.UserID == user.ID
                                     //未過期
                                     && x.EndTime > nowtime && !x.IsValid).AsEnumerable().LastOrDefault();

                if (validCode == null)
                {
                    result.Message = isEmail ? SystemMessage.ValidCodUrlError : SystemMessage.ValidCodError;
                }
            }

            //success
            if (string.IsNullOrEmpty(result.Message))
            {
                try
                {
                    validCode.ValidTime = DateTime.Now;
                    validCode.IsValid = true;

                    switch (validType)
                    {
                        case ValidType.ConfirmEmail:
                            user.EmailIsVerify = true;
                            user.UserStatus = (int)UserStatus.Enabled;
                            AddLog(user.ID, UserLogType.EnabledEmail, null, null, user.ID);

                            //LoginType
                            UpdateLoginType(user);

                            result.Message = SystemMessage.EmailValidSuccess;
                            result.IsSuccess = true;
                            break;

                        case ValidType.ConfirmPhone:
                            user.PhoneIsVerify = true;
                            AddLog(user.ID, UserLogType.EnabledPhone, null, null, user.ID);

                            result.Message = SystemMessage.PhoneValidSuccess;
                            result.IsSuccess = true;
                            break;

                        case ValidType.ForgotPassword:

                            result.Message = SystemMessage.PasswordValidSuccess;
                            result.IsSuccess = true;
                            break;
                    }

                    Db.SaveChanges();
                }
                catch (Exception ex)
                {
                    result.Message = SystemMessage.ErrorAndHelp;
                    _Log.CreateText("CheckValidCode:" + _Json.ModelToJson(ex));
                }
            }

            return result;
        }

        #endregion

        #region UserRoleRelation

        /// <summary>
        /// 修改帳號角色 (for Admin: 全部移除重增)
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="roleCheck">The role check.</param>
        /// <returns></returns>
        private void UpdateUserRoles(mgt_User user, List<Guid> roleIDs)
        {
            var deletes = user.mgt_UserRoleRelation.ToList();
            if (deletes != null)
            {
                Db.mgt_UserRoleRelation.RemoveRange(deletes);
            }

            foreach (var id in roleIDs)
            {
                user.mgt_UserRoleRelation.Add(
                    new mgt_UserRoleRelation
                    {
                        ID = Guid.NewGuid(),
                        RoleID = id,
                        CreateTime = DateTime.Now,
                        IsEnabled = true
                    });
            }
        }

        /// <summary>
        /// 自動新增Member角色 (from 新會員) 訂單申請身分在Order內 add Role
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="AddRoleID">The add role identifier.</param>
        private void AddUserMemberRoles(mgt_User user, List<Guid> roleIDs, Guid? adminID = null)
        {
            //user.mgt_UserRoleRelation.Clear();

            foreach (var id in roleIDs)
            {
                if (!user.mgt_UserRoleRelation.Any(x => x.RoleID == id && !x.IsDelete))
                {
                    var role = new mgt_UserRoleRelation
                    {
                        ID = Guid.NewGuid(),
                        RoleID = id,
                        CreateTime = DateTime.Now,
                        IsEnabled = true,
                    };
                    user.mgt_UserRoleRelation.Add(role);

                    AddLog(user.ID, UserLogType.CreateRole, "mgt_UserRoleRelation", role, adminID == null ? user.ID : adminID.Value);
                }
            }
        }


        //---會員身分編輯(身分可重複)---    

        public mgt_UserRoleRelation GetRoleRelation(Guid id)
        {
            return Db.mgt_UserRoleRelation.Find(id);
        }

        public CiResult CreateUserMemberRoles(mgt_UserRoleRelation model, Guid adminID, bool isEnabled = true)
        {
            CiResult result = new CiResult();
            //var data = GetRoleRelation(model.UserID, model.RoleID);

            try
            {
                //身分不可重複
                //if (data != null)
                //{
                //    result.Message = string.Format(SystemMessage.FielExist, "身分");
                //}

                if (string.IsNullOrEmpty(result.Message))
                {
                    model.ID = Guid.NewGuid();
                    model.CreateTime = DateTime.Now;
                    model.IsEnabled = isEnabled;
                    Db.mgt_UserRoleRelation.Add(model);

                    AddLog(model.UserID, UserLogType.CreateRole, "mgt_UserRoleRelation", model, adminID);
                    Db.SaveChanges();

                    result.Message = SystemMessage.CreateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("User_CreateUserMemberRoles:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult UpdateUserMemberRoles(mgt_UserRoleRelation model, Guid adminID)
        {
            CiResult result = new CiResult();
            var data = GetRoleRelation(model.ID);

            try
            {
                //data.RoleID = model.RoleID; //role不可修改、不可重複
                data.RoleNumber = model.RoleNumber;
                data.IsTimeLimited = model.IsTimeLimited;
                data.StartTime = model.IsTimeLimited ? model.StartTime : null;
                data.EndTime = model.IsTimeLimited ? model.EndTime : null;
                data.IsEnabled = model.IsEnabled;

                AddLog(model.UserID, UserLogType.UpdateRole, "mgt_UserRoleRelation", model, adminID);
                Db.SaveChanges();

                result.Message = SystemMessage.UpdateSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("User_UpdateUserMemberRoles:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult DeleteUserMemberRoles(Guid id, Guid adminID)
        {
            CiResult result = new CiResult();
            var data = GetRoleRelation(id);

            try
            {
                //Db.mgt_UserRoleRelation.Remove(data);
                data.IsDelete = true;

                AddLog(data.UserID, UserLogType.DeleteRole, "mgt_UserRoleRelation", data, adminID);
                Db.SaveChanges();

                result.Message = SystemMessage.DeleteSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.DeleteFail;
                _Log.CreateText("User_DeleteUserMemberRoles:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        #endregion
    }
}
