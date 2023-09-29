select *
from FactLiability as a inner join Mims3.dbo.Transactions as b on a.TransactionId = b.TransactionId
where a.PayerId <> b.PayerId

drop table #Payers

--************************************************

select distinct Payerid
into #Payers
from MIMS3.dbo.Transactions as a
where operation in ( 2,12,27, 1, 24, 4, 18, 23, 25)
and TransactionId > (select max(TransactionId) from FactLiability)
order by PayerId

select PayerId, 'MaxTransactionId' = max(TransactionId)
into #Existing
from FactLiability
where PayerId in (Select PayerId from #Payers)
group by PayerId
order by PayerId

select PayerId
into #New
from #Payers
except
select PayerId from #Existing

select *
from #Existing
union
select PayerId, 'MaxTransactionId' = 0
from #New








--************************************************


select a.Datum, b.Modifiedon, a.*
from FactLiability as a inner join Mims3.dbo.Transactions as b on a.TransactionId = b.TransactionId
where a.datum <> convert(Date, b.Modifiedon)
and a.Datum between '2022/06/01' and '2022/06/30'
and convert(Date, b.Modifiedon) > '2022/06/30'
order by b.modifiedon








exec [dbo].[LiabilityEOM] 2022, 6

exec [dbo].[LiabilityEOM] 2022, 7

exec [dbo].[LiabilityEOM] 2022, 8

exec [dbo].[LiabilityEOM] 2022, 9

exec [dbo].[LiabilityEOM] 2022, 10

exec [dbo].[LiabilityEOM] 2022, 11

exec [dbo].[LiabilityEOM] 2022, 12

exec [dbo].[LiabilityEOM] 2023, 1

exec [dbo].[LiabilityEOM] 2023, 2







select a.Datum, b.Modifiedon, a.*
from FactLiability as a inner join Mims3.dbo.Transactions as b on a.TransactionId = b.TransactionId
where a.datum <> convert(Date, b.Modifiedon)
and a.Datum between '2022/06/01' and '2022/06/30'
and convert(Date, b.Modifiedon) > '2022/06/30'
order by b.modifiedon


select a.Datum, b.Modifiedon, a.*
from FactLiability as a inner join Mims3.dbo.Transactions as b on a.TransactionId = b.TransactionId
where a.datum <> convert(Date, b.Modifiedon)
and a.Datum between '2022/09/01' and '2022/09/30'
and convert(Date, b.Modifiedon) > '2022/09/30'
order by b.modifiedon



select a.Datum, b.Modifiedon, a.*
from FactLiability as a inner join Mims3.dbo.Transactions as b on a.TransactionId = b.TransactionId
where a.datum <> convert(Date, b.Modifiedon)
and a.Datum between '2022/12/01' and '2022/12/31'
and convert(Date, b.Modifiedon) > '2022/12/31'
order by b.modifiedon
