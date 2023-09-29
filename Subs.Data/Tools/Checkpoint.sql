select *
from 

update customer
set CheckpointDatePayment = '2022/09/2', CheckpointDateInvoice = '2022/09/02'
where customerid = 114480


update customer
set CheckpointDatePayment = '2017/06/01', CheckpointDateInvoice = '2017/06/01'
where customerid = 114480

exec [dbo].[MIMS.DataContext.InvoicesAndPayments] 114480

select *
from Exception
where modifiedon > '2023/06/09'
and severity = 1