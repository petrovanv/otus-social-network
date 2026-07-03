"""
Write-нагрузка для теста файловера.

Пишет строки в таблицу failover_test на мастере (порт 3406) в цикле.
Считает на стороне клиента, сколько COMMIT прошло успешно.
При падении мастера продолжает пытаться (ошибки считаются отдельно),
пока не будет остановлен Ctrl+C или файлом-флагом stop.flag.

По завершении печатает итог: successful_commits — сравнить с COUNT(*)
на новом мастере после промоута.
"""
import mysql.connector
import time
import os
import sys
import uuid

DB = {
    "host": "127.0.0.1", "port": 3406, "user": "admin",
    "password": "Tenerife12!", "database": "social_network",
    "connection_timeout": 3,
}

def ensure_table():
    conn = mysql.connector.connect(**DB)
    cur = conn.cursor()
    cur.execute("""
        CREATE TABLE IF NOT EXISTS failover_test (
            id VARCHAR(36) PRIMARY KEY,
            seq BIGINT NOT NULL,
            created_at TIMESTAMP(6) DEFAULT CURRENT_TIMESTAMP(6)
        )
    """)
    cur.execute("TRUNCATE TABLE failover_test")
    conn.commit()
    conn.close()

def main():
    ensure_table()
    success = 0
    errors = 0
    seq = 0
    conn = None
    start = time.time()

    print("Write load started. Stop: create stop.flag or Ctrl+C")
    try:
        while not os.path.exists("stop.flag"):
            seq += 1
            try:
                if conn is None or not conn.is_connected():
                    conn = mysql.connector.connect(**DB)
                cur = conn.cursor()
                cur.execute(
                    "INSERT INTO failover_test (id, seq) VALUES (%s, %s)",
                    (str(uuid.uuid4()), seq),
                )
                conn.commit()
                success += 1
            except Exception:
                errors += 1
                conn = None
                time.sleep(0.2)  # мастер упал — ретраим с паузой

            if seq % 500 == 0:
                print(f"\r  attempts={seq} success={success} errors={errors}", end="", flush=True)
    except KeyboardInterrupt:
        pass

    elapsed = time.time() - start
    print(f"\n--- RESULT ---")
    print(f"attempts:            {seq}")
    print(f"successful_commits:  {success}")
    print(f"errors:              {errors}")
    print(f"duration:            {elapsed:.1f}s")
    print(f"\nСравните successful_commits с COUNT(*) FROM failover_test на новом мастере.")

if __name__ == "__main__":
    main()
