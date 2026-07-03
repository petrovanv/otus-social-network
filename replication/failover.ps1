# Файловер: выбор самого свежего слейва и промоут его до мастера
# Использование: .\failover.ps1  (после смерти мастера)

$PWD_DB = "Tenerife12!"

function MySqlN($container, $query) {
    ($query | docker exec -i $container mysql -uroot "-p$PWD_DB" -N) -join ""
}
function MySqlExec($container, $query) {
    $query | docker exec -i $container mysql -uroot "-p$PWD_DB" | Out-Null
}

Write-Host "1. Сравниваем GTID слейвов (кто применил больше транзакций)..."
$gtid1 = MySqlN "mysql-slave1" "SELECT @@GLOBAL.gtid_executed"
$gtid2 = MySqlN "mysql-slave2" "SELECT @@GLOBAL.gtid_executed"
Write-Host "  slave1: $gtid1"
Write-Host "  slave2: $gtid2"

function MaxTx($gtid) {
    $max = 0
    foreach ($m in [regex]::Matches($gtid, ":(?:\d+-)?(\d+)")) {
        $v = [long]$m.Groups[1].Value
        if ($v -gt $max) { $max = $v }
    }
    return $max
}
$tx1 = MaxTx $gtid1
$tx2 = MaxTx $gtid2
Write-Host "  slave1 max tx: $tx1 | slave2 max tx: $tx2"

if ($tx1 -ge $tx2) { $newMaster = "mysql-slave1"; $other = "mysql-slave2" }
else               { $newMaster = "mysql-slave2"; $other = "mysql-slave1" }
Write-Host "2. Самый свежий: $newMaster -> промоут до мастера"

# sql_log_bin=0 — служебное создание repl-юзера не пишем в бинлог, иначе second slave споткнётся о дубль
MySqlExec $newMaster "STOP REPLICA; RESET REPLICA ALL; SET GLOBAL super_read_only=0; SET GLOBAL read_only=0; SET SESSION sql_log_bin=0; CREATE USER IF NOT EXISTS 'repl'@'%' IDENTIFIED WITH mysql_native_password BY 'ReplPass12!'; GRANT REPLICATION SLAVE ON *.* TO 'repl'@'%'; FLUSH PRIVILEGES;"

Write-Host "3. Переключаем $other на нового мастера $newMaster..."
MySqlExec $other "STOP REPLICA; CHANGE REPLICATION SOURCE TO SOURCE_HOST='$newMaster', SOURCE_PORT=3306, SOURCE_USER='repl', SOURCE_PASSWORD='ReplPass12!', SOURCE_AUTO_POSITION=1, GET_SOURCE_PUBLIC_KEY=1; START REPLICA;"

Start-Sleep 2
$rows = MySqlN $newMaster "SELECT COUNT(*) FROM social_network.failover_test"
Write-Host "4. Строк в failover_test на новом мастере ($newMaster): $rows"
Write-Host "   Сравните с successful_commits из write-load.py."
Write-Host "Done. Новый мастер: $newMaster"
