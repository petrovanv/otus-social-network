# Migrate data from local MySQL (3306) to cluster master (3406)
# Dump includes table structure and indexes (incl. idx_name_search from HW2)

$ErrorActionPreference = "Stop"
$MYSQL = "C:\Program Files\MySQL\MySQL Server 8.0\bin"
$DUMP = "$env:TEMP\social_network_dump.sql"

Write-Host "1. Dumping local database (1M users)..."
cmd /c "`"$MYSQL\mysqldump.exe`" -h 127.0.0.1 -P 3306 -u admin -pTenerife12! --single-transaction --set-gtid-purged=OFF --default-character-set=utf8mb4 social_network users > `"$DUMP`" 2>nul"
if ($LASTEXITCODE -ne 0) { throw "mysqldump failed" }

$size = [math]::Round((Get-Item $DUMP).Length / 1MB, 1)
Write-Host "   Dump: $DUMP ($size MB)"

Write-Host "2. Loading into cluster master (3406)..."
cmd /c "`"$MYSQL\mysql.exe`" -h 127.0.0.1 -P 3406 -u root -pTenerife12! --default-character-set=utf8mb4 social_network < `"$DUMP`" 2>nul"
if ($LASTEXITCODE -ne 0) { throw "mysql load failed" }

Write-Host "3. Row count on master:"
cmd /c "`"$MYSQL\mysql.exe`" -h 127.0.0.1 -P 3406 -u root -pTenerife12! -N -e `"SELECT COUNT(*) FROM social_network.users;`" 2>nul"

Write-Host "Done. Data replicates to slaves automatically (GTID)."
