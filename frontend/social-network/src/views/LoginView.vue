<template>
  <div class="auth-container">
    <div class="auth-card">
      <h2>Вход в социальную сеть</h2>
      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label>Email</label>
          <input v-model="form.email" type="email" placeholder="ivan@example.com" required />
        </div>
        <div class="form-group">
          <label>Пароль</label>
          <input v-model="form.password" type="password" placeholder="Введите пароль" required />
        </div>
        <div v-if="error" class="error-msg">{{ error }}</div>
        <button type="submit" :disabled="loading" class="btn-primary">
          {{ loading ? 'Вход...' : 'Войти' }}
        </button>
      </form>
      <p class="auth-link">
        Нет аккаунта? <router-link to="/register">Зарегистрироваться</router-link>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '../api'

const router = useRouter()
const form = ref({ email: '', password: '' })
const error = ref('')
const loading = ref(false)

async function handleLogin() {
  error.value = ''
  loading.value = true
  try {
    const res = await authApi.login(form.value.email, form.value.password)
    localStorage.setItem('token', res.data.token)
    router.push(`/profile/${res.data.user_id}`)
  } catch (e) {
    error.value = e.response?.data?.message || 'Неверный email или пароль'
  } finally {
    loading.value = false
  }
}
</script>
