USE [WebMakerDB]
GO
/****** Object:  StoredProcedure [dbo].[Proc_GetCompetitionUnits]    Script Date: 2019/3/14 下午 06:41:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Annie
-- Create date: 2019.3.3
-- Description:	聯絡人&單位與選手 (選手會重複)
-- Example: exec Proc_GetCompetitionUnits 'e8b12b40-57df-4ca9-a2e9-614175e24395','524dde74-fdef-481a-b9ed-49bab41f7964'
--			exec Proc_GetCompetitionUnits '16fae3b3-c1b8-46d8-bb81-14237aef232e','524dde74-fdef-481a-b9ed-49bab41f7964'
-- =============================================
ALTER PROCEDURE [dbo].[Proc_GetCompetitionUnits]
	@ArticleID uniqueidentifier,
	@ClientID uniqueidentifier 
AS
BEGIN
	SET NOCOUNT ON;

select c.Name as CreateUser, c.Phone, c.Email,
n.TempNo as UnitNo, n.County, n.Unit, n.UnitShort, n.Coach, n.Leader, n.Manager,
OrderNumber,
(select count(*) from mgt_UserProfile du where du.OrderID = o.ID and u.Unit = du.Unit and u.UnitShort=du.UnitShort ) as MemberCount,
u.TempNo as MemberNo, NickName as Name

from erp_OrderUnit n
left join mgt_UserProfile u on n.ID = u.UnitID
inner join erp_Order o on o.ID = u.OrderID
inner join mgt_User c on c.ID = o.CreateUser
where OrderStatus = 100 and　o.ItemID=@ArticleID and o.ClientID=@ClientID
Order by n.TempNo, n.Unit

END
