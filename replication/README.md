# Репликация MySQL — ДЗ

Докеризированное приложение: **.NET 8 API + кластер MySQL** (1 master + 2 slave),
потоковая GTID-репликация, полусинхронный (semi-sync) кворум, файловер.

## Быстрый старт (одна команда)

```bash
cd replication

# Поднимает ВСЁ: backend (:5282) + master(:3406) + slave1(:3407) + slave2(:3408)
docker compose up -d --build

# Настроить GTID-репликацию (слейвы получают схему+индекс с мастера)
powershell -File setup-replication.ps1

# Включить кворумную semi-sync репликацию
powershell -File setup-semisync.ps1
```

После этого API доступен на **http://localhost:5282/swagger**.
Схема таблицы + индекс `idx_name_search` создаются автоматически на мастере
(`init/init.sql`) и реплицируются на слейвы.

### Наполнение данными (для нагрузочных тестов)

```bash
# перенести 1M анкет из локальной БД (:3306) в мастер — реплицируется на слейвы
powershell -File migrate-data.ps1
```

## Сервисы docker-compose

| Сервис | Порт | Роль |
|--------|------|------|
| `app` (sn-app) | 5282→8080 | .NET 8 Web API, читает со слейвов, пишет на мастер |
| `mysql-master` | 3406 | приём записи, источник репликации |
| `mysql-slave1` | 3407 | реплика (чтение) |
| `mysql-slave2` | 3408 | реплика (чтение) |

## Нагрузочное тестирование чтения

```bash
cd loadtest
python export-ids.py           # выгрузить 10k id для k6

# фаза 1 — чтение с мастера (в appsettings UseReplicas=false)
powershell -File run-all.ps1 -Phase baseline

# фаза 2 — чтение со слейвов (UseReplicas=true, перезапустить API)
powershell -File run-all.ps1 -Phase slaves

python collect-results.py      # свести результаты в summary.json
```

## Тест файловера

```bash
cd loadtest
# терминал 1: непрерывная запись с подсчётом коммитов
python write-load.py

# терминал 2: убить мастер под нагрузкой
docker kill mysql-master
# затем в терминале 1 создать stop.flag (touch stop.flag)

cd ..
powershell -File failover.ps1  # промоут самого свежего слейва
```

## Роутинг чтения/записи в приложении

`backend/SocialNetwork/Services/ConnectionRouter.cs` — аналог Spring `ReplicationRoutingDataSource`:
- запись (`CreateAsync`) → мастер;
- чтение (`GetByIdAsync`, `SearchAsync`) → round-robin по слейвам.

Управляется секцией `Replication` в `appsettings.json`:
```json
"Replication": {
  "UseReplicas": true,
  "Slaves": [ "...:3407...", "...:3408..." ]
}
```

## Результаты

Отчёт: [../report/replication.html](../report/replication.html) · [PDF](../report/replication.pdf)

Ключевой вывод: на одной физической машине read-replica не ускоряет чтение (нет нового железа),
но разгружает мастер и даёт HA. Semi-sync обеспечил файловер **без потери транзакций**
(2432 подтверждённых коммита пережили kill мастера).
