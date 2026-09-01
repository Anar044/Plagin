set class=HorecaContext
set namespace=Resto.Front.Api.HorecaControlPlugin.Sql

rem exportsqlce40 "Data Source=.\%class%_orig.sdf" .\%class%.sqlce schemaonly


sqlcecmd40 -d"Data Source=.\%class%.sdf" -e create -n
sqlcecmd40 -d"Data Source=.\%class%.sdf" -i .\%class%.sqlce -n
sqlmetal /namespace:%namespace% /code:%class%.cs .\%class%.sdf
del %class%.sdf 

pause
