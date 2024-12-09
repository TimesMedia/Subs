select
c.CustomerId, 'FirstName' = c.firstname, 'Surname' = c.surname, 'Service Provider' = 'Arena Holdings Media MIMS',
'Accreditation Number' = d.AccreditationNumber,
'CPD Description' = d.Naam,
'Module' = b.Naam,
'CPD Type' = case when b.EthicsPoints > 0 and b.NormalPoints > 0 then 'Both'
                  when b.EthicsPoints > 0 and b.NormalPoints = 0 then 'Ethics'
                  else 'Clinical' end,
b.EthicsPoints,
b.NormalPoints,
'CPD Points' = case when b.EthicsPoints > 0 and b.NormalPoints > 0 then 0
                  when b.EthicsPoints > 0 and b.NormalPoints = 0 then b.EthicsPoints
                  else b.NormalPoints
               end,
 a.Datum
--into #Result
from result as a inner join Module as b on a.ModuleId = b.ModuleId
inner join Mims3.dbo.Customer as c on a.CustomerId = c.CustomerId
inner join Survey2 as d on b.SurveyId = d.SurveyId
where a.Pass = 1
and a.Datum > '2022/01/01'
order by c.CouncilNumber, a.Datum
  
select [Registration Number], [Service Provider], [Accreditation Number], [CPD Description], [CPD Type],
 'CPD Points' =  convert(nvarchar(2), sum(EthicsPoints)) + '|'  + convert(nvarchar(2), sum(NormalPoints)),
  'Activity Start Date' = convert(nvarchar(2), DatePart(mm, min(Datum))) + '/'
                      + convert(nvarchar(2), DatePart(dd, min(Datum))) + '/'
                                                              + convert(nvarchar(4), DatePart(yyyy, min(Datum))),
'Activity End Date' = convert(nvarchar(2), DatePart(mm, max(Datum))) + '/'
                      + convert(nvarchar(2), DatePart(dd, max(Datum))) + '/'
                                                              + convert(nvarchar(4), DatePart(yyyy, max(Datum)))
into #Result2
from #Result as a
where EthicsPoints > 0 and NormalPoints > 0
group by [Registration Number], [Service Provider], [Accreditation Number], [CPD Description],[CPD Type]
union all
 
select [Registration Number], [Service Provider], [Accreditation Number], [CPD Description], [CPD Type],
 'CPD Points' =  convert(nvarchar(2), sum(EthicsPoints)),
  'Activity Start Date' = convert(nvarchar(2), DatePart(mm, min(Datum))) + '/'
                      + convert(nvarchar(2), DatePart(dd, min(Datum))) + '/'
                                                              + convert(nvarchar(4), DatePart(yyyy, min(Datum))),
'Activity End Date' = convert(nvarchar(2), DatePart(mm, max(Datum))) + '/'
                      + convert(nvarchar(2), DatePart(dd, max(Datum))) + '/'
                                                              + convert(nvarchar(4), DatePart(yyyy, max(Datum)))
from #Result as a
where EthicsPoints > 0 and NormalPoints = 0
group by [Registration Number], [Service Provider], [Accreditation Number], [CPD Description],[CPD Type]
 
union all
 
select [Registration Number], [Service Provider], [Accreditation Number], [CPD Description], [CPD Type],
 'CPD Points' =  convert(nvarchar(2), sum(NormalPoints)),
  'Activity Start Date' = convert(nvarchar(2), DatePart(mm, min(Datum))) + '/'
                      + convert(nvarchar(2), DatePart(dd, min(Datum))) + '/'
                                                              + convert(nvarchar(4), DatePart(yyyy, min(Datum))),
'Activity End Date' = convert(nvarchar(2), DatePart(mm, max(Datum))) + '/'
                      + convert(nvarchar(2), DatePart(dd, max(Datum))) + '/'
                                                              + convert(nvarchar(4), DatePart(yyyy, max(Datum)))
from #Result as a
where EthicsPoints = 0 and NormalPoints > 0
group by [Registration Number], [Service Provider], [Accreditation Number], [CPD Description],[CPD Type]
 
Select *
from #Result2
order by [Registration Number], [Service Provider], [Accreditation Number], [CPD Description],[CPD Type]

 