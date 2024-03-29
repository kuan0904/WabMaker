USE [WebMakerDB]
GO
/****** Object:  StoredProcedure [dbo].[Proc_GetCompetitionMembers]    Script Date: 2019/3/14 下午 06:41:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Annie
-- Create date: 2019.2.24
-- Description:	選手與項目
-- Example: exec Proc_GetCompetitionMembers 'e8b12b40-57df-4ca9-a2e9-614175e24395','524dde74-fdef-481a-b9ed-49bab41f7964'
--			exec Proc_GetCompetitionMembers '16fae3b3-c1b8-46d8-bb81-14237aef232e','524dde74-fdef-481a-b9ed-49bab41f7964'
-- =============================================
ALTER PROCEDURE [dbo].[Proc_GetCompetitionMembers]
	@ArticleID uniqueidentifier,
	@ClientID uniqueidentifier 
AS
BEGIN

select u.TempNo as  MemberNo, NickName as Name, Gender, Birthday, IdentityCard, n.TempNo as UnitNo, n.Unit, l.[Subject], d.[Option],
(select count(*) from erp_OrderDetailTeam dt where dt.UserProfileID = u.ID) as ItemCount,
o.ID as OrderID, u.ID as UserProfileID

from mgt_UserProfile u --選手
inner join erp_Order o on u.OrderID = o.ID --訂單
left join erp_OrderUnit n on u.UnitID = n.ID
inner join cms_Item m on m.ID = o.ItemID -- 文章
left join erp_OrderDetailTeam t on t.UserProfileID = u.ID --團隊
inner join erp_OrderDetail d on t.OrderDetailID = d.ID --訂單明細
inner join cms_Item md on d.ItemID = md.ID--比賽項目
inner join cms_ItemLanguage l on md.ID = l.ItemID --
where o.OrderStatus = 100 and o.ItemID=@ArticleID and m.ClientID=@ClientID
order by u.TempNo, u.Unit


END
