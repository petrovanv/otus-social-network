# Настройка GTID-репликации: master + 2 slaves
# Запускать после docker compose up -d, когда все узлы здоровы

$PWD_DB = "Tenerife12!"

function Invoke-MySql($container, $query) {
    # cmd /c гасит stderr-warning'и mysql, которые PowerShell 5.1 превращает в ошибки
    cmd /c "docker exec $container mysql -uroot -p$PWD_DB -e `"$query`" 2>nul"
}

Write-Host "0. Ждём готовности всех узлов..."
foreach ($c in @("mysql-master", "mysql-slave1", "mysql-slave2")) {
    while ((docker inspect -f '{{.State.Health.Status}}' $c) -ne "healthy") { Start-Sleep 3 }
    Write-Host "  $c healthy"
}

Write-Host "1. Создаём пользователя репликации на мастере..."
Invoke-MySql mysql-master "CREATE USER IF NOT EXISTS 'repl'@'%' IDENTIFIED WITH mysql_native_password BY 'ReplPass12!'; GRANT REPLICATION SLAVE ON *.* TO 'repl'@'%'; FLUSH PRIVILEGES;"

foreach ($slave in @("mysql-slave1", "mysql-slave2")) {
    Write-Host "2. Настраиваем $slave..."
    Invoke-MySql $slave "STOP REPLICA; CHANGE REPLICATION SOURCE TO SOURCE_HOST='mysql-master', SOURCE_PORT=3306, SOURCE_USER='repl', SOURCE_PASSWORD='ReplPass12!', SOURCE_AUTO_POSITION=1, GET_SOURCE_PUBLIC_KEY=1; START REPLICA;"
}

Write-Host "3. Статус репликации:"
foreach ($slave in @("mysql-slave1", "mysql-slave2")) {
    Write-Host "--- $slave ---"
    cmd /c "docker exec $slave mysql -uroot -p$PWD_DB -e `"SHOW REPLICA STATUS\G`" 2>nul" |
        Select-String "Replica_IO_Running:|Replica_SQL_Running:|Last_.*Error:" |
        ForEach-Object { Write-Host "  $($_.Line.Trim())" }
}

Write-Host "Done."
