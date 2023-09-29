drop table #Local

select 'InvoiceDate' = a.DateFrom, a.InvoiceId, c.ProductName, 'Price' = b.UnitPrice * b.UnitsPerIssue * b.NumberOfIssues
from Transactions as a inner join  SubScription as b on a.InvoiceId = b.InvoiceId
inner join Product2 as c on b.ProductId = c.ProductId
inner join Customer as d on a.PayerId = d.CustomerId 
where a.operation = 19
and d.CountryId = 61
and a.datefrom between '2022/11/01' and '2022/11/30'


--**********************************************************

select 'PaymentDate' = a.DateFrom, b.InvoiceId, 'Allocated' = sum(b.Amount)
into #InvoicesPaidThisMonth
from Transactions as a inner join InvoicePayment as b on a.TransactionId = b.PaymentTransactionId
inner join Customer as c on a.PayerId = c.CustomerId 
where operation = 1
and c.CountryId = 61
and a.datefrom between '2022/11/01' and '2022/11/30'
Group by a.dateFrom, b.InvoiceId


-- Price by Invoice of all invoices implicated by payments this month
drop table #pricebyinvoice

select b.PaymentDate, b.InvoiceId, 'Price' = sum(a.UnitPrice * a.UnitsPerIssue * a.NumberOfIssues)
into #PriceByInvoice
from Subscription as a inner join #InvoicesPaidThisMonth as b on a.InvoiceId = b.InvoiceId
group by b.PaymentDate, b.InvoiceId


-- Price by product
drop table #PriceByProduct
select b.PaymentDate, b.InvoiceId, c.ProductName, 'Price' = sum(a.UnitPrice * a.UnitsPerIssue * a.NumberOfIssues)
into #PriceByProduct
from Subscription as a inner join #InvoicesPaidThisMonth as b on a.InvoiceId = b.InvoiceId
inner join Product2  as c on a.ProductId = c.ProductId
group by b.PaymentDate, b.InvoiceId, c.ProductName


drop table #PaymentByFraction
select a.InvoiceId, 'Price' = isnull(a.Price, 0),'Allocated' =  isnull(b.Allocated, 0), 'FractionPaid' = isnull(b.Allocated, 0)/isnull(a.Price, 0)
into #PaymentByFraction
from #PriceByInvoice as a left outer join #InvoicesPaidThisMonth as b on a.InvoiceId = b.InvoiceId

select *
from #PaymentByFraction

select *
from Subscription
where invoiceid = 79162


-- Summary by invoice
drop table #SummaryByInvoice
 
select a.InvoiceId, a.ProductName, 'PricePaid' = a.Price * b.FractionPaid
into #SummaryByInvoice
from #PriceByProduct as a inner join #PaymentByFraction as b on a.InvoiceId = b.InvoiceId
group by a.InvoiceId, a.ProductName, a.Price, a.Price * b.FractionPaid,a.Price - (a.Price * b.FractionPaid)

select *
from  #SummaryByInvoice



-- Include payment dates and full amounts -- 1

drop table #FullPayment

select 'PaymentDate' = a.DateFrom, a.TransactionId, 'FullPayment' = a.CreditValue, c.InvoiceId
into #FullPayment
from Transactions as a inner join Customer as b on a.PayerId = b.Customerid
left outer join InvoicePayment as c on a.TransactionId = c.PaymentTransactionId
where Operation = 1
and b.CountryId = 61
and datefrom between '2022/11/01' and '2022/11/30'
order by a.DateFrom

select *
from InvoicePayment
where PaymentTransactionId in (select TransactionId from #FullPayment)



-- Include payment dates and full amounts -- 2

select a.*, b.ProductName, b.PricePaid
from #FullPayment as a inner join #SummaryByInvoice as b on a.InvoiceId = b.InvoiceId


-- Identify payments not allocated

drop table #Unallocated
select 'PaymentDate' = a.DateFrom, a.TransactionId, 'Full Payment' = max(a.CreditValue), 'InvoiceId' = 0, 'ProductName'  = 'ZNoProduct', 'PricePaid' = max(a.CreditValue) - sum(c.Amount)
into #Unallocated
from Transactions as a inner join Customer as b on a.PayerId = b.Customerid
left outer join InvoicePayment as c on a.TransactionId = c.PaymentTransactionId
where Operation = 1
and b.CountryId = 61
and datefrom between '2022/11/01' and '2022/11/30'
group by a.DateFrom, TransactionId,  c.InvoiceId

-- Include payment dates and full amounts -- 2

select a.*, b.ProductName, b.PricePaid
from #FullPayment as a inner join #SummaryByInvoice as b on a.InvoiceId = b.InvoiceId
union
select *
from #Unallocated
where Abs(PricePaid) > 1




