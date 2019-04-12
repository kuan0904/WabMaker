USE [WebMakerDB]
GO
/****** Object:  StoredProcedure [dbo].[Proc_GetCompetitionCount]    Script Date: 2019/4/11 下午 10:40:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Annie
-- Create date: 2019.4.6
-- Create date: 2019.4.11
-- Description:	比賽銷售和狀態統計
-- exec Proc_GetCompetitionCount 'a4dde220-30e5-490b-bb5b-3c39c3abbe4f', 'dd394e36-98fc-44b5-8bd6-705b76d72d19'
-- =============================================
ALTER PROCEDURE [dbo].[Proc_GetCompetitionCount]
	@ClientID uniqueidentifier,
	@ParentItemID uniqueidentifier
AS
BEGIN


select t1.ParentSubject, t1.ArticleSubject, t1.OptionSubject, t1.StockCount, t1.SaleCount, ISNULL(t3.MemberCount, 0 ) MemberCount, t2.*, t1.[Date], t1.Sort from

--//-------1.銷售量-------//----
(select 
lan2.[Subject] as ParentSubject, lanl.[Subject] as ArticleSubject, m0.[Subject] as OptionSubject,
m0.[StockCount], m0.[SaleCount], m0.ID as ItemID, m1.[Date], m0.Sort
   
from View_ItemProduct m0 --組別
--cms_Item m0 --組別
--inner join cms_ItemLanguage lan0 on m0.ID = lan0.ItemID
--inner join cms_Structure s on m0.StructureID = s.ID

left join cms_ItemRelation r1 on m0.ID = r1.ChildID and r1.IsCrumb=1 --文章
left join cms_Item m1 on m1.ID = r1.ParentID  
inner join cms_ItemLanguage lanl on m1.ID=lanl.ItemID

left join cms_ItemRelation r2 on m1.ID = r2.ChildID and r2.IsCrumb=1  --文章群組
left join cms_Item m2 on m2.ID = r2.ParentID  
inner join cms_ItemLanguage lan2 on m2.ID = lan2.ItemID
 
where m2.ClientID = @ClientID and m2.ID=@ParentItemID
and m1.IsDelete=0 and m2.IsDelete=0

) t1

--//-------2.狀態統計-------//----
left join
(
select * from (
select d.ItemID, d.OrderID, CONCAT('Status', d.OrderStatus) as OrderStatus from erp_OrderDetail d
inner join erp_Order o on d.OrderID = o.ID
where o.ClientID = @ClientID
) i 
PIVOT (
	Count(OrderID) 
	FOR  OrderStatus in ([Status50],[Status54],[Status55],[Status80],[Status99],[Status100],[Status110],[Status300])
) p
) t2
on t1.ItemID = t2.ItemID

--//-------3.選手數統計-------//----
left join
(
select d.ItemID, count(*) as MemberCount
from erp_OrderDetailTeam dt 
inner join erp_OrderDetail d on dt.OrderDetailID = d.ID
inner join erp_Order o on o.ID = d.OrderID
where o.ClientID = @ClientID
and d.OrderStatus <= 100
group by d.ItemID
) t3
on t1.ItemID = t3.ItemID


order by [Date] desc, Sort desc

END
