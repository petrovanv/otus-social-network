import axios from 'axios'

const api = axios.create({
  baseURL: 'https://localhost:7162',
  headers: { 'Content-Type': 'application/json' }
})

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const authApi = {
  login: (email, password) => api.post('/login', { email, password })
}

export const userApi = {
  register: (data) => api.post('/user/register', data),
  getById: (id) => api.get(`/user/get/${id}`)
}

export default api
