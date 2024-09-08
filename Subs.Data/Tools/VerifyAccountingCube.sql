delete from FactAgeAnalysis
where Rundate = '2024/09/07'


select *
from SubscriptionIssue

-- DBCC CHECKIDENT ('FactAgeAnalysis', RESEED, 1);

exec [dbo].[SUBSDW.EOM]

exec [dbo].[SUBSDW.EOMCheck] '2024/09/08'



select distinct Rundate
from [dbo].[FactAgeAnalysis]

select *
from Transactions
where PayerId = 93745
order by datefrom 


exec [dbo].[MIMS.CustomerDoc.Due3] 93745

select *
from Subscription
where invoiceid in (60547)

select *
delete from Transactions
where operation = 2
and SubscriptionId in (232044)



select *
from SubscriptionIssue 
where SubscriptionId in (186492, 186493)


, 63217, 65315)





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

