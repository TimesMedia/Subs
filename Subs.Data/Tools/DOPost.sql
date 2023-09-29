
select *
from Exception
where modifiedon > '2023/06/02'
and severity = 1

select *
from DebitorderBankStatement
where CustomerId = 114480
and transactionDate = '2023/05/31'


select a.*, b.TransactionId
from DebitorderBankStatement as a left outer join Transactions as b on a.CustomerId = b.PayerId and b.Operation = 1 and a.TransactionDate = b.DateFrom
where transactionDate = '2023/05/31'
and posted = 0
and b.PayerId is not null

update DebitorderBankStatement
set posted = 1, PaymentTransactionId = 2491899
where PaymentId = 3683