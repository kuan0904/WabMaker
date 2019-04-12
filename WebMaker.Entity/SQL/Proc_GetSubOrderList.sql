USE [WebMakerDB]
GO
/****** Object:  StoredProcedure [dbo].[Proc_GetSubOrderList]    Script Date: 2019/4/9 下午 01:55:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Annie
-- Create date: 2019.4.9
-- Description:	文章項目
-- =============================================
ALTER PROCEDURE [dbo].[Proc_GetSubOrderList]
	@ArticleID as uniqueidentifier
AS
BEGIN
	 SET NOCOUNT ON;
	 
	 SELECT m.*
	 
	 FROM cms_Item pm	
	 inner join cms_Structure s on pm.StructureID = s.ID
	 left join cms_StructureRelation r on s.ID = r.ParentID
	 inner join cms_Structure s2 on s2.ID = r.ChildID and s2.ItemTypes like '%055%'--訂單項目

	 left join cms_ItemRelation mr on mr.ParentID=@ArticleID
	 inner join View_ItemProduct m on mr.ChildID=m.ID
	 
	 where pm.ID=@ArticleID
	 order by m.Sort desc
END
