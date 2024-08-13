select *
from Exception
where modifiedon > '2024/08/13'
and severity = 1

select *
from transactions
where payerid = 5874
and operation = 26
order by datefrom



-- These should be cancelled!

drop table #Dangers
drop table #Revertable

select distinct c.CustomerId 
into #Dangers
from SubscriptionIssue as a inner join Subscription as b on a.SubscriptionId = b.SubscriptionId
inner join Customer as c on b.PayerId = c.CustomerId
where a.UnitsLeft > 0
and c.BalanceInvoiceId is null
order by Customerid


select a.CustomerId, 'BalanceInvoiceId' = convert( integer, isnull(b.Reference2, 0)) 
into #Revertable
from  #Dangers as a left outer join Transactions as b on a.CustomerId = b.PayerId
where operation = 26

update Customer
set BalanceInvoiceId = 0
from Customer as a inner join #Dangers as b on a.CustomerId = b.CustomerId





