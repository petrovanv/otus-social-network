"""E2E-тест ленты с кэшем через докеризированный API (порт 5282)."""
import json, time, urllib.request, urllib.error

BASE = "http://localhost:5282"

def call(method, path, token=None, body=None):
    url = BASE + path
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(url, data=data, method=method)
    req.add_header("Content-Type", "application/json")
    if token:
        req.add_header("Authorization", "Bearer " + token)
    try:
        with urllib.request.urlopen(req) as r:
            raw = r.read().decode()
            return r.status, (json.loads(raw) if raw else None)
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode()

def register(email, fn, sn):
    call("POST", "/user/register", body={
        "email": email, "first_name": fn, "second_name": sn,
        "birthdate": "1990-01-01", "city": "Moscow", "password": "password123"})
    # login читает со слейва — ждём догона репликации (read-your-writes lag)
    for _ in range(10):
        _, d = call("POST", "/login", body={"email": email, "password": "password123"})
        if isinstance(d, dict) and "token" in d:
            return d["token"], d["user_id"]
        time.sleep(0.3)
    raise RuntimeError(f"login failed for {email}: {d}")

ts = int(time.time())
print("1) Регистрируем демо-пользователя и 3 друзей")
d_tok, d_id = register(f"feed_demo_{ts}@x.com", "Демо", "Пользователь")
f1_tok, f1_id = register(f"feed_f1_{ts}@x.com", "Друг", "Первый")
f2_tok, f2_id = register(f"feed_f2_{ts}@x.com", "Друг", "Второй")
f3_tok, f3_id = register(f"feed_f3_{ts}@x.com", "Друг", "Третий")
print(f"   demo={d_id}")

print("2) Демо добавляет троих в друзья (friend/set)")
for fid in (f1_id, f2_id, f3_id):
    print("  ", call("PUT", f"/friend/set/{fid}", token=d_tok))

print("3) Друзья создают посты (post/create -> fan-out)")
for tok, name in ((f1_tok, "F1"), (f2_tok, "F2"), (f3_tok, "F3")):
    for i in range(2):
        st, d = call("POST", "/post/create", token=tok, body={"text": f"Пост от {name} #{i}"})
        print(f"   {name}: {d}")

print("4) Демо читает ленту (cache MISS -> rebuild из БД)")
t0 = time.time()
st, feed = call("GET", "/post/feed?offset=0&limit=10", token=d_tok)
print(f"   {len(feed)} постов за {(time.time()-t0)*1000:.1f} ms")
for p in feed:
    print("    -", p["text"], "|", p["author_user_id"][:8])

print("5) Демо читает ленту повторно (cache HIT из Redis)")
t0 = time.time()
st, feed2 = call("GET", "/post/feed?offset=0&limit=10", token=d_tok)
print(f"   {len(feed2)} постов за {(time.time()-t0)*1000:.1f} ms (из кэша)")

print("6) F1 постит ещё раз -> fan-out дожен долить в тёплый кэш демо")
call("POST", "/post/create", token=f1_tok, body={"text": "СВЕЖИЙ пост от F1"})
time.sleep(0.3)
st, feed3 = call("GET", "/post/feed?offset=0&limit=10", token=d_tok)
print(f"   верхний пост: {feed3[0]['text']!r}  (ожидаем 'СВЕЖИЙ пост от F1')")

print("\nИтог:", "OK" if feed3 and feed3[0]["text"] == "СВЕЖИЙ пост от F1" else "ПРОВЕРЬ")
open("demo_feed_user.txt", "w").write(d_id)
