SELECT m.ID, [Subject], m.StructureID,
	 
m.SalePrice, m.StockCount, m.SaleCount as OriSaleCount,
(select count(*) from erp_OrderDetail where ItemID=m.ID and OrderStatus in(11,20,30,31,50,54,55,70,80,100)) as SaleCount
,m.SaleLimit, PeopleMin, PeopleMax, DateLimit, m.Sort,
	 
 CAST(CASE  WHEN s2.ContentTypes like '%310%'THEN 1   ELSE 0 END as bit)AS [IsCheckSaleCount] --販售數量

FROM cms_Item m 
left join cms_ItemLanguage l on m.ID = l.ItemID
inner join cms_Structure s2 on s2.ID = m.StructureID and s2.ItemTypes like '%055%'--訂單項目

where m.IsDelete=0