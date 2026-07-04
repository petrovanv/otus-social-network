"""
Наполнение БД постами и дружескими связями (для демо ленты).

Стратегия:
  - берём N случайных существующих пользователей как "авторов";
  - каждому автору создаём несколько постов (тексты из posts.txt);
  - строим граф дружбы: каждый пользователь из выборки дружит с несколькими другими.
Пишем на мастер (:3406), данные реплицируются на слейвы.
"""
import mysql.connector
import uuid
import random
import os

DB = dict(host="127.0.0.1", port=3406, user="root",
          password="Tenerife12!", database="social_network")

N_AUTHORS      = 2000     # сколько пользователей будут авторами/участниками графа
POSTS_PER_USER = 5        # постов на автора
FRIENDS_PER_USER = 15     # исходящих связей дружбы

def load_texts():
    path = os.path.join(os.path.dirname(__file__), "posts.txt")
    with open(path, encoding="utf-8") as f:
        return [ln.strip() for ln in f if ln.strip()]

def main():
    texts = load_texts()
    conn = mysql.connector.connect(**DB)
    cur = conn.cursor()

    print("Clearing posts/friends...")
    cur.execute("DELETE FROM posts")
    cur.execute("DELETE FROM friends")
    conn.commit()

    # выбираем случайных пользователей
    cur.execute(f"SELECT id FROM users ORDER BY RAND() LIMIT {N_AUTHORS}")
    users = [r[0] for r in cur.fetchall()]
    print(f"Picked {len(users)} users")

    # посты
    posts = []
    for uid in users:
        for _ in range(POSTS_PER_USER):
            posts.append((str(uuid.uuid4()), uid, random.choice(texts)))
    cur.executemany(
        "INSERT INTO posts (id, author_user_id, text) VALUES (%s, %s, %s)", posts)
    conn.commit()
    print(f"Inserted {len(posts)} posts")

    # дружба (направленно): каждый подписан на FRIENDS_PER_USER других
    friends = set()
    for uid in users:
        pool = random.sample(users, min(FRIENDS_PER_USER + 1, len(users)))
        for fid in pool:
            if fid != uid:
                friends.add((uid, fid))
    cur.executemany(
        "INSERT IGNORE INTO friends (user_id, friend_id) VALUES (%s, %s)", list(friends))
    conn.commit()
    print(f"Inserted {len(friends)} friendship edges")

    # сохраним одного пользователя с гарантированно непустой лентой для демо
    demo = users[0]
    cur.execute("SELECT COUNT(*) FROM friends WHERE user_id=%s", (demo,))
    fcount = cur.fetchone()[0]
    with open(os.path.join(os.path.dirname(__file__), "demo_user.txt"), "w") as f:
        f.write(demo)
    print(f"Demo user {demo} has {fcount} friends -> demo_user.txt")

    conn.close()

if __name__ == "__main__":
    main()
