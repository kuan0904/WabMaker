USE [WebMakerDB]
GO
/****** Object:  StoredProcedure [dbo].[Proc_NewOrderNumber]    Script Date: 2019/4/11 ¤U¤È 09:46:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Annie
-- Create date: 2019.4.8
-- Update date: 2019.4.11
-- Description:	CreateOrderNumber
-- =============================================
ALTER PROCEDURE [dbo].[Proc_NewOrderNumber]
AS
BEGIN	
	DECLARE @OrderNumber char(12); --=Date + ff + xxxx
	DECLARE @Num int;
	DECLARE @Now DATETIME
	SET @Now = GETDATE()
	
	--today count
	select @Num = count(*) from erp_Order
	where convert(varchar(10),CreateTime,111)=CAST(@Now AS DATE)

	-- add 1
	set @OrderNumber =  FORMAT(@Now, 'yyMMddff')  + Replace(Str(@Num+1, 4), ' ' , '0')
			
	select @OrderNumber
END
