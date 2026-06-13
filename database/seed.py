import csv
import uuid
import mysql.connector
import time
import sys

# Pre-computed BCrypt hash of "password123" — same for all seed users (perf optimization)
DUMMY_HASH = "$2a$11$Ttm5qLCpNkxmVFW8pnQIV.vV0Q2VmfAfN6MbxFdpJFTlUw6.RMPJK"

DB_CONFIG = {
    "host": "127.0.0.1",
    "port": 3306,
    "user": "admin",
    "password": "Tenerife12!",
    "database": "social_network",
    "charset": "utf8mb4",
}

CSV_PATH = r"C:\Users\green\Downloads\people.v2 (1).csv"
BATCH_SIZE = 5000

def seed():
    conn = mysql.connector.connect(**DB_CONFIG)
    cursor = conn.cursor()

    # Truncate existing data
    print("Clearing table...")
    cursor.execute("TRUNCATE TABLE users")
    conn.commit()

    sql = """
        INSERT INTO users (id, email, first_name, second_name, birthdate, gender, biography, city, password_hash)
        VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s)
    """

    batch = []
    total = 0
    start = time.time()

    with open(CSV_PATH, encoding="utf-8", errors="replace") as f:
        for line in f:
            line = line.strip()
            if not line:
                continue

            parts = line.split(",")
            if len(parts) < 3:
                continue

            full_name = parts[0].strip()
            birthdate  = parts[1].strip()
            city       = parts[2].strip()

            name_parts = full_name.split(" ", 1)
            if len(name_parts) < 2:
                continue

            second_name = name_parts[0]   # Фамилия
            first_name  = name_parts[1]   # Имя

            user_id = str(uuid.uuid4())
            email   = f"u{user_id.replace('-','')}@x.com"

            batch.append((
                user_id, email, first_name, second_name,
                birthdate, "", "", city, DUMMY_HASH
            ))

            if len(batch) >= BATCH_SIZE:
                cursor.executemany(sql, batch)
                conn.commit()
                total += len(batch)
                batch = []
                elapsed = time.time() - start
                rps = total / elapsed
                print(f"\r  {total:,} rows inserted ({rps:.0f} rows/sec)...", end="", flush=True)

    # Last batch
    if batch:
        cursor.executemany(sql, batch)
        conn.commit()
        total += len(batch)

    elapsed = time.time() - start
    print(f"\nDone! {total:,} rows in {elapsed:.1f}s ({total/elapsed:.0f} rows/sec)")

    cursor.close()
    conn.close()

if __name__ == "__main__":
    seed()
