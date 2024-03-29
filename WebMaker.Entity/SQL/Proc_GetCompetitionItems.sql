USE [WebMakerDB]
GO
/****** Object:  StoredProcedure [dbo].[Proc_GetCompetitionItems]    Script Date: 2019/3/14 下午 06:40:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Annie
-- Create date: 2019.2.24
-- Description:	比賽項目人數統計
-- Example: exec Proc_GetCompetitionItems 'e8b12b40-57df-4ca9-a2e9-614175e24395','524dde74-fdef-481a-b9ed-49bab41f7964'
-- =============================================
ALTER PROCEDURE [dbo].[Proc_GetCompetitionItems]
	@ArticleID uniqueidentifier, --訂單
	@ClientID uniqueidentifier 
AS
BEGIN

--所有組別、項目
select l.[Subject], item as mOption

from cms_Item m
inner join cms_ItemRelation r on m.ID = r.ChildID
inner join cms_ItemLanguage l on m.ID = l.ItemID
cross apply 
(SELECT *, ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS num FROM [dbo].[Fn_Split](m.Options, ',')) as t
--[dbo].[Fn_Split](m.Options, ',')
where r.ParentID = @ArticleID and m.IsDelete = 0 and m.ClientID=@ClientID --and StructureID =@StructureID 
order by m.Sort, t.num desc


END
