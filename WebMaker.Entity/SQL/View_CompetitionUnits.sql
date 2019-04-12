--
-- select * from View_CompetitionUnits
-- where ClientID='524dde74-fdef-481a-b9ed-49bab41f7964' and ItemID='16fae3b3-c1b8-46d8-bb81-14237aef232e'
-- order by Unit
--

SELECT  u.Unit, u.UnitShort, u.HouseholdAddress AS County, o.Leader, o.Coach, o.Manager, 
		o.ClientID, o.ItemID, o.CreateUser, o.ID AS OrderID,
            (SELECT  COUNT(*) AS Expr1
             FROM    dbo.mgt_UserProfile AS du
             WHERE   (OrderID = o.ID) AND (u.Unit = Unit) AND (u.UnitShort = UnitShort)) AS MemberCount
FROM    dbo.mgt_UserProfile AS u INNER JOIN
        dbo.erp_Order AS o ON o.ID = u.OrderID
WHERE   (o.OrderStatus = 100)