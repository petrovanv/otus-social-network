import http from 'k6/http';
import { check } from 'k6';
import { Trend, Rate } from 'k6/metrics';

const TOKEN   = __ENV.TOKEN;
const BASE    = 'http://localhost:5282';

const FIRST = ['Ал', 'Ан', 'Ив', 'Ни', 'Ми', 'Се', 'Вл', 'Дм', 'Ев', 'Ек'];
const LAST  = ['Ив', 'Смир', 'Куз', 'Пет', 'Ков', 'Лебед', 'Ново', 'Мор', 'Попо', 'Воро'];

export let latency   = new Trend('search_latency', true);
export let errorRate = new Rate('errors');

export let options = {
  scenarios: {
    load: {
      executor: 'constant-vus',
      vus: parseInt(__ENV.VUS || '1'),
      duration: '30s',
    },
  },
};

export default function () {
  const fn = encodeURIComponent(FIRST[Math.floor(Math.random() * FIRST.length)]);
  const ln = encodeURIComponent(LAST[Math.floor(Math.random() * LAST.length)]);

  const res = http.get(
    `${BASE}/user/search?first_name=${fn}&last_name=${ln}`,
    { headers: { Authorization: `Bearer ${TOKEN}` } }
  );

  const ok = check(res, { 'status 200': (r) => r.status === 200 });
  errorRate.add(!ok);
  latency.add(res.timings.duration);
}
