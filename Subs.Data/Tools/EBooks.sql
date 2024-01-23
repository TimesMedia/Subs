select *
from Exception
where modifiedon > '2024/01/18 08:25:00'




select distinct b.IssueId, d.IssueDescription
from subscription as a
inner join SubscriptionIssue as b on a.SubscriptionId = b.SubscriptionId
inner join Issue as d on b.IssueId = d.IssueId
where b.UnitsLeft = 0
and d.EBookURL is not null

Select *
from Product2






select *
from Issue
where Issueid = 899

select *
from Product2 
where medium in (2,3)

select *
from Issue
where Issueid = 1054

select a.*, b.*
from Subscription as a inner join SubscriptionIssue as b on a.Subscriptionid = b.SubscriptionId
where b.subscriptionid = 208792


