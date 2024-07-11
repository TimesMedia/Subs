select Payerid, datefrom, 'Oldinvoice' = Reference2, 'NewInvoice' = InvoiceId
from transactions
where operation = 26
and datefrom between '2024/05/01' and  '2024/06/01'  -- Checkpoints during May 2024
order by PayerId, datefrom