
update Customer
set CheckpointDateInvoice = '2021/07/01'
where CheckpointDateInvoice < '2021/07/01'

update Customer
set CheckpointDatePayment = '2021/07/01'
where CheckpointDatePayment < '2021/07/01'

exec [dbo].[MIMS.CashBook_AgeAnalysis2]


-- Run job to reallocate allpayments for the new era. 


select *
from Exception
where modifiedon > '2023/07/18 08:54'




