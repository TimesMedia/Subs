delete from FactAgeAnalysis
where Rundate = '2024/08/21'

select * -- max(AgeAnalysisId)
from FactAgeAnalysis



-- DBCC CHECKIDENT ('FactAgeAnalysis', RESEED, 1);


exec [dbo].[SUBSDW.EOM]

select *



select *
from FactAgeAnalysis
where Rundate = '2024/08/20'
order by INvoiceId








-- Check Debtors



Select *
from Transactions
where operation = 12
and payerId = 608
and datefrom between '2022/0301' and '2022/03/01'


exec [dbo].[MIMS.CustomerDoc.Due3] 121513



select *
from subscription
where subscriptionId = 236891

select *
from Transactions
where operation = 19
and invoiceid = 84009




--delete from Transactions
where transactionid in (2339812,
2339813)

select *
from #Trouble
where payerid = 56

drop table #Returned

select a.TransactionId, a.PayerId, a.CreditValue, b.Difference, a.DateFrom
-- into #Returned
from Transactions as a inner join #Trouble  as b on a.PayerId = b.PayerId and Abs(a.CreditValue - b.Difference) < 1 
where a.operation = 12
order by dateFrom

Delete from Transactions
where transactionid in (1910322,
1820970,
1838748,
2422686,
1902521,
1995031,
2261500,
2323471,
2146162,
1916991)







and DateFrom >= '2017/06/01'



select SubscriptionId, IssueId, DateFrom 
into #Expired
from Transactions
where operation = 10
and SubscriptionId in (select SubscriptionId from #Returned)
order by DateFrom

select *
from #Expired as a inner join #Returned as b on a.SubscriptionId = b.SubscriptionId
where a.DateFrom > b.DateFrom
order by b.dateFrom

select *
from transactions
where operation in (2, 12, 27, 19, 22)
and payerId = 60
and datefrom >= '2020/06/09'
order by datefrom 

 




select sum(Inc)
from FactAgeAnalysis
where PayerId = 56
and Rundate = '2024/08/18'
and operationId in (2, 12, 27, 19, 22)
group by



exec [dbo].[MIMS.CustomerDoc.Deliverable] 56


select *
from Transactions
where operation = 12
and payerid = 56
and subscriptionId = 185403
order by DateFrom

 






















-- INvoices without Subscriptions pointing to them 

drop table #Invoices
drop table #Subs

select InvoiceId
into #Invoices
from Transactions
where operation = 19



select distinct b.InvoiceId
into #Subs
from Transactions as a inner join Subscription as b on a.Subscriptionid = b.SubscriptionId
where operation = 16


select *
from #Invoices
where InvoiceId not in (select distinct InvoiceId from #Subs)
order by InvoiceId    ---   thsi does not work

select a.InvoiceId
into #Phantom
from #Invoices as a left outer join #Subs as b on a.InvoiceId = b.InvoiceId 
where b.InvoiceId is null
order by a.InvoiceId

delete from Transactions
where operation = 19
and invoiceid in (Select InvoiceId from #Phantom)


select distinct InvoiceId 
from #Subs
where invoiceid = 87270
order by InvoiceId


select distinct b.InvoiceId
from Transactions as a left outer join Subscription as b on a.Subscriptionid = b.SubscriptionId
where operation = 16
and datefrom > '2024/01/01'
and b.SubscriptionId is null

select *
from Subscription
where invoiceid = 87270










select *
from #INvoices
where InvoiceId not in (select InvoiceId from #Subs)












delete from Transactions
where operation = 19 
and invoiceid = 87541


select *
from Transactions
where operation = 19 
and Value = 720.0200
and transactionid > 2422156