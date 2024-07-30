select *
from Exception
where modifiedon > '2024/07/30'

drop table #Hein

 -- Change the dates on the invoice
 -- No, that damages the whle payment calculation.   Since these are before 2022, just write them off
select a.PayerId, a.InvoiceId, 'SubscriptionId' = a.SubscriptionId, 'SubscriptionDate' = c.DateFrom, 'InvoiceDate' = b.DateFrom
into #Hein1
from Subscription as a inner join Transactions as b on a.InvoiceId = b.InvoiceId and b.operation = 19
inner join Transactions as c on a.SubscriptionId = c.SubscriptionId and c.Operation = 16
where DATEDIFF(MM, c.datefrom, b.DateFrom) > 1
and c.DateFrom > '2017/06/01'


select distinct PayerId
into #Hein2
from #Hein1


drop table #Dormant1
drop table #Dormant2
drop table #Dormant3


select distinct PayerId
into #WithInvoices
from Transactions 
where DateFrom >= DATEFROMPARTS(Year(Dateadd(yyyy, -1, GetDate())), 7,1)
and operation in (19)

select a.PayerId, b.Due, b.BalanceInvoiceId
into #Candidates
from #WithInvoices as a inner join Customer as b on a.PayerId = b.CustomerId
where b.BalanceInvoiceId is null
order by a.PayerId

drop table #Balance

select b.CustomerId, 'BalanceInvoiceId' = min(a.InvoiceId)
into #Balance 
	from Transactions as a inner join Customer as b on a.PayerId = b.CustomerId
	inner join #Candidates as c on c.PayerId = b.CustomerId
	where operation = 19
	and a.DateFrom > (select CheckpointDateInvoice from Customer where CustomerId = b.CustomerId)
	group by b.CustomerId

update Customer
set BalanceInvoiceId = b.BalanceInvoiceId
From Customer as a inner join #Balance as b on a.CustomerId = b.CustomerId

--------------------------






select distinct PayerId
into #Dormant2
from Transactions 
where DateFrom >= DATEFROMPARTS(Year(Dateadd(yyyy, -1, GetDate())), 7,1)
and operation not in (11,20,33 )


select *
from #Dormant2



select a.PayerId, b.Due, b.BalanceInvoiceId
--into #Dormant3
from #WithInvoices as a inner join Customer as b on a.PayerId = b.CustomerId
where b.BalanceInvoiceId is null
order by a.PayerId

select *
from Transactions
where payerid = 12
and modifiedon > '2023/07/01'





select *
from Customer as a left outer join Transactions as b on a.CustomerId = b.PayerId and b.Operation = 19
where a.BalanceInvoiceId = 0
and b.PayerId is null   -- New customer without invoice

select *
	from Customer
	where customerid = 121565



drop table #Balance

select *
from #WithInvoices



select b.CustomerId, 'ValidBalanceInvoiceId' = min(a.InvoiceId)
--into #Balance 
	from Transactions as a inner join Customer as b on a.PayerId = b.CustomerId
	inner join #WithInvoices as c on c.PayerId = b.CustomerId
	where operation = 19
	and a.DateFrom > (select CheckpointDateInvoice from Customer where CustomerId = b.CustomerId)
	group by b.CustomerId, BalanceInvoiceId





	select *
	from #Balance


	select *
	from Customer
	where BalanceInvoiceId = 0


	select *
from mims3.dbo.Transactions
where operation = 1








select top 1 @InvoiceId = InvoiceId
	from Transactions as a inner join #Dormant3 as b on a.PayerId = b.PayerId
	where operation = 19
	and DateFrom > (select CheckpointDateInvoice from Customer where CustomerId = @PayerId)
	order by InvoiceId asc


exec [dbo].[MIMS.CustomerDoc.Due3] 90446



select *
from customer
where customerid in (89203,117304,95285)



select *
from Transactions
where Payerid = 86729
order by modifiedon

select a.*
from Transactions as a inner join Customer as b on a.PayerId = b.CustomerId and BalanceinvoiceiD IS NULL
where a.operation = 1
and a.DateFrom > '2023/07/01'



select *
--into #NewSubs
from Transactions
where operation = 16
and datefrom > '2024/07/01'
and Payerid = 93727


exec [dbo].[MIMS.CashBook_AgeAnalysis5]

select *
from Subscription
where InvoiceId = 85086

select *
from Transac
where Payerid = 114327
and ModifiedOn > '2023/01/01'
order by MOdifiedon


select *
from 


update customer
set BalanceInvoiceId



select distinct Payerid
into #PreviousFY
from Transactions
where operation = 16
and datefrom between 2023/07/01 and '2024/07/01'
order by Payerid

select *
from #NewSubs
where PayerId not in (select PayerId from #PreviousFY)
order by PayerId

select c.BalanceInvoiceId, b.PayerId, b.InvoiceId, a.SubscriptionId, a.DateFrom, b.VatPercentage, 'Value' = (b.Unitprice * b.NumberOfIssues * UnitsPerIssue), b.ProductId
--into #Subscription -- Select all subscriptions after CheckpointdateInvoice.
from Transactions as a inner join Subscription as b on a.SubscriptionId = b.SubscriptionId
inner join Customer as c on a.PayerId = c.CustomerId
where a.Operation in (16)
and b.InvoiceId >= isnull(c.BalanceInvoiceId, 0)
and b.UnitPrice > 0
and b.InvoiceId is not null -- Noninvoiced subscriptions are ignored.
and b.SubscriptionId = 242398

select *
from Customer
where customerid = 2508