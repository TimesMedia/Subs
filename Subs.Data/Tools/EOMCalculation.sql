select Comment
from Exception
where modifiedon > '2023/02/09 21:00'
and Comment like 'Customer%'



select PayerId, 'Datum' = max(Datum)
into #Temp
from LiabilityReport
where Datum <= '2023/01/31'
group by PayerId


select a.PayerId, a.Balance
into #Temp2
from LiabilityReport as a inner join #Temp as b on a.PayerId = b.Payerid and a.Datum = b.Datum

select Payerid, count(*)
from #Temp2
group by PayerId
having count(*) > 1

Truncate table LiabilityReport




select *
from Customer
where CheckpointdatePayment > '2022/11/30'

778
1363
1841
87441
107375



select *
from Customer
where CheckpointdateInvoice > '2022/11/30'


792
6512
89071
99472
101426
107108
108244
113964



select *
LiabilityEOM
where Datum in ('2022/12/31', '2023/01/31')
and Payerid in (88290,
107978,
90102,
106933,
7981,
111798,
9437,
257,
114953)



2022-11-30	7981	617.526246
2022-11-30	9437	4498.596133
2022-11-30	88290	333.025395
2022-11-30	90102	-0.000128
2022-11-30	106933	0.000012
2022-11-30	107978	-807.802500
2022-11-30	111798	377.492430
2022-11-30	114953	-0.003330
2023-01-31	107978	-807.802500

select *
from LiabilityEOM
where Datum in ('2022/12/31', '2023/01/31')
where payerid in 






select Datum, sum(liability)
from LiabilityEOM
where Datum in ('2022/12/31', '2023/01/31')
group by Datum


drop table #January

select *
into #December
from LiabilityEOM
where Datum in ('2022/12/31')

select *
into #January
from LiabilityEOM
where Datum in ('2023/01/31')



select a.PayerId, 'Liability' = isnull(a.Liability, 0) - isnull(b.Liability, 0)
into #All
from #January as a inner join #December as b on a.PayerId = b.PayerId
union all
select PayerId, -Liability
from #December
where PayerId not in (select PayerId from #January) 
union all
select PayerId, Liability
from #January
where PayerId not in (select PayerId from #December) 



select *
from #All
order by PayerId


drop table #Dw

select PayerId, 'Liability' = sum([Value])
into #DW
from SubsDW.dbo.FactLiability
where Datum between '2023/01/01' and '2023/01/31'
group by PayerId
order by Payerid

select *
from #DW
where abs(Liability) > 0.00
order by PayerId




select sum(Liability)
from #All 
where PayerId not in (select distinct PayerId from #DW)  -- 64441.912064

select sum(Liability)
from #DW 
where PayerId not in (select distinct PayerId from #All)    -- 8668.81
order by PayerId

select 64441.912064 + 8668.81



select *
from Transactions 
where operation = 1
and payerid = 208
and Modifiedon > '2023/01/31'

--*******************************************************December*****************************************************************************************************

drop table #Balances12_1
drop table #Balances12_2
drop table #Balances12_3


drop table #Change12


select Payerid, Datum, 'TransactionId' = max(TransactionId)
into #Balances12_1
from LiabilityReport
where Datum <= '2022/12/31'
group by PayerId, Datum

select PayerId, 'Datum' = max(Datum)
into #Balances12_2
from #Balances12_1
group by PayerId

select  a.PayerId, a.TransactionId
into #Balances12_3
from  #Balances12_1 as a inner join #Balances12_2 as b on a.PayerId = b.PayerId and a.Datum = b.Datum

select a.PayerId, a.Balance
into #December
from LiabilityReport as a inner join #Balances12_3 as b on a.TransactionId = b.TransactionId
order by PayerId

select  sum(a.Balance)
from LiabilityReport as a inner join #Balances12_3 as b on a.TransactionId = b.TransactionId


select Payerid, 'Change' = sum(Value)
into #Change12
from FactLiability
where Datum between '2022/12/01' and '2022/12/31'
group by PayerId

select sum(Change)
from #Change12


select count(*)
from #Change12




--*******************************************************January*****************************************************************************************************
drop table #Balances01
drop table #Change01
drop table #December

select Payerid, Datum, 'TransactionId' = max(TransactionId)
into #Balances01_1
from LiabilityReport
where Datum <= '2023/01/31'
group by PayerId, Datum

select PayerId, 'Datum' = max(Datum)
into #Balances01_2
from #Balances01_1
group by PayerId

select  a.PayerId, a.TransactionId
into #Balances01_3
from  #Balances01_1 as a inner join #Balances01_2 as b on a.PayerId = b.PayerId and a.Datum = b.Datum




select a.PayerId, a.Balance
into #January
from LiabilityReport as a inner join #Balances01_3 as b on a.TransactionId = b.TransactionId
order by PayerId

select  sum(a.Balance)
from LiabilityReport as a inner join #Balances01_3 as b on a.TransactionId = b.TransactionId


select a.PayerId, 'December' = a.Balance, 'January' = b.Balance, 'Difference' = b.Balance - a.Balance
into #Overlap
from #December as a inner join #January as b on a.PayerId = b.PayerId
where abs(b.Balance - a.Balance) > 0
order by PayerId


select a.PayerId, 'Change' = sum(Value)
into #Changes
from LiabilityReport as a inner join #Overlap as b on a.PayerId = b.PayerId
where Datum between '2023/01/01' and '2023/01/31'
group by a.PayerId


select a.PayerId, a.[Difference], b.Change
from #Overlap as a inner join #Changes as b on a.PayerId = b.PayerId
where a.[Difference] <> b.Change
order by a.PayerId



select *
from LiabilityReport 
where PayerId = 123
and Datum between '2023/01/01' and '2023/01/31'

select *
from LiabilityReport 
where PayerId = 123
and Datum <= '2022/12/31'
order by Datum, TransactionId








-- **********************************************************************************************************








