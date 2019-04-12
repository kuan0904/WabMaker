
--[mgt_UserLog] 修改 
--[mgt_UserRoleRelation] 修改 
--CodeNumber -> RoleNumber

update mgt_UserRoleRelation
set CreateTime = (select CreateTime from mgt_User where mgt_UserRoleRelation.UserID = mgt_User.ID)

--[mgt_Client] 修改、add資料
--SystemMailTypes -> ClientSetting


--[mgt_Position] 整個刪掉
--[mgt_UserPositionRelation] 整個刪掉
--mgt_User add DepartmentID 加關聯


--[Sructure] add OrderErrorReturnPage

update cms_Structure
set OrderErrorReturnPage=0


--[mgt_UserProfile]
--add UnitAddress
--add Sports
--SignFilePath -> Referrer


--[Order]
--add RoleNumber
--add FilePath
--設計 -> 索引: 加入OrderNumber 叢集索引(建立成CLUSTERED) CX_erp_Order_OrderNumber


--[Item]
--add CreateUser not null
--add DepartmentID
--add Options nvarchar(2000)
--add PeopleMin
--add PeopleMax

update cms_Item set CreateUser = UpdateUser
update cms_Item set PeopleMin = 0, PeopleMax=0

--Item Department關聯
--索引 IX_cms_Items 刪除，已經有RouteName IX_cms_Item_RouteName

--8geman structure設定 訂單返回頁面
--換一下super密碼


-------------------------------------------------------------
---------------------以上2018/12/4完成------------------------

--//---訂單新增欄位---
ALTER TABLE  [WebMakerDB].[dbo].[erp_Order]
ADD TeamName nvarchar(100),
	Coach nvarchar(100),
	Leader nvarchar(100),
	Manager nvarchar(100);

EXECUTE sp_addextendedproperty
N'MS_Description', N'團隊名稱', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'TeamName'
GO
EXECUTE sp_addextendedproperty
N'MS_Description', N'教練', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'Coach'
GO
EXECUTE sp_addextendedproperty
N'MS_Description', N'領隊', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'Leader'
GO
EXECUTE sp_addextendedproperty
N'MS_Description', N'管理', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'Manager'
GO


--//---團隊成員---
ALTER TABLE  [WebMakerDB].[dbo].[mgt_UserProfile]
ADD OrderID uniqueidentifier

ALTER TABLE [mgt_UserProfile] 
WITH CHECK ADD CONSTRAINT [FK_mgt_UserProfile_erp_Order]
FOREIGN KEY(OrderID)
REFERENCES [erp_Order] (ID)
GO

EXECUTE sp_addextendedproperty --sp_updateextendedproperty
N'MS_Description', N'人物介紹文章', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'ItemID'
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'比賽團隊成員', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'OrderID'
GO



