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

select distinct PayerId, b.Due
from Transactions as a inner join Customer as b on a.PayerId = b.CustomerId
where operation = 2
and a.datefrom > '2024/08/01'
and b.BalanceInvoiceId is null

-- Customer with potentially wrong deliveries
Update Customer
set BalanceInvoiceId = 0
where CustomerId in (
5499,
116682,
118690,
5874,
8980,
121086)


    select *
	from Customer
	where (due > 1000
	or Liable > 1000)
	and BalanceInvoiceId is not null
	order by CustomerId




exec [dbo].[MIMS.CustomerData.GetDiscrepancy] 1000

select CustomerId, isnull(FirstName, 'NoName'), 
	                   isnull(Surname, 'NoSurname'), Due, Liable
	from Customer
	where due < - @AbsoluteValue
	or Liable < - @AbsoluteValue
	order by CustomerId

