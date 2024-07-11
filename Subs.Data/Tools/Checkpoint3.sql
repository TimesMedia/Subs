drop table #Raw
drop table #Adjusted

select b.CustomerId, 'Deliverable' = isnull(sum(a.UnitPrice * d.UnitsLeft), 0)
into #Raw
from Subscription as a
inner join Customer as b on a.PayerId = b.CustomerId
inner join SubscriptionIssue as d on a.Subscriptionid = d.SubscriptionId
where d.UnitsLeft <> 0
and a.Status = 1
group by b.CustomerId

select b.CustomerId, 'Deliverable' = isnull(sum(a.UnitPrice * d.UnitsLeft), 0)
into #Adjusted
from Subscription as a
inner join Customer as b on a.PayerId = b.CustomerId
inner join SubscriptionIssue as d on a.Subscriptionid = d.SubscriptionId
where d.UnitsLeft <> 0
and a.Status = 1
and a.InvoiceId >= isnull(b.BalanceInvoiceId, 0)
group by b.CustomerId

--select sum(Deliverable)
--from #Raw
--order by Deliverable

select a.CustomerId, 'Raw' = isnull(a.Deliverable, 0), 'Adjusted' = isnull(b.Deliverable, 0), 'Difference' = isnull(a.Deliverable, 0) - isnull(b.Deliverable, 0)

from #Raw as a inner join #Adjusted as b on a.CustomerId = b.CustomerId
where a.Deliverable <> b.Deliverable
union

select a.CustomerId, 'Raw' = isnull(a.Deliverable, 0), 'Adjusted' = isnull(b.Deliverable, 0), 'Difference' = isnull(a.Deliverable, 0) - isnull(b.Deliverable, 0)
from #Raw as a left outer join #Adjusted as b on a.CustomerId = b.CustomerId
where b.Deliverable is null
and isnull(a.Deliverable, 0) <> 0
union
select a.CustomerId, 'Raw' = isnull(a.Deliverable, 0), 'Adjusted' = isnull(b.Deliverable, 0), 'Difference' = isnull(a.Deliverable, 0) - isnull(b.Deliverable, 0)
from #Raw as a right outer join #Adjusted as b on a.CustomerId = b.CustomerId
where a.Deliverable is null
and isnull(b.Deliverable, 0) <> 0

--******************************************************************************************************************************************

--Customers with transactions in last financial 

select distinct Payerid
into #Active
from Transactions
where modifiedon > '2023/07/01'   -- 5765


-- Identify checkpoint changes over the whole period
select CustomerId, Due, Liable, BalanceInvoiceId
into #Modified
from Customer as a inner join #Active as b on a.CustomerId = b.PayerId
where balanceinvoiceid is not null
and balanceinvoiceid <> 0  -- 3466

-- Identify checkpoint changes affecting the financial year or later

select b.*, a.DateFrom 
into #After
from Transactions as a inner join #Modified as b on a.PayerId = b.CustomerId and a.InvoiceId = b.BalanceInvoiceId
where a.operation = 19
and a.Datefrom > '2023/07/01'
order by b.CustomerId -- 420

-- New customers since beginning of financial year
-- I am assuming that they were not truncated


select a.PayerId
into #New
from Transactions as a inner join #After as b on a.PayerId = b.CustomerId
where operation = 13
and a.Datefrom > '2023/07/01'  -- 101

-- Remove customers created during after start of the financial year

select *
into #MinusNew
from #After
where CustomerId not in (Select PayerId from #New)
order by CustomerId  -- 321

-- Check for transactions during the financial year that migh have been obscured

select top 10 a.CustomerId, count(*)
from #MinusNew as a inner join Transactions as b on a.CustomerId = b.PayerId
where b.Datefrom between '2023/07/01' and DateAdd(d, -1, a.DateFrom)
group by a.CustomerId
order by a.CustomerId -- 130


select b.*
from #MinusNew as a inner join Transactions as b on a.CustomerId = b.PayerId
where b.Datefrom between '2023/07/01' and DateAdd(d, -1, a.DateFrom)
and a.CustomerId = 73




