

--�ϥΪ��v�����p��: �إ�ID���D��, �[�J�q�����p, IsEnabled (�إ߭q���false,�ҥΫ�true)

--//---mgt_UserRoleRelation add ID as primary key
--add ID
ALTER TABLE [mgt_UserRoleRelation]
ADD ID uniqueidentifier
GO

Update [mgt_UserRoleRelation] set ID = NEWID(); 

ALTER TABLE [mgt_UserRoleRelation]
ALTER COLUMN ID uniqueidentifier not null
GO

--change primary key
ALTER TABLE [mgt_UserRoleRelation] DROP PK_CMS_UserRoleRelations
GO

ALTER TABLE [mgt_UserRoleRelation] ADD PRIMARY KEY(ID)
GO



--//---mgt_UserRoleRelation add OrderID---
ALTER TABLE mgt_UserRoleRelation
ADD OrderID uniqueidentifier
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�q��ID(�q�q��[�J����)', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserRoleRelation', N'COLUMN', N'OrderID'
GO

ALTER TABLE [mgt_UserRoleRelation] 
WITH CHECK ADD CONSTRAINT [FK_mgt_UserRoleRelation_erp_Order]
FOREIGN KEY(OrderID)
REFERENCES [erp_Order] (ID)
GO


--//---mgt_UserRoleRelation add IsEnabled
ALTER TABLE [mgt_UserRoleRelation]
ADD IsEnabled bit
GO

Update [mgt_UserRoleRelation] set IsEnabled = 1; 

ALTER TABLE [mgt_UserRoleRelation]
ALTER COLUMN IsEnabled bit not null
GO

--��ʧ���춶��: ID��̫e


--//---add data from order: ���ӽ�Role,���A������
insert mgt_UserRoleRelation
SELECT NEWID(), CreateUser, RoleID, RoleNumber, 0, null,null, CreateTime, ID, 0 FROM [erp_Order]
where RoleID is not null and OrderStatus !=100 

-- check����� !!!!!! 


-- �R�����
ALTER TABLE erp_Order
DROP CONSTRAINT FK_erp_Order_mgt_Role
GO   

ALTER TABLE erp_Order
DROP COLUMN RoleID, RoleNumber


--//---mgt_UserRoleRelation add IsDelete
ALTER TABLE [mgt_UserRoleRelation]
ADD IsDelete bit
GO

Update [mgt_UserRoleRelation] set IsDelete = 0; 

ALTER TABLE [mgt_UserRoleRelation]
ALTER COLUMN IsDelete bit not null
GO

--=========================================================================
--=========================================================================

--//---Structure add DiscountTypes
ALTER TABLE [cms_Structure]
ADD DiscountTypes varchar(4000)
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�]�t�馩����', N'SCHEMA', N'dbo', N'TABLE', N'cms_Structure', N'COLUMN', N'DiscountTypes'
GO

--��ʧ���춶��

--//---Item add DiscountType
ALTER TABLE cms_Item
ADD DiscountType int
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�w�q�馩����', N'SCHEMA', N'dbo', N'TABLE', N'cms_Item', N'COLUMN', N'DiscountType'
GO

Update [cms_Item] set DiscountType = 0; 

ALTER TABLE [cms_Item]
ALTER COLUMN DiscountType int not null
GO

--=========================================================================
--=========================================================================

USE [WebMakerDB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[erp_OrderDiscount](
	[ID] [uniqueidentifier] NOT NULL,
	[OrderID] [uniqueidentifier] NOT NULL,
	[OrderDetailID] [uniqueidentifier] NULL,
	[DiscountID] [uniqueidentifier] NOT NULL,
	[DiscountPrice] [decimal](9, 2) NOT NULL,
 CONSTRAINT [PK_erp_OrderDiscount] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[erp_OrderDiscount]  WITH CHECK ADD  CONSTRAINT [FK_erp_OrderDiscount_cms_Item] FOREIGN KEY([DiscountID])
REFERENCES [dbo].[cms_Item] ([ID])
GO

ALTER TABLE [dbo].[erp_OrderDiscount] CHECK CONSTRAINT [FK_erp_OrderDiscount_cms_Item]
GO

ALTER TABLE [dbo].[erp_OrderDiscount]  WITH CHECK ADD  CONSTRAINT [FK_erp_OrderDiscount_erp_Order] FOREIGN KEY([OrderID])
REFERENCES [dbo].[erp_Order] ([ID])
GO

ALTER TABLE [dbo].[erp_OrderDiscount] CHECK CONSTRAINT [FK_erp_OrderDiscount_erp_Order]
GO

ALTER TABLE [dbo].[erp_OrderDiscount]  WITH CHECK ADD  CONSTRAINT [FK_erp_OrderDiscount_erp_OrderDetail] FOREIGN KEY([OrderDetailID])
REFERENCES [dbo].[erp_OrderDetail] ([ID])
GO

ALTER TABLE [dbo].[erp_OrderDiscount] CHECK CONSTRAINT [FK_erp_OrderDiscount_erp_OrderDetail]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�A�Χ馩(ItemID)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderDiscount', @level2type=N'COLUMN',@level2name=N'DiscountID'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�馩���B(�t��)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderDiscount', @level2type=N'COLUMN',@level2name=N'DiscountPrice'
GO

--=========================================================================
--=========================================================================

--//---�����b��
ALTER TABLE [erp_Order]
ADD VirtualAccount varchar(20),
	VirtualCreateTime datetime,
	PayDeadline datetime
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�����b��', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'VirtualAccount'
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�����b���إ߮ɶ�', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'VirtualCreateTime'
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�I�ڴ���', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'PayDeadline'
GO

--����Y�g
ALTER TABLE mgt_UserProfile
ADD UnitShort nvarchar(50)
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�A�ȳ��²�g', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'UnitShort'
GO

--���鶤�W

ALTER TABLE erp_OrderDetail
ADD DetailTeamName nvarchar(100)
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'���鶤�W', N'SCHEMA', N'dbo', N'TABLE', N'erp_OrderDetail', N'COLUMN', N'DetailTeamName'
GO

--��ʧ���춶��


--fix �q��

--detail�M�D�ɪ��A�ۦP
update erp_OrderDetail
set orderStatus = (select orderStatus from erp_Order where ID = erp_OrderDetail.OrderID)


----- �ƽ����I�ڤ覡-----
--��ATM���������
update erp_Order 
set PayType = 2
where ClientID = '524dde74-fdef-481a-b9ed-49bab41f7964' and PayType = 1

--�����B -> �I�ڤ覡:�������
update erp_Order 
set PayType = 2
where StructureID='cad7be08-2c1c-4cad-8546-da5bfb1d0f7b' and PayType = 0 and TotalPrice!=0
--�L���B -> �I�ڤ覡:�L
update  erp_Order 
set PayType = 0
where StructureID='cad7be08-2c1c-4cad-8546-da5bfb1d0f7b'  and TotalPrice=0

--�ݥI�ڥ����͵����b��(��I�ڤ覡�O�L) �אּ�B�z��
update erp_Order 
set OrderStatus=30
where StructureID='cad7be08-2c1c-4cad-8546-da5bfb1d0f7b' and OrderStatus=20 and VirtualAccount is null

--=========================================================================
--=========================================================================

--�X�֭q�����p
ALTER TABLE erp_Order
ADD CombineOrderID uniqueidentifier
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�֤J�q��ID', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'CombineOrderID'
GO

ALTER TABLE [erp_Order] 
WITH CHECK ADD CONSTRAINT [FK_erp_Order_erp_Order]
FOREIGN KEY(CombineOrderID)
REFERENCES [erp_Order] (ID)
GO

--=========================================================================
--=========================================================================

--�J�b�q��
USE [WebMakerDB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[erp_GetPayMessage](
	[SeqNo] [bigint] IDENTITY(1,1) NOT NULL,
	[ClientID] [uniqueidentifier] NOT NULL,
	[OriData] [nvarchar](max) NULL,
	[DecryptData] [nvarchar](max) NULL,
	[OrderID] [uniqueidentifier] NULL,
	[PayType] [int] NOT NULL,
	[PayPrice] [decimal](9, 2) NULL,
	[PayTime] [datetime] NULL,
	[CreateTime] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_erp_GetPayMessage] PRIMARY KEY CLUSTERED 
(
	[SeqNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[erp_GetPayMessage]  WITH CHECK ADD  CONSTRAINT [FK_erp_GetPayMessage_erp_Order] FOREIGN KEY([OrderID])
REFERENCES [dbo].[erp_Order] ([ID])
GO

ALTER TABLE [dbo].[erp_GetPayMessage] CHECK CONSTRAINT [FK_erp_GetPayMessage_erp_Order]
GO

ALTER TABLE [dbo].[erp_GetPayMessage]  WITH CHECK ADD  CONSTRAINT [FK_erp_GetPayMessage_mgt_Client] FOREIGN KEY([ClientID])
REFERENCES [dbo].[mgt_Client] ([ID])
GO

ALTER TABLE [dbo].[erp_GetPayMessage] CHECK CONSTRAINT [FK_erp_GetPayMessage_mgt_Client]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��l���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_GetPayMessage', @level2type=N'COLUMN',@level2name=N'OriData'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ѱK����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_GetPayMessage', @level2type=N'COLUMN',@level2name=N'DecryptData'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���p�q��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_GetPayMessage', @level2type=N'COLUMN',@level2name=N'OrderID'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�I�O�覡' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_GetPayMessage', @level2type=N'COLUMN',@level2name=N'PayType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�I�ڪ��B' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_GetPayMessage', @level2type=N'COLUMN',@level2name=N'PayPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�I�ڮɶ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_GetPayMessage', @level2type=N'COLUMN',@level2name=N'PayTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�إߤ��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_GetPayMessage', @level2type=N'COLUMN',@level2name=N'CreateTime'
GO

--=========================================================================
--=========================================================================

--���ϥ�datetime2
ALTER TABLE  mgt_UserLog
ALTER COLUMN CreateTime datetime

ALTER TABLE  erp_GetPayMessage
ALTER COLUMN CreateTime datetime

--=========================================================================
--=========================================================================

--�X�֭q�����p��_
ALTER TABLE erp_OrderDetail
ADD CombineOriOrderID uniqueidentifier
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�q�q��֤JID', N'SCHEMA', N'dbo', N'TABLE', N'erp_OrderDetail', N'COLUMN', N'CombineOriOrderID'
GO

ALTER TABLE erp_OrderDetail 
WITH CHECK ADD CONSTRAINT [FK_erp_OrderDetail_erp_Order1]
FOREIGN KEY(CombineOriOrderID)
REFERENCES [erp_Order] (ID)
GO

--=========================================================================
--�H�WServer��s����
--=========================================================================

--//---���s�W���--//---
ALTER TABLE mgt_UserProfile
ADD CreateUser uniqueidentifier,
	Sort int,
	TempNo int,
	IsKeep bit
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�إߪ�', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'CreateUser'
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�Ƨ�', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'Sort'
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'²���s��', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'TempNo'
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�O�_���`��', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'IsKeep'
GO


--�إ߸��

Update [mgt_UserProfile] set Sort = 0, TempNo=0, IsKeep=0; 

update u
set u.CreateUser = o.CreateUser  
from [mgt_UserProfile] u inner join erp_Order o on u.OrderID = o.ID
where orderID is not null

--update u
--set u.IsKeep = 1
--from [mgt_UserProfile] u inner join erp_Order o on u.OrderID = o.ID
--where OrderStatus = 100

ALTER TABLE [mgt_UserProfile]
ALTER COLUMN Sort int not null
GO

ALTER TABLE [mgt_UserProfile]
ALTER COLUMN TempNo int not null
GO

ALTER TABLE [mgt_UserProfile]
ALTER COLUMN IsKeep bit not null
GO


--//---����ƪ�--//---
USE [WebMakerDB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[erp_OrderUnit](
	[ID] [uniqueidentifier] NOT NULL,
	[OrderID] [uniqueidentifier] NOT NULL,
	[Unit] [nvarchar](50) NULL,
	[UnitShort] [nvarchar](50) NULL,
	[County] [nvarchar](50) NULL,
	[Coach] [nvarchar](100) NULL,
	[Leader] [nvarchar](100) NULL,
	[Manager] [nvarchar](100) NULL,
	[CreateTime] [datetime] NOT NULL,
	[CreateUser] [uniqueidentifier] NOT NULL,
	[UpdateTime] [uniqueidentifier] NOT NULL,
	[Sort] [int] NOT NULL,
	[TempNo] [int] NOT NULL,
	[IsKeep] [bit] NOT NULL,
 CONSTRAINT [PK_erp_OrderUnit] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[erp_OrderUnit]  WITH CHECK ADD  CONSTRAINT [FK_erp_OrderUnit_erp_Order] FOREIGN KEY([OrderID])
REFERENCES [dbo].[erp_Order] ([ID])
GO

ALTER TABLE [dbo].[erp_OrderUnit] CHECK CONSTRAINT [FK_erp_OrderUnit_erp_Order]
GO

ALTER TABLE [dbo].[erp_OrderUnit]  WITH CHECK ADD  CONSTRAINT [FK_erp_OrderUnit_mgt_User] FOREIGN KEY([CreateUser])
REFERENCES [dbo].[mgt_User] ([ID])
GO

ALTER TABLE [dbo].[erp_OrderUnit] CHECK CONSTRAINT [FK_erp_OrderUnit_mgt_User]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderUnit', @level2type=N'COLUMN',@level2name=N'Unit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���²�g' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderUnit', @level2type=N'COLUMN',@level2name=N'UnitShort'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderUnit', @level2type=N'COLUMN',@level2name=N'County'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�нm' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderUnit', @level2type=N'COLUMN',@level2name=N'Coach'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ⶤ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderUnit', @level2type=N'COLUMN',@level2name=N'Leader'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�޲z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderUnit', @level2type=N'COLUMN',@level2name=N'Manager'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�إ߮ɶ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderUnit', @level2type=N'COLUMN',@level2name=N'CreateTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�إߪ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderUnit', @level2type=N'COLUMN',@level2name=N'CreateUser'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ƨ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderUnit', @level2type=N'COLUMN',@level2name=N'Sort'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'²���s��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderUnit', @level2type=N'COLUMN',@level2name=N'TempNo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O�_���`��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'erp_OrderUnit', @level2type=N'COLUMN',@level2name=N'IsKeep'
GO




--//---���������--//---
ALTER TABLE mgt_UserProfile
ADD UnitID uniqueidentifier
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'���', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'UnitID'
GO

--���p
USE [WebMakerDB]
GO

ALTER TABLE [dbo].[mgt_UserProfile]  WITH CHECK ADD  CONSTRAINT [FK_mgt_UserProfile_erp_OrderUnit] FOREIGN KEY([UnitID])
REFERENCES [dbo].[erp_OrderUnit] ([ID])
GO

ALTER TABLE [dbo].[mgt_UserProfile] CHECK CONSTRAINT [FK_mgt_UserProfile_erp_OrderUnit]
GO

--���p
USE [WebMakerDB]
GO

ALTER TABLE [dbo].[mgt_UserProfile]  WITH CHECK ADD  CONSTRAINT [FK_mgt_UserProfile_mgt_User] FOREIGN KEY([CreateUser])
REFERENCES [dbo].[mgt_User] ([ID])
GO

ALTER TABLE [dbo].[mgt_UserProfile] CHECK CONSTRAINT [FK_mgt_UserProfile_mgt_User]
GO

--=========================================================================
--�H�W��K��s����
--=========================================================================

--�J�b�[�T���إߤ覡
ALTER TABLE erp_GetPayMessage
ADD IsByQuery bit
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'��Console���s�d�b', N'SCHEMA', N'dbo', N'TABLE', N'erp_GetPayMessage', N'COLUMN', N'IsByQuery'
GO

Update erp_GetPayMessage set IsByQuery=0; 

ALTER TABLE erp_GetPayMessage
ALTER COLUMN IsByQuery bit not null
GO

--=========================================================================
--=========================================================================

---Unit add UpdateTime


--�J�b�[�T���إߤ覡
ALTER TABLE erp_GetPayMessage
ADD IsEnabled bit
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�q��O�_����', N'SCHEMA', N'dbo', N'TABLE', N'erp_GetPayMessage', N'COLUMN', N'IsEnabled'
GO

Update erp_GetPayMessage set IsEnabled=1; 

ALTER TABLE erp_GetPayMessage
ALTER COLUMN IsEnabled bit not null
GO


--�ƽ��� ���W��Ƨ�
update t
set Sort = ROWID
from (
SELECT ROW_NUMBER() OVER(ORDER BY CreateTime) AS ROWID,*
FROM mgt_UserProfile
where OrderID is not null
) t

--=========================================================================
--=========================================================================

--log���������b��
ALTER TABLE erp_OrderLog
ADD DataContent nvarchar(MAX)
GO

--message ���������b��
ALTER TABLE erp_GetPayMessage
ADD VirtualAccount nvarchar(200)
GO

update m
set m.VirtualAccount = o.VirtualAccount
from erp_GetPayMessage m
inner join erp_Order o on m.OrderID=o.ID

--=========================================================================
--=========================================================================

--�ƽ��� ��JUnit���
insert erp_OrderUnit
select distinct NEWID(), OrderID, Unit,UnitShort, [HouseholdAddress], [Coach], [Leader], [Manager], o.CreateTime, o.CreateTime, o.CreateUser, 0, 0
,CASE WHEN OrderStatus =100 THEN 1 ELSE 0 END 
from mgt_UserProfile u
inner join erp_Order o on u.OrderID=o.ID
where o.StructureID='d83dc304-8f3a-48bc-b85f-23695ba0e9d6'--���ɤ峹


update u
set u.UnitID = t.ID
from [mgt_UserProfile] u inner join erp_Order o on u.OrderID = o.ID
left join erp_OrderUnit t on u.Unit=t.Unit and u.UnitShort=t.UnitShort and u.HouseholdAddress=t.County and u.OrderID = t.OrderID
where  o.StructureID='d83dc304-8f3a-48bc-b85f-23695ba0e9d6'

--=========================================================================
--=========================================================================

--//---²�TTemplate--//---
ALTER TABLE cms_EmailTemplate
ADD SMSContent nvarchar(200),
	SMSIsEnabled bit
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'²�T���e', N'SCHEMA', N'dbo', N'TABLE', N'cms_EmailTemplate', N'COLUMN', N'SMSContent'
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'²�T�O�_�ҥ�', N'SCHEMA', N'dbo', N'TABLE', N'cms_EmailTemplate', N'COLUMN', N'SMSIsEnabled'
GO

Update cms_EmailTemplate set SMSIsEnabled = 0; 

ALTER TABLE cms_EmailTemplate
ALTER COLUMN SMSIsEnabled bit not null
GO


--//---²�T����--//---
USE [WebMakerDB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[cms_SmsLog](
	[SeqNo] [bigint] IDENTITY(1,1) NOT NULL,
	[ClientID] [uniqueidentifier] NOT NULL,
	[PhoneNumber] [nvarchar](20) NOT NULL,
	[SMSContent] [nvarchar](200) NOT NULL,
	[ToUser] [uniqueidentifier] NULL,
	[IsSend] [bit] NOT NULL,
	[SendTime] [datetime] NULL,
	[CreateTime] [datetime] NOT NULL,
	[Msgid] [nvarchar](20) NULL,
	[SmsResultType] [int] NOT NULL,
	[UpdateResultTime] [datetime] NULL,
 CONSTRAINT [PK_cms_SmsLog] PRIMARY KEY CLUSTERED 
(
	[SeqNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[cms_SmsLog]  WITH CHECK ADD  CONSTRAINT [FK_cms_SmsLog_mgt_Client] FOREIGN KEY([ClientID])
REFERENCES [dbo].[mgt_Client] ([ID])
GO

ALTER TABLE [dbo].[cms_SmsLog] CHECK CONSTRAINT [FK_cms_SmsLog_mgt_Client]
GO

ALTER TABLE [dbo].[cms_SmsLog]  WITH CHECK ADD  CONSTRAINT [FK_cms_SmsLog_mgt_User] FOREIGN KEY([ToUser])
REFERENCES [dbo].[mgt_User] ([ID])
GO

ALTER TABLE [dbo].[cms_SmsLog] CHECK CONSTRAINT [FK_cms_SmsLog_mgt_User]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�۰ʽs���A�O������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cms_SmsLog', @level2type=N'COLUMN',@level2name=N'SeqNo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�q�ܸ��X' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cms_SmsLog', @level2type=N'COLUMN',@level2name=N'PhoneNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'²�T���e' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cms_SmsLog', @level2type=N'COLUMN',@level2name=N'SMSContent'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cms_SmsLog', @level2type=N'COLUMN',@level2name=N'ToUser'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cms_SmsLog', @level2type=N'COLUMN',@level2name=N'SendTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cms_SmsLog', @level2type=N'COLUMN',@level2name=N'CreateTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���A��s�ɶ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cms_SmsLog', @level2type=N'COLUMN',@level2name=N'UpdateResultTime'
GO

--=========================================================================
--=========================================================================

--//---�s�W��������--//---
ALTER TABLE mgt_UserProfile
ADD IsPassportNumber bit,
	IsMyChild bit
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�O�@�өΨ�����', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'IsPassportNumber'
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�O�a���إߪ��p��', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'IsMyChild'
GO

Update mgt_UserProfile set IsPassportNumber = 0, IsMyChild = 0; 

ALTER TABLE mgt_UserProfile
ALTER COLUMN IsPassportNumber bit not null
GO
ALTER TABLE mgt_UserProfile
ALTER COLUMN IsMyChild bit not null
GO


--//---�s�W��������2--//---
ALTER TABLE mgt_UserProfile
ADD IsDelete bit,
	ClientID uniqueidentifier,
	FromSourceID uniqueidentifier
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�ƻs���ӷ�', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'FromSourceID'
GO

Update mgt_UserProfile set IsDelete = 0


Update p
set p.ClientID=u.ClientID
from mgt_UserProfile p inner join mgt_User u on p.CreateUser=u.ID

Update p
set p.ClientID=u.ClientID
from mgt_UserProfile p inner join mgt_User u on p.ID=u.UserProfileID

Update p
set p.ClientID=u.ClientID
from mgt_UserProfile p inner join cms_Item u on p.ItemID=u.ID

Update p
set p.ClientID=u.ClientID
from mgt_UserProfile p inner join erp_Order u on p.OrderID=u.ID
-----------------
--check
select * from mgt_UserProfile where clientID is null

--delete from mgt_UserProfile where clientID is null

ALTER TABLE mgt_UserProfile
ALTER COLUMN IsDelete bit not null
GO

ALTER TABLE mgt_UserProfile
ALTER COLUMN ClientID uniqueidentifier not null
GO

--forieng key
ALTER TABLE mgt_UserProfile 
WITH CHECK ADD CONSTRAINT [FK_mgt_UserProfile_mgt_Client]
FOREIGN KEY(ClientID)
REFERENCES mgt_Client (ID)
GO

ALTER TABLE mgt_UserProfile 
WITH CHECK ADD CONSTRAINT [FK_mgt_UserProfile_mgt_UserProfile]
FOREIGN KEY(FromSourceID)
REFERENCES mgt_UserProfile (ID)
GO


--//--��Ʊ��v--//---

USE [WebMakerDB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[mgt_UserAssign](
	[ID] [uniqueidentifier] NOT NULL,
	[UserProfileID] [uniqueidentifier] NOT NULL,
	[FromUser] [uniqueidentifier] NOT NULL,
	[ToUser] [uniqueidentifier] NOT NULL,
	[Note] [nvarchar](200) NULL,
	[Sort] [int] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[DeleteTime] [datetime] NULL,
	[CreateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_mgt_UserAssign] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[mgt_UserAssign]  WITH CHECK ADD  CONSTRAINT [FK_mgt_UserAssign_mgt_User] FOREIGN KEY([FromUser])
REFERENCES [dbo].[mgt_User] ([ID])
GO

ALTER TABLE [dbo].[mgt_UserAssign] CHECK CONSTRAINT [FK_mgt_UserAssign_mgt_User]
GO

ALTER TABLE [dbo].[mgt_UserAssign]  WITH CHECK ADD  CONSTRAINT [FK_mgt_UserAssign_mgt_User1] FOREIGN KEY([ToUser])
REFERENCES [dbo].[mgt_User] ([ID])
GO

ALTER TABLE [dbo].[mgt_UserAssign] CHECK CONSTRAINT [FK_mgt_UserAssign_mgt_User1]
GO

ALTER TABLE [dbo].[mgt_UserAssign]  WITH CHECK ADD  CONSTRAINT [FK_mgt_UserAssign_mgt_UserProfile] FOREIGN KEY([UserProfileID])
REFERENCES [dbo].[mgt_UserProfile] ([ID])
GO

ALTER TABLE [dbo].[mgt_UserAssign] CHECK CONSTRAINT [FK_mgt_UserAssign_mgt_UserProfile]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��Ʊ��v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'mgt_UserAssign', @level2type=N'COLUMN',@level2name=N'ID'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'mgt_UserAssign', @level2type=N'COLUMN',@level2name=N'UserProfileID'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�֦��H(�a��)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'mgt_UserAssign', @level2type=N'COLUMN',@level2name=N'FromUser'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Q���v�H(�нm)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'mgt_UserAssign', @level2type=N'COLUMN',@level2name=N'ToUser'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'mgt_UserAssign', @level2type=N'COLUMN',@level2name=N'IsDelete'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'mgt_UserAssign', @level2type=N'COLUMN',@level2name=N'CreateTime'
GO

--=========================================================================
--=========================================================================

--//---Structure add PriceTypes
ALTER TABLE [cms_Structure]
ADD PriceTypes varchar(4000)
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�]�t�p���覡', N'SCHEMA', N'dbo', N'TABLE', N'cms_Structure', N'COLUMN', N'PriceTypes'
GO

--��ʧ���춶��

--//---Item add DiscountType
ALTER TABLE cms_Item
ADD PriceType int
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�p���覡', N'SCHEMA', N'dbo', N'TABLE', N'cms_Item', N'COLUMN', N'PriceType'
GO

Update [cms_Item] set PriceType = 0; 

ALTER TABLE [cms_Item]
ALTER COLUMN PriceType int not null
GO


--//---Item �W�[����
ALTER TABLE cms_Item
ADD SaleLimit int,
	DateLimit datetime
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�P�⭭��', N'SCHEMA', N'dbo', N'TABLE', N'cms_Item', N'COLUMN', N'SaleLimit'
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�������', N'SCHEMA', N'dbo', N'TABLE', N'cms_Item', N'COLUMN', N'DateLimit'
GO
Update [cms_Item] set SaleLimit = 0; 

ALTER TABLE [cms_Item]
ALTER COLUMN SaleLimit int not null
GO

--=========================================================================
--=========================================================================

--//---�q����Ӱߤ@���
ALTER TABLE erp_OrderUnit
ADD OrderDetailID [uniqueidentifier]
GO

ALTER TABLE erp_OrderUnit 
WITH CHECK ADD CONSTRAINT [FK_erp_OrderUnit_erp_OrderDetail]
FOREIGN KEY(OrderDetailID)
REFERENCES erp_OrderDetail (ID)
GO

--=========================================================================
--=========================================================================

--//---���O�_�w�H�q���H--//---
ALTER TABLE mgt_UserProfile
ADD IsSend bit
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�O�_�w�H�q���H', N'SCHEMA', N'dbo', N'TABLE', N'mgt_UserProfile', N'COLUMN', N'IsSend'
GO

Update mgt_UserProfile set IsSend = 0

ALTER TABLE mgt_UserProfile
ALTER COLUMN IsSend bit not null
GO

--//---�q��s�����--//--
ALTER TABLE erp_Order
ADD EditDeadline datetime
GO

EXECUTE sp_addextendedproperty
N'MS_Description', N'�s�����', N'SCHEMA', N'dbo', N'TABLE', N'erp_Order', N'COLUMN', N'EditDeadline'
GO

--=========================================================================
--�H�WServer��s����
--=========================================================================