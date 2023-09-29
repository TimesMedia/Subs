select distinct payerid
into #Lost
from transactions
where datefrom between '2017/01/01' and '2019/01/01'
and operation = 16  -- new subscription
except
select distinct payerid
from transactions
where datefrom > '2019/01/01'
and operation = 16

select PayerId, LastOrdered=max(DateFrom)
into #LostDate
from Transactions
where operation = 16
and Payerid in (select PayerId from #Lost)
group by PayerId

select a.LastOrdered, b.*
from #LostDate as a inner join Customer as b on a.PayerId = b.CustomerId
order by a.LastOrdered



 