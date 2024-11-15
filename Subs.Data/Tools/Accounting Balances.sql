select distinct PayerId, RunDate, DueOnRundate, LiableOnRundate
into #RunDate
from FactAccounting
where Rundate = '2024/11/04'
order by PayerId    -- 5450

drop table #DeltaDue

--  Delta Due on '2024/07/01'
select PayerId, 'Due' = sum(inc) 
into #DeltaDue
from FactAccounting
where RunDate = '2024/11/04'
and DateFrom between '2024/07/01' and '2024/11/04'
and operationId in (1, 4, 19, 22, 23,24,25)
group by PayerId




drop table #DeltaLiability

--  Delta Liability on '2024/07/01'
select PayerId, 'Maand' = cast(Year(DateFrom) as nchar(4)) + FORMAT( Cast(Month(DateFrom) as nchar(2)), 'Liability' = sum(inc) 
--into #DeltaLiability
from FactAccounting
where RunDate = '2024/11/04'
and DateFrom between '2024/07/01' and '2024/11/04'
and operationId in (2, 12, 19, 22, 24, 27)
and payerid = 4
group by PayerId, Year(DateFrom), Month(DateFrom)













-- Save it

select a.PayerId, 'RunDate' = a.Rundate, 'Month' = '2024/07/01', 'RunDue' = a.DueOnRundate, 'DeltaDue' = b.Due, 'Due' = a.DueOnRunDate - isnull(b.Due, 0)
into #Due
from #Rundate as a left outer join #DeltaDue as b on a.PayerId = b.PayerId
order by Payerid


select a.PayerId, 'RunDate' = a.Rundate, 'Month' = '2024/07/01', 'RunLiability' = a.LiableOnRundate, 'DeltaLiability' = b.Liability, 'Liability' = a.LiableOnRunDate - isnull(b.Liability, 0)
into #Liable
from #Rundate as a left outer join #DeltaLiability as b on a.PayerId = b.PayerId

-- Join into FactBalance
insert into FactBalance([PayerId], [RunDate], [Month], [RunDue], [DeltaDue], [Due], [RunLiability], [DeltaLiability], [Liability])
select a.*, b.RunLiability, b.DeltaLiability, b.Liability
from #Due as a inner join #Liable as b on a.PayerId = b.PayerId and a.Rundate = b.Rundate and a.[Month] = b.[Month]


-- Add the Balances into Accounting


insert into FactAccounting([RunDate], [PayerId], [InvoiceId], DateFrom, [OperationId], [VatPercentage], [Exc], [Vat], [Inc], [DueOnRunDate],[LiableOnRundate])

select a.RunDate, a.PayerId, 0, '2024/07/01', 35, 
       case when b.CountryId = 61 then 15 else 0 end, 
       case when b.CountryId = 61 then a.Due * 0.85 else a.Due end,
	   case when b.CountryId = 61 then a.Due * 0.15 else 0 end,
	   a. Due, 0,0 
from FactBalance as a inner join MIMS3.dbo.Customer as b on a.PayerId = b.CustomerId
union 
select a.Rundate, a.PayerId, 0, '2024/07/01', 21, 
       case when b.CountryId = 61 then 15 else 0 end, 
       case when b.CountryId = 61 then a.Liability * 0.85 else a.Liability end,
	   case when b.CountryId = 61 then a.Liability * 0.15 else 0 end,
	   a. Liability,0,0 
from FactBalance as a inner join MIMS3.dbo.Customer as b on a.PayerId = b.CustomerId

