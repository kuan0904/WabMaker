USE [WebMakerDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Annie
-- Create date: 2018.9.25
-- Description:	get distinct tag
-- Example: exec [Proc_AllKeywords] 'Keywords'
-- =============================================
ALTER PROCEDURE [dbo].[Proc_GetAllTags]
	@ColumnName  varchar(50) ,-- Keywords or Author
	@ClientID uniqueidentifier
AS

-- 宣告變數
DECLARE @value Nvarchar(2000)
DECLARE @T AS TABLE (Name Nvarchar(2000));

-- FORWARD_ONLY：指定資料指標只能從第一個資料列捲到最後一個資料列。
DECLARE @sql nvarchar(max)  
SET @sql = 
'DECLARE OrdersCursor CURSOR FAST_FORWARD FOR '+
'SELECT ['+ @ColumnName +'] FROM cms_ItemLanguage ' +
'Inner Join cms_Item on cms_ItemLanguage.ItemID=cms_Item.ID ' +
'Where ['+ @ColumnName +'] is not null and '''+ CAST(@ClientID AS NVARCHAR(36)) +'''=ClientID';
--PRINT @sql
EXEC(@sql)
 
-- 開啟指標
OPEN OrdersCursor;
FETCH NEXT FROM OrdersCursor INTO @value;
WHILE @@FETCH_STATUS = 0
BEGIN
  -- do something --
  INSERT INTO @T(Name)
  SELECT * FROM [dbo].[Fn_Split](@value, ',')
  FETCH NEXT FROM OrdersCursor INTO @value;
END
 
-- 查詢資料表變數：@T
SELECT Name, count(*) as [Count] FROM @T Group By Name;
CLOSE OrdersCursor;
DEALLOCATE OrdersCursor;
