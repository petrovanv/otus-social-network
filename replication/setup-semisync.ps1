# Включение кворумной (полусинхронной) репликации
# Мастер ждёт ACK минимум от 1 из 2 слейвов перед подтверждением COMMIT

$PWD_DB = "Tenerife12!"

function Invoke-MySql($container, $query) {
    $query | docker exec -i $container mysql -uroot "-p$PWD_DB"
}

Write-Host "1. Master: плагин semi-sync source + включение..."
Invoke-MySql mysql-master "INSTALL PLUGIN rpl_semi_sync_source SONAME 'semisync_source.so';"
Invoke-MySql mysql-master "SET GLOBAL rpl_semi_sync_source_enabled = 1; SET GLOBAL rpl_semi_sync_source_timeout = 10000; SET GLOBAL rpl_semi_sync_source_wait_for_replica_count = 1;"

foreach ($slave in @("mysql-slave1", "mysql-slave2")) {
    Write-Host "2. ${slave}: плагин semi-sync replica + рестарт IO-потока..."
    Invoke-MySql $slave "INSTALL PLUGIN rpl_semi_sync_replica SONAME 'semisync_replica.so';"
    Invoke-MySql $slave "SET GLOBAL rpl_semi_sync_replica_enabled = 1; STOP REPLICA IO_THREAD; START REPLICA IO_THREAD;"
}

Start-Sleep 3
Write-Host "3. Статус semi-sync на мастере:"
"SHOW STATUS WHERE Variable_name IN ('Rpl_semi_sync_source_status','Rpl_semi_sync_source_clients')" |
    docker exec -i mysql-master mysql -uroot "-p$PWD_DB"

Write-Host "Done. Мастер подтверждает COMMIT только после ACK от >=1 слейва."
