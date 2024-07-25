select *
from Exception
where modifiedon > '2024/07/25'

exec [dbo].[MIMS.CustomerDoc.Due3] 81

update Customer
set balanceinvoiceid = 62683
where customerid = 81

select *
from Transactions
where payerid = 81

select *
from Customer
where customerid = 81

drop table #Dormant1
drop table #Dormant2


Select  distinct 'PayerId' = CustomerId
into #Dormant1
From   Customer
where abs(due)  > 1
order by PayerId


select distinct a.PayerId
into #Dormant2
from #Dormant1 as a inner join Transactions as b on a.PayerId = b.PayerId and b.operation not in (11,33)
   and b.DateFrom > DATEFROMPARTS(Year(Dateadd(yyyy, -1, GetDate())), 7,1)

select a.PayerId, b.Due
from #Dormant1 as a inner join Customer as b on a.PayerId = b.CustomerId
where PayerId not in (select payerid from #Dormant2)
order by a.PayerId



select *
from Transactions
where payerid = 118
order by modifiedon






select b.SubscriptionId, c.IssueId, a.DebitValue, a.CreditValue, b.InvoiceId, c.EndDate, a.DateFrom
from Transactions as a inner join Subscription as b on a.SubscriptionId = b.SubscriptionId
inner join Issue as c on a.Issueid = c.IssueId
where operation = 2
and b.Invoiceid = 72106
and c.EndDate < a.DateFrom
and a.PayerId = 89112


