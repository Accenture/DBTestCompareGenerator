Write-Output "Downloading AdventureWorks2008R2FullDatabaseBackup"
New-Item -Path './zip' -ItemType Directory -Force
dir
Invoke-WebRequest -Uri "https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks2008r2/adventure-works-2008r2-oltp.bak" -Out "./adventure-works-2008r2-oltp.bak"
 

Write-Output "Downloading sqljdbc drivers"
Invoke-WebRequest -Uri "https://go.microsoft.com/fwlink/?linkid=2356503" -OutFile "./zip/sqljdbc_13.4.0.0_enu.zip"
Write-Output "Unzipping sqljdbc drivers"
Expand-Archive -LiteralPath './zip/sqljdbc_13.4.0.0_enu.zip' -DestinationPath "./zip/sqljdbc" -Force
New-Item -Path './jdbc_drivers' -ItemType Directory -Force
Copy-Item -Path "./zip/sqljdbc/sqljdbc_13.4/enu/jars/*"  -Destination "./jdbc_drivers" -Include "mssql-jdbc-*.jar"
Remove-Item -Path  './zip/sqljdbc_13.4.0.0_enu.zip' -Force