select *
from Exception
where modifiedon between '2023/03/01' and '2023/03/02'
and [Message] like '%Consolidate%'


select *
from Exception
where [Message] like '%120036%'




select *
from subsdw.dbo.FactLiability
where EffectiveDate between '2019/07/01' and '2019/08/01'
and CaptureDate between '2023/03/01' and '2023/04/01'


select *
from Transactions
where transactionid = 2462187
