delete from FactAgeAnalysis
where Rundate = '2024/08/20'

select * -- max(AgeAnalysisId)
from FactAgeAnalysis



-- DBCC CHECKIDENT ('FactAgeAnalysis', RESEED, 1);


exec [dbo].[SUBSDW.EOM]



select *
from FactAgeAnalysis
where Rundate = '2024/08/20'
order by INvoiceId








-- Check Debtors


drop table #Due

select PayerId, 'Due' = sum(Inc), 'DueStatement' = Max(DueOnRundate)
into #Due
from FactAgeAnalysis
where operationId in (1, 4, 19,22, 23, 24,25)
and Rundate = '2024/08/20'
group by PayerId
order by PayerId

select *
from #Due
where Abs(Due-DueStatement) > 1
order by PayerId


-- Check liability


drop table #Liability

select PayerId, 'Liability' = sum(Inc), 'LiabilityStatement' = Max(LiableOnRundate)
into #Liability
from FactAgeAnalysis
where operationId in (2, 12, 27, 19, 22)
and Rundate = '2024/08/20'
group by PayerId
order by PayerId


select PayerId, operationId, Inc, LiableOnRundate
from FactAgeAnalysis
where operationId in (2, 12, 27, 19, 22)
and Rundate = '2024/08/20'


select *, 'Difference' = Liability-LiabilityStatement
--into #Trouble
from #Liability
where Abs(Liability-LiabilityStatement) > 1
order by PayerId   -- 113


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

select InvoiceId
into #Invoices
from Transactions
where operation = 19
and datefrom > '2024/01/01'


select distinct b.InvoiceId
into #Subs
from Transactions as a inner join Subscription as b on a.Subscriptionid = b.SubscriptionId
where operation = 16
and datefrom > '2024/01/01'

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