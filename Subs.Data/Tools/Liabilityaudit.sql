--select a.[PayerId], [CustomerLiability], [JournalLiability], [Difference], b.BalanceInvoiceId, 'Balancedate' = Convert( date, c.DateFrom),
--'DebitOrder' = isnull(d.CustomerId, 0) 
--from Liability as a inner join Customer as b on a.PayerId = b.CustomerId
--inner join Transactions as c on b.BalanceInvoiceId = c.InvoiceId and c.operation = 19
--left outer join SBDebitOrder as d on a.PayerId = d.CustomerId and d.Suspended = 0
--where abs(Difference) > 0.2
--and Datum is not null

exec [dbo].[MIMS.CustomerDoc.DueSum] 112490


exec [dbo].[MIMS.CustomerDoc.Deliverable] 112490



select a.InvoiceId, 'Deliverable' = sum(a.UnitPrice * d.UnitsLeft)
from Subscription as a
inner join Customer as b on a.PayerId = b.CustomerId
inner join SubscriptionIssue as d on a.Subscriptionid = d.SubscriptionId
where d.UnitsLeft <> 0
and a.Status = 1
and a.PayerId = 112490
and a.InvoiceId >= b.BalanceInvoiceId
group by a.InvoiceId


-- Find all deliveries that occured before the existence of invoice.

drop table #Temp1

select a.PayerId, c.BalanceInvoiceId, 'InvoiceBalanceDate' = a.dateFrom
into #Temp1
from Transactions as a inner join Liability as b on a.PayerId = b.PayerId
inner join Customer as c on a.PayerId = c.CustomerId
where a.operation = 19
and a.InvoiceId = c.BalanceInvoiceId

drop table #Payers

select  a.PayerId --'DeliveryDate' = a.DateFrom, b.InvoiceBalanceDate, c.DeliverOnCredit, a.*
into #Payers
from Transactions as a inner join  #Temp1 as b on a.PayerId = b.PayerId
inner join SubscriptionIssue as c on a.SubscriptionId = c.SubscriptionId and a.IssueId = c.IssueId
inner join Subscription as d on a.SubscriptionId = d.SubscriptionId
where operation = 2
--and a.PayerId = 120954
and a.DateFrom < b.InvoiceBalanceDate
and d.InvoiceId >= b.BalanceInvoiceId 
group by a.PayerId
order by a.PayerId

drop table #Proposal

select a.CustomerId, a.BalanceInvoiceId, 'NewBalanceInvoiceid' = isnull(min(c.InvoiceId),0)
into #Proposal
from Customer as a inner join #Payers as b on a.CustomerId = b.PayerId
inner join Transactions as c on a.CustomerId = c.PayerId and c.operation = 19
where c.InvoiceId > a.BalanceInvoiceid
and c.DateFrom > '2021/04/09'
group by a.CustomerId, a.BalanceInvoiceId








select *
from Customer
where BalanceInvoiceId = 0





update Customer
set BalanceInvoiceId = b.NewBalanceInvoiceid
from Customer as a inner join #Proposal as b on a.CustomerId = b.CustomerId






