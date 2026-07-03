import http from 'k6/http';

export let options = { vus: 1, iterations: 3 };

const TOKEN = __ENV.TOKEN;
const FN = ['Ал', 'Ан', 'Ив'];
const LN = ['Ив', 'Смир', 'Куз'];

export default function () {
  const fn = encodeURIComponent(FN[Math.floor(Math.random() * FN.length)]);
  const ln = encodeURIComponent(LN[Math.floor(Math.random() * LN.length)]);

  const res = http.get(
    `http://localhost:5282/user/search?first_name=${fn}&last_name=${ln}`,
    { headers: { Authorization: `Bearer ${TOKEN}` } }
  );
  console.log(`status=${res.status} fn=${fn} body_len=${res.body ? res.body.length : 0}`);
}
