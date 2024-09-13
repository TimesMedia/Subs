Description

ABC QUERY

select
b.CustomerId,
h.IssueDescription,
'BatchDate' = a.DateFrom,
g.[Description],
'Name' = c.[Description] + ' ' + b.Initials + ' ' + b.Surname,
d.CompanyName,
b.Department,
j.Building,
j.Room,
j.FloorNo,
j.StreetNo,
j.Street,
j.StreetExtension,
j.StreetSuffix,
j.Suburb,
j.city,
j.Province,
j.PostCode,
e.CountryName,
b.CellPhoneNumber,
b.PhoneNumber,
b.EmailAddress,
'Value' = a.DebitValue,
'Pieces' = a.DebitUnits,
h.StartDate,
h.EndDate

from Transactions as a inner join Subscription as f on a.SubscriptionId = f.SubscriptionId

inner join Issue as h on h.IssueId = a.IssueId

left outer join EnumTable as g on g.Id = f.DeliveryMethod and g.EnumName = 'DeliveryMethod'

left outer join Customer as b on b.Customerid = a.Receiverid

left outer join EnumTable as c on b.TitleId = c.Id and c.EnumName = 'Title'

left outer join Company as d on d.CompanyId = b.CompanyId

left outer join DeliveryAddressCustomer as i on i.CustomerId = b.CustomerId

left outer join DeliveryAddress as j on j.DeliveryAddressId = i.DeliveryAddressId

left outer join Country as e on e.CountryId = b.CountryId

left outer join DeliveryAddressCustomer as m on m.CustomerId = b.CustomerId

left outer join DeliveryAddress as l on l.DeliveryAddressId = m.deliveryaddressid

where h.IssueId in (1027, 1028, 1029)
and a.DateFrom between '2021/01/03' and '2022/12/31'



Description

WHAT WAS CAPTURED BY WHICH USER ON THE DATABASE

select *
from transactions
where modifiedon >  '2024/03/08 00:00:000'
and ModifiedBy like '%%'

select * from
EnumTable where EnumName = 'operation'

select * from issue
where IssueDescription like '%%'



Outstanding Balances

 

select 'StatementDate' = a.DateFrom, 'StatementValue' = a.Value, CustomerId,  Initials, FirstName, Surname,
d.CountryName, EmailAddress, c.CompanyName
from transactions as a inner join Customer as b on a.PayerId = b.CustomerId
inner join Company as c on b.CompanyId = c.CompanyId
inner join Country as d on b.CountryId = d.CountryId
where a.DateFrom > DateAdd(mm, -10,  GetDate())
and a.Value > '3000'
and a.Value < '550000'
and c.CompanyName like '%mediclinic%'
----and b.EmailAddress like '%medscheme%'
order by StatementValue