delete from FactAccounting
where Rundate = '2024/11/19'


select distinct Rundate
from FactAccounting


-- DBCC CHECKIDENT ('FactAgeAnalysis', RESEED, 1);

exec [dbo].[SUBSDW.EOM]

exec [dbo].[SUBSDW.EOMCheck] '2024/12/04'



select distinct Rundate
from [dbo].[FactAccounting]

select *
from Transactions as a left outer join subscription as b on a.Invoiceid = b.Invoiceid
where operation = 19
and Reference3 = 0
and datefrom > '2024/01/01'
and b.SubscriptionId is null
order by datefrom 


delete from Transactions
where transactionid in (2623712,
2629449,
2632446)


select *
from Transactions
where PayerId = 121715
and Datefrom > '2024/01/01'


update transactions
set value = 1350
where transactionid = 2632635





select *
from subscription as a left outer join transactions as b on a.Subscriptionid = b.subscriptionid
where b.operation = 16
and datefrom > '2024/08/01'and b.Subscriptionid is null



select *
from exception
where modifiedon  between '2024/08/26' and '2024/08/27'
and severity = 5














exec [dbo].[MIMS.CustomerDoc.Due3] 93745

select *
from Subscription
where invoiceid in (60547)

select *
from Exception
where modifiedon between '2024/08/26' and '2024/08/27'
and severity = 1


--delete from transactions
where transactionid in (2248862, 
2200627)


--update Transactions
set DebitUnits = 3, Debitvalue = 344.519
where transactionid = 2231749



select *
from issue
where issueid in (982,
983,
979,
1067,
1068,
1069,
1070,
980,
981,
1071)

