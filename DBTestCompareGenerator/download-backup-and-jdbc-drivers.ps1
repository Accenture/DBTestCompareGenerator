Write-Output "Downloading AdventureWorks2008R2FullDatabaseBackup"
New-Item -Path './zip' -ItemType Directory -Force
dir
Invoke-WebRequest -Uri "https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks2008r2/adventure-works-2008r2-oltp.bak" -Out "./adventure-works-2008r2-oltp.bak"
 
Write-Output "Downloading sqljdbc drivers"
Invoke-WebRequest -Uri "https://download.microsoft.com/download/4/0/8/40815588-bef6-4715-bde9-baace8726c2a/sqljdbc_8.2.0.0_enu.zip" -Out "./zip/sqljdbc_8.2.0.0_enu.zip"
Write-Output "Unzipping sqljdbc drivers"

Expand-Archive -LiteralPath './zip/sqljdbc_8.2.0.0_enu.zip' -DestinationPath "./zip/sqljdbc" -Force

Remove-Item -Path  './zip/sqljdbc_8.2.0.0_enu.zip' -Force
New-Item -Path './jdbc_drivers' -ItemType Directory -Force
Copy-Item -Path ./zip/sqljdbc/sqljdbc_8.2/enu/* -Destination "./jdbc_drivers" -Include "mssql-jdbc-*.jar"  