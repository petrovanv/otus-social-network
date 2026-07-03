import http from 'k6/http';
import { check } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import { SharedArray } from 'k6/data';

// План нагрузочного тестирования на ЧТЕНИЕ:
//   50% запросов -> GET /user/get/{id}
//   50% запросов -> GET /user/search?first_name=..&last_name=..
// Запуск: k6 run -e TOKEN=<jwt> -e VUS=10 -e OUT=results.json read-load.js

const TOKEN = __ENV.TOKEN;
const BASE  = __ENV.BASE || 'http://localhost:5282';

const ids = new SharedArray('ids', () => JSON.parse(open('./ids.json')));

const FIRST = ['Ал', 'Ан', 'Ив', 'Ни', 'Ми', 'Се', 'Вл', 'Дм', 'Ев', 'Ек'];
const LAST  = ['Ив', 'Смир', 'Куз', 'Пет', 'Ков', 'Лебед', 'Ново', 'Мор', 'Попо', 'Воро'];

export let getLatency    = new Trend('get_latency', true);
export let searchLatency = new Trend('search_latency', true);
export let errorRate     = new Rate('errors');

export let options = {
  scenarios: {
    load: {
      executor: 'constant-vus',
      vus: parseInt(__ENV.VUS || '1'),
      duration: __ENV.DURATION || '30s',
    },
  },
};

export default function () {
  const headers = { Authorization: `Bearer ${TOKEN}` };

  if (Math.random() < 0.5) {
    // GET /user/get/{id}
    const id = ids[Math.floor(Math.random() * ids.length)];
    const res = http.get(`${BASE}/user/get/${id}`, { headers, tags: { api: 'get' } });
    const ok = check(res, { 'get 200': (r) => r.status === 200 });
    errorRate.add(!ok);
    getLatency.add(res.timings.duration);
  } else {
    // GET /user/search
    const fn = encodeURIComponent(FIRST[Math.floor(Math.random() * FIRST.length)]);
    const ln = encodeURIComponent(LAST[Math.floor(Math.random() * LAST.length)]);
    const res = http.get(`${BASE}/user/search?first_name=${fn}&last_name=${ln}`, { headers, tags: { api: 'search' } });
    const ok = check(res, { 'search 200': (r) => r.status === 200 });
    errorRate.add(!ok);
    searchLatency.add(res.timings.duration);
  }
}
