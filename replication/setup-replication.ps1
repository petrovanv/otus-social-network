# Настройка GTID-репликации: master + 2 slaves
# Запускать после `docker compose up -d`, когда узлы здоровы.
# Слейвы стартуют пустыми и получают схему+данные с мастера через GTID-репликацию с нуля.

$PWD_DB = "Tenerife12!"

# SQL подаётся через stdin (docker exec -i) — так сложные запросы с кавычками не ломаются
function Invoke-MySql($container, $query) {
    $query | docker exec -i $container mysql -uroot "-p$PWD_DB"
}

Write-Host "0. Ждём готовности всех узлов..."
foreach ($c in @("mysql-master", "mysql-slave1", "mysql-slave2")) {
    while ((docker inspect -f '{{.State.Health.Status}}' $c) -ne "healthy") { Start-Sleep 3 }
    Write-Host "  $c healthy"
}

Write-Host "1. Создаём пользователя репликации на мастере (не пишем в бинлог)..."
Invoke-MySql mysql-master "SET SESSION sql_log_bin=0; CREATE USER IF NOT EXISTS 'repl'@'%' IDENTIFIED WITH mysql_native_password BY 'ReplPass12!'; GRANT REPLICATION SLAVE ON *.* TO 'repl'@'%'; FLUSH PRIVILEGES;"

foreach ($slave in @("mysql-slave1", "mysql-slave2")) {
    Write-Host "2. Настраиваем $slave..."
    Invoke-MySql $slave "STOP REPLICA; RESET REPLICA ALL; CHANGE REPLICATION SOURCE TO SOURCE_HOST='mysql-master', SOURCE_PORT=3306, SOURCE_USER='repl', SOURCE_PASSWORD='ReplPass12!', SOURCE_AUTO_POSITION=1, GET_SOURCE_PUBLIC_KEY=1; START REPLICA;"
}

Start-Sleep 5
Write-Host "3. Статус репликации:"
foreach ($slave in @("mysql-slave1", "mysql-slave2")) {
    Write-Host "--- $slave ---"
    "SHOW REPLICA STATUS\G" | docker exec -i $slave mysql -uroot "-p$PWD_DB" |
        Select-String "Replica_IO_Running:|Replica_SQL_Running:|Last_IO_Error:|Last_SQL_Error:" |
        ForEach-Object { Write-Host "  $($_.Line.Trim())" }
}

Write-Host "Done. Схема и данные приедут на слейвы автоматически."