--//---項目與成員---
USE [WebMakerDB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[erp_OrderDetailTeam](
	[OrderDetailID] [uniqueidentifier] NOT NULL,
	[UserProfileID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_erp_OrderDetailTeam] PRIMARY KEY CLUSTERED 
(
	[OrderDetailID] ASC,
	[UserProfileID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[erp_OrderDetailTeam]  WITH CHECK ADD  CONSTRAINT [FK_erp_OrderDetailTeam_erp_OrderDetail] FOREIGN KEY([OrderDetailID])
REFERENCES [dbo].[erp_OrderDetail] ([ID])
GO

ALTER TABLE [dbo].[erp_OrderDetailTeam] CHECK CONSTRAINT [FK_erp_OrderDetailTeam_erp_OrderDetail]
GO

ALTER TABLE [dbo].[erp_OrderDetailTeam]  WITH CHECK ADD  CONSTRAINT [FK_erp_OrderDetailTeam_mgt_UserProfile] FOREIGN KEY([UserProfileID])
REFERENCES [dbo].[mgt_UserProfile] ([ID])
GO

ALTER TABLE [dbo].[erp_OrderDetailTeam] CHECK CONSTRAINT [FK_erp_OrderDetailTeam_mgt_UserProfile]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'項目ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderDetailTeam', @level2type=N'COLUMN',@level2name=N'OrderDetailID'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'選手ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderDetailTeam', @level2type=N'COLUMN',@level2name=N'UserProfileID'
GO



--//---項目Detail 檔案上傳---
ALTER TABLE  [WebMakerDB].[dbo].[erp_OrderDetail]
ADD FilePath nvarchar(200);

EXECUTE sp_addextendedproperty
N'MS_Description', N'檔案上傳', N'SCHEMA', N'dbo', N'TABLE', N'erp_OrderDetail', N'COLUMN', N'FilePath'
GO


--//---Item Creater 關聯---
ALTER TABLE [dbo].[cms_Item]  WITH CHECK ADD  CONSTRAINT [FK_cms_Item_mgt_User1] FOREIGN KEY([CreateUser])
REFERENCES [dbo].[mgt_User] ([ID])
GO

ALTER TABLE [dbo].[cms_Item] CHECK CONSTRAINT [FK_cms_Item_mgt_User1]
GO


EXECUTE sp_addextendedproperty 
N'MS_Description', N'建立者', N'SCHEMA', N'dbo', N'TABLE', N'cms_Item', N'COLUMN', N'CreateUser'
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'編輯者', N'SCHEMA', N'dbo', N'TABLE', N'cms_Item', N'COLUMN', N'UpdateUser'
GO

-------------------------------------------------------------
-------------------以上2018/12/11完成------------------------

--//---訂單文章---
ALTER TABLE  [WebMakerDB].[dbo].[erp_Order]
ADD ItemID uniqueidentifier

ALTER TABLE [erp_Order] 
WITH CHECK ADD CONSTRAINT [FK_erp_Order_cms_Item]
FOREIGN KEY(ItemID)
REFERENCES [cms_Item] (ID)
GO

EXECUTE sp_addextendedproperty --sp_updateextendedproperty
N'MS_Description', N'訂單文章', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'ItemID'
GO


update [erp_Order] set ItemID =
(select top 1 ItemID from [erp_OrderDetail] where OrderID = erp_Order.ID)


-------------------------------------------------------------
--------------------以上2018/12/12完成------------------------


ALTER TABLE  [WebMakerDB].[dbo].[erp_OrderDetail]
ADD [Option] nvarchar(200)

-------------------------------------------------------------
-------------------以上2018/12/18完成------------------------

 --//比賽文章 接到次分類
update [cms_StructureRelation]
set ParentID = 'cc3c3753-94fb-44b5-bf41-f7400054e6b8'
where parentID='25f7b5ba-782d-4659-ab2b-63d67cbe2d25'

--比賽文章 父:多層 sort:5
--比賽列表 分類、結構 關閉

-------------------------------------------------------------
--------------------以上2018/12/19完成------------------------
 --============================= ok ============================

--//---郵件樣板---
ALTER TABLE [cms_EmailTemplate]
ADD TemplateBcc nvarchar(4000),
	StructureID uniqueidentifier
GO

ALTER TABLE [cms_EmailTemplate] 
WITH CHECK ADD CONSTRAINT [FK_cms_EmailTemplate_cms_Structure]
FOREIGN KEY(StructureID)
REFERENCES [cms_Structure] (ID)
GO


EXECUTE sp_addextendedproperty
N'MS_Description', N'預設BCC', N'SCHEMA', N'dbo', N'TABLE', N'cms_EmailTemplate', N'COLUMN', N'TemplateBcc'
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'指定結構', N'SCHEMA', N'dbo', N'TABLE', N'cms_EmailTemplate', N'COLUMN', N'StructureID'
GO


--//---add戶籍地---
ALTER TABLE [mgt_UserProfile]
ADD HouseholdAddress nvarchar(500)
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'戶籍地', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'HouseholdAddress'
GO



--//---add戶籍地---
ALTER TABLE [cms_ItemOrderRoleRelation]
ADD ItemOrderRoleType int
GO

--允許身分
Update cms_ItemOrderRoleRelation set ItemOrderRoleType = 390; 

ALTER TABLE [cms_ItemOrderRoleRelation]
ALTER COLUMN ItemOrderRoleType int not null
GO


--update pk
ALTER TABLE [cms_ItemOrderRoleRelation]
DROP CONSTRAINT [PK_cms_ItemRoleRelation]
GO   
ALTER TABLE [cms_ItemOrderRoleRelation] ADD  CONSTRAINT [PK_cms_ItemOrderRoleRelation] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC,
	[RoleID] ASC,
	[ItemOrderRoleType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

--加入身分
insert into cms_ItemOrderRoleRelation
select ID, OrderAutoRole, 391 from cms_Item where OrderAutoRole is not null

---check data---

-- 刪除欄位
ALTER TABLE [cms_Item]
DROP CONSTRAINT FK_cms_Item_mgt_Role
GO   

ALTER TABLE [cms_Item]
DROP COLUMN OrderAutoRole


--//---Order add RoleID---
EXECUTE sp_updateextendedproperty
N'MS_Description', N'角色編號(證號)', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'RoleNumber'
GO

ALTER TABLE [erp_Order]
ADD RoleID uniqueidentifier
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'角色ID', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'RoleID'
GO

ALTER TABLE [erp_Order] 
WITH CHECK ADD CONSTRAINT [FK_erp_Order_mgt_Role]
FOREIGN KEY(RoleID)
REFERENCES [mgt_Role] (ID)
GO

-------------------以上2018/1/4完成------------------------
 --============================= ok ============================

