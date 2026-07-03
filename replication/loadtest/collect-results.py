"""Собирает все k6-summary в один JSON для отчёта."""
import json, os

RESULTS = os.path.join(os.path.dirname(__file__), "results")
LEVELS = [1, 10, 50, 100]
PHASES = ["baseline", "slaves"]

def metric(m, name, key):
    return round(m.get(name, {}).get(key, 0), 2)

out = {}
for phase in PHASES:
    out[phase] = []
    for vu in LEVELS:
        path = os.path.join(RESULTS, f"{phase}_{vu}vu.json")
        if not os.path.exists(path):
            continue
        m = json.load(open(path))["metrics"]
        out[phase].append({
            "vus": vu,
            "throughput": metric(m, "http_reqs", "rate"),
            "get_p95": metric(m, "get_latency", "p(95)"),
            "get_avg": metric(m, "get_latency", "avg"),
            "search_p95": metric(m, "search_latency", "p(95)"),
            "search_avg": metric(m, "search_latency", "avg"),
        })

print(json.dumps(out, indent=2, ensure_ascii=False))
json.dump(out, open(os.path.join(RESULTS, "summary.json"), "w"), indent=2, ensure_ascii=False)
