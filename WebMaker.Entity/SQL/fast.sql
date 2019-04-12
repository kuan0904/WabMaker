--快速刪除某個User
DECLARE @userid uniqueidentifier = 'user guid'

delete from cms_EmailSendUser where ToUser = @userid;
delete from cms_Email
where ID in (select EmailID from cms_EmailSendUser where ToUser = @userid)

delete from erp_OrderLog 
where OrderID in (select ID from erp_Order where CreateUser = @userid)

delete from erp_OrderDetailTeam 
where OrderDetailID in (select ID from erp_OrderDetail where OrderID in(
select ID from erp_Order where CreateUser = @userid))

delete from mgt_UserProfile 
where OrderID in (select ID from erp_Order where CreateUser = @userid)

delete from erp_OrderDetail 
where OrderID in (select ID from erp_Order where CreateUser = @userid)

delete from erp_Order where CreateUser = @userid;
delete from mgt_UserValidCode where UserID = @userid;
delete from mgt_UserExternalLogin where UserID = @userid;
delete from mgt_UserRoleRelation where UserID = @userid;
delete from mgt_UserLog where UserID = @userid;

delete from mgt_User where ID = @userid;



--快速刪除某個Item
DECLARE @itemid uniqueidentifier = 'item id'

delete from cms_ItemFile where ItemID in 
(select ChildID from cms_ItemRelation where ParentID=@itemid)

delete from cms_ItemLanguage where ItemID in 
(select ChildID from cms_ItemRelation where ParentID=@itemid)

delete from cms_Item where ID in 
(select ChildID from cms_ItemRelation where ParentID=@itemid)

delete from cms_ItemRelation where ParentID=@itemid;

delete from cms_ItemLanguage where ItemID = @itemid;
delete from cms_Item where ID = @itemid;


--快速刪除某筆Order
DECLARE @orderID uniqueidentifier = 'order id'

delete from erp_OrderDetailTeam
where OrderDetailID in (select ID from erp_OrderDetail where OrderID = @orderID)

delete from mgt_UserProfile
where OrderID  = @orderID;

delete erp_OrderDetail
where OrderID = @orderID;

delete erp_OrderLog
where OrderID = @orderID;

delete mgt_UserRoleRelation
where OrderID = @orderID;

delete [erp_Order]
where ID = @orderID;


--快速刪除所有Order
DECLARE @struID uniqueidentifier = 'StructureID'

delete erp_OrderUnit
where OrderID in (select id from erp_Order where StructureID = @struID);

delete erp_OrderDetailTeam 
where OrderDetailID in (select id from erp_OrderDetail 
where OrderID in (select id from erp_Order where StructureID = @struID));

delete mgt_UserProfile
where OrderID in (select id from erp_Order where StructureID = @struID);

delete erp_OrderDetail
where OrderID in (select id from erp_Order where StructureID = @struID);

delete erp_OrderLog
where OrderID in (select id from erp_Order where StructureID = @struID);


delete mgt_UserRoleRelation
where OrderID in (select id from erp_Order where StructureID = @struID);

delete [erp_Order]
where StructureID = @struID;

--新增EmailTemplate
DECLARE @clientid uniqueidentifier = 'client'

INSERT INTO [cms_EmailTemplate] 
([ID],[ClientID],[Subject],[Content],[SystemMailType],[Sort],[IsEnabled],[IsDelete],[UpdateTime],[UpdateUser])
SELECT 
newid(),@clientid,[Subject],[Content],[SystemMailType],[Sort],[IsEnabled],[IsDelete],getdate(),[UpdateUser]
FROM [WebMakerDB].[dbo].[cms_EmailTemplate] 
WHERE [ClientID] = '00000000-1111-2222-3333-123456789999'


--新增Role  /////systemnumber=clientCode_006_日期000001/////
DECLARE @clientid uniqueidentifier = 'client'

INSERT INTO [mgt_Role] 
([ID],[ClientID],[SystemNumber],[Name],[MemberLevel],[AccountType],[UserContentTypes],[UserRequiredTypes],[Sort],[IsEnabled],[IsDelete],[CreateTime],[UpdateTime])
SELECT 
newid(),@clientid,
(select ClientCode + '_006_' + FORMAT(getdate() , 'yyMMdd') + '00000' from mgt_Client where id=@clientid) + CAST(ROW_NUMBER() OVER(ORDER BY Sort)AS varchar)
,[Name],[MemberLevel],[AccountType],[UserContentTypes],[UserRequiredTypes],[Sort],[IsEnabled],[IsDelete],getdate(),getdate()
FROM [WebMakerDB].[dbo].[mgt_Role] 
WHERE [ClientID] = '00000000-1111-2222-3333-123456789999' and [IsDelete]=0


--複製本機structure到server
DECLARE @struID uniqueidentifier = 'StructureID'

select * from cms_Structure where id=@struID
select * from cms_StructureRelation where ChildID=@struID