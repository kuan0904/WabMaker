--
-- select * from View_CompetitionMember 
-- where ClientID='524dde74-fdef-481a-b9ed-49bab41f7964' and ItemID='16fae3b3-c1b8-46d8-bb81-14237aef232e'
-- order by Unit
--

SELECT  u.TempNo, u.NickName AS Name, u.Gender, u.IdentityCard, u.Birthday, u.Unit, u.HouseholdAddress,
           (SELECT  COUNT(*) AS Expr1
             FROM   dbo.erp_OrderDetailTeam AS dt
            WHERE   (UserProfileID = u.ID)) AS ItemCount, o.ClientID, m.ID AS ItemID, u.ID AS UserProfileID, o.ID AS OrderID
FROM  dbo.mgt_UserProfile AS u INNER JOIN
      dbo.erp_Order AS o ON u.OrderID = o.ID INNER JOIN
      dbo.cms_Item AS m ON m.ID = o.ItemID
WHERE  (o.OrderStatus = 100)