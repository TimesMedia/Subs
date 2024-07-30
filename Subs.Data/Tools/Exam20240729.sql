truncate table FactAgeAnalysis


DBCC CHECKIDENT ('FactAgeAnalysis', RESEED, 1);


exec [dbo].[SUBSDW.EOM]

drop table #Due

select PayerId, 'Due' = sum(Inc), 'DueStatement' = Max(DueOnRundate)
into #Due
from FactAgeAnalysis
where operationId in (1, 4, 19,22, 23, 24,25)
group by PayerId
order by PayerId

select *
from #Due
where Abs(Due-DueStatement) > 1
order by PayerId


select *
from Transactions
where payerId = 49
and operation = 19
and datefrom > '2017/06/01'
order by datefrom

exec [dbo].[MIMS.CustomerDoc.Due3] 121187

select 'PayerId' = a.PayerId, 'InvoiceId' = a.InvoiceId, 'DateFrom' = a.DateFrom, 
 'VatPercentage' = (select  top 1 [Percentage] = isnull(Percentage, 0) from MIMS3.dbo.VAT where DateFrom <= a.DateFrom order by DateFrom desc),
  a.Value 
from MIMS3.dbo.Transactions as a inner join Mims3.dbo.Customer as b on a.PayerId = b.CustomerId
where operation = 19
and b.BalanceInvoiceId is not null
and b.BalanceInvoiceId <> 0
and a.InvoiceId >= b.BalanceInvoiceId
and b.CustomerId = 447
order by InvoiceId asc




select *
from subscription
where Invoiceid = 85859






select *
delete from Transactions
where Transactionid = 2612151

update Customer
set Balanceinvoiceid = 85883
where customerid = 121187




select * 
from transactions
where subscriptionId = 237833

select * 
from SubscriptionIssue
where subscriptionId = 237833


drop table #Target
drop table #Target2
drop table #Target3

select 'Woord' = Substring(Message,13, 10), ModifiedOn, ModifiedBy
into #Target
from Exception
where Message like '%destroyed%'
and modifiedon > '2017/006/01'


select 'Woord2' =  Substring(Woord,1, CharIndex(' de', Woord)), ModifiedOn, ModifiedBy
into #Target2
from #Target

select *
into #Target3
from #Target2
where Woord2 not like '%-%'
and Woord2 <> ''

select Woord2, SubscriptionId = convert(integer, Woord2)
into #Target4
from #Target3


select a.*
from Subscription as a inner join #Target4 as b on a.SubscriptionId = b.SubscriptionId
inner join SubscriptionIssue as c on a.Subscriptionid = c.SubscriptionId 
order by Modifiedon  -- none

-- Also check for INvoices.

select a.*
from Subscription as a inner join #Target4 as b on a.SubscriptionId = b.SubscriptionId
where a.Invoiceid is not null
order by Modifiedon

select a.*
from Subscription as a inner join #Target4 as b on a.SubscriptionId = b.SubscriptionId
where a.Invoiceid is not null
order by Modifiedon

--delete from Subscription
where SubscriptionId in (select SubscriptionId from #Target4)

select *
--delete from Transactions 
where operation = 19
and invoiceid in (84350,
85859)











select *
from Subscription
where subscriptionid = 237833


select a.*, a.Modifiedon
from Subscription as a left outer join Transactions as b on a.SubscriptionId = b.SubscriptionId and b.operation = 16
where b.SubscriptionId is null
and a.Status <> 6
and a.Modifiedon > '2024/01/01'
order by a.ModifiedOn





select distinct b.InvoiceId
--into #ValidInvoices
from Transactions as a inner join Subscription as b on a.SubscriptionId = b.SubscriptionId
where a.Operation in (16)
and  b.InvoiceId >= 84350
and b.UnitPrice > 0
and b.InvoiceId is not null
and a.payerid = 90446




drop table #Fix

select distinct 'InvoiceId' = a.InvoiceId, 'T'= a.PayerId, 'S' = b.PayerId
into #Fix
from Transactions as a inner join Subscription as b on a.InvoiceId = b.InvoiceId 
where a.operation = 19
and a.PayerId <> b.PayerId


update Transactions
set PayerId = b.S
from Transactions as a inner join #Fix as b on a.InvoiceId = b.InvoiceId and a.PayerId = b.T
and a.operation = 19


select unitprice * NumberofIssues * unitsperissue 
from Subscription
where invoiceid = 79409

drop table #Results
drop table #Result2

select a.InvoiceId, 'InvoiceValue' = max(a.Value), 'SubValue' = sum(b.UnitPrice * UnitsPerIssue * NumberOfIssues)
into #Results
from Transactions as a inner join subscription as b on a.InvoiceId = b.InvoiceId and a.operation = 19
and a.datefrom >= '2015/01/01'
group by a.InvoiceId

select *
into #Result2
from #Results
where Abs(InvoiceValue - Subvalue) > 1


update Transactions
set Value = b.SubValue
from Transactions as a inner join #Result2 as b on a.InvoiceId = b.InvoiceId 
and a.operation = 19


select *
from #Result2
where invoiceid = 64770



select a.PayerId, b.InvoiceId, a.SubscriptionId, a.DateFrom, b.VatPercentage, 'Value' = (b.Unitprice * b.NumberOfIssues * UnitsPerIssue), b.ProductId
--into #Subscription -- Select all subscriptions after CheckpointdateInvoice.
from MIMS3.dbo.Transactions as a inner join MIMS3.dbo.Subscription as b on a.SubscriptionId = b.SubscriptionId
inner join MIMS3.dbo.Customer as c on a.PayerId = c.CustomerId
where a.Operation in (16)
and b.UnitPrice > 0
and b.InvoiceId is not null
and b.PayerId = 118613
order by InvoiceId



select a.SubscriptionId, b.Modifiedon
from Mims3.dbo.Subscription as a inner join MIMS3.dbo.Transactions as b on a.SubscriptionId = b.SubscriptionId and b.Operation = 16
left outer join MIMS3.dbo.Transactions as c on a.InvoiceId = c.InvoiceId and c.operation= 19  
where b.Modifiedon > '2020/01/01'
and c.InvoiceId is null
and b.InvoiceId is not null
and a.Status = 1
order by b.ModifiedOn

-- So, there are many subscriptions that has never been invoiced. Maybe they are all free.
-- All invoiceids since 2020 are real invoices?

exec [dbo].[MIMS.CustomerDoc.Due3] 118613


select *
from MIMS3.dbo.subscription 
where  subscriptionid in (211959,
211960,
211958,
211957)