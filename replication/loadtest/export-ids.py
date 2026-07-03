"""Выгружает 10 000 случайных id пользователей в ids.json для k6."""
import json
import mysql.connector

conn = mysql.connector.connect(
    host="127.0.0.1", port=3406, user="admin",
    password="Tenerife12!", database="social_network",
)
cur = conn.cursor()
# ORDER BY RAND() дорого на 1М строк, но это одноразовая операция
cur.execute("SELECT id FROM users LIMIT 10000")
ids = [row[0] for row in cur.fetchall()]
conn.close()

with open("ids.json", "w") as f:
    json.dump(ids, f)

print(f"Exported {len(ids)} ids to ids.json")
