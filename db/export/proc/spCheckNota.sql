create procedure spCheckNota (@Jurnal varchar(254) = null, @Nr varchar(100) = null, @Data datetime = null, @IDUtilizator int = null)
as
begin
  set nocount on
  set @Jurnal = isnull(@Jurnal, '')
  set @Nr = isnull(@Nr, '')
  set @Data = isnull(@Data, getdate())
  

  declare @result table (cod int, descriere varchar(254))
  insert into @result (cod)
  select
    case 
      when exists(select top 1 1 from cnote where stare = 1 and jurnal like @Jurnal and NrDoc like @Nr and data = @Data) then 1 
      when exists(select top 1 1 from cnote where stare = 1 and jurnal like @Jurnal and NrDoc like @Nr and month(data) = month(@Data) and year(data) = year(@data)) then 2
      when exists(select top 1 1 from cnote where stare = 1 and NrDoc like @Nr and data = @Data) then 3
    else 0 end

  update @result set descriere = 
      case when cod = 1 then 'Nota cu numarul ' + @Nr + ' din ' + convert(varchar(10), @data, 103) + ' a mai fost introdusa'
           when cod = 2 then 'Nota cu numarul ' + @Nr + ' a mai fost introdusa in luna ' + .dbo.fnLunaStr(month(@data)) +  ' in data de ' + 
              convert(varchar(10), (select top 1 data from cnote where stare = 1 and jurnal like @Jurnal and NrDoc like @Nr and month(data) = month(@Data) and year(data) = year(@data)), 103) 
           when cod = 3 then 'Nota cu numarul ' + @Nr + ' din ' + convert(varchar(10), @data, 103) + ' a mai fost introdusa in jurnalul '  + 
            (select top 1 jurnal from cnote where stare = 1 and NrDoc like @Nr and data = @Data) 
      else 'Nu exista' end

  set nocount off

  select * from @result
end

