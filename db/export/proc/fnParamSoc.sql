create function fnParamSoc (@codSoc int, @paramName varchar(128), @defaValue nvarchar(1024)) returns varchar(1024) as
begin
  return isnull( (select top 1 param_value from setari_societate where codSoc = @codSoc and param_name like @paramName), @defaValue)
end

