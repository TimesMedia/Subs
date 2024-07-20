select *
from Exception
where modifiedon > '2024/07/20'

exec [dbo].[MIMS.CustomerDoc.Customer.FillById] 'ActiveCustomer', 0, ''

drop table #Dormant1
drop table #Dormant2


Select  distinct 'PayerId' = CustomerId
into #Dormant1
From   Customer
where abs(due)  > 1
order by PayerId


select distinct a.PayerId
into #Dormant2
from #Dormant1 as a inner join Transactions as b on a.PayerId = b.PayerId and b.operation not in (11,33)
   and b.DateFrom > DATEFROMPARTS(Year(Dateadd(yyyy, -1, GetDate())), 7,1)

select a.*, b.Due
from #Dormant1  as a inner join Customer as b on a.PayerId = b.CustomerId
where a.PayerId not in (select payerid from #Dormant2)





