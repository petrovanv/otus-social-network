<template>
  <div class="auth-container">
    <div class="auth-card wide">
      <h2>Регистрация</h2>
      <form @submit.prevent="handleRegister">
        <div class="form-group">
          <label>Email *</label>
          <input v-model="form.email" type="email" placeholder="ivan@example.com" required />
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Имя *</label>
            <input v-model="form.first_name" type="text" placeholder="Иван" required />
          </div>
          <div class="form-group">
            <label>Фамилия *</label>
            <input v-model="form.second_name" type="text" placeholder="Иванов" required />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Дата рождения *</label>
            <input v-model="form.birthdate" type="date" required />
          </div>
          <div class="form-group">
            <label>Пол</label>
            <select v-model="form.gender">
              <option value="">Не указан</option>
              <option value="male">Мужской</option>
              <option value="female">Женский</option>
            </select>
          </div>
        </div>
        <div class="form-group">
          <label>Город *</label>
          <input v-model="form.city" type="text" placeholder="Москва" required />
        </div>
        <div class="form-group">
          <label>Интересы / О себе</label>
          <textarea v-model="form.biography" rows="3" placeholder="Расскажите о себе..."></textarea>
        </div>
        <div class="form-group">
          <label>Пароль *</label>
          <input v-model="form.password" type="password" placeholder="Минимум 6 символов" required minlength="6" />
        </div>
        <div v-if="error" class="error-msg">{{ error }}</div>
        <div v-if="successId" class="success-msg">
          Регистрация прошла успешно!<br/>
          Ваш ID: <strong>{{ successId }}</strong><br/>
          <small>Сохраните его — он нужен для входа</small>
        </div>
        <button type="submit" :disabled="loading" class="btn-primary">
          {{ loading ? 'Регистрация...' : 'Зарегистрироваться' }}
        </button>
      </form>
      <p class="auth-link">
        Уже есть аккаунт? <router-link to="/login">Войти</router-link>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { userApi } from '../api'

const router = useRouter()
const form = ref({
  email: '', first_name: '', second_name: '', birthdate: '',
  gender: '', biography: '', city: '', password: ''
})
const error = ref('')
const successId = ref('')
const loading = ref(false)

async function handleRegister() {
  error.value = ''
  successId.value = ''
  loading.value = true
  try {
    const res = await userApi.register(form.value)
    successId.value = res.data.user_id
    setTimeout(() => router.push('/login'), 3000)
  } catch (e) {
    error.value = e.response?.data?.message || 'Ошибка при регистрации'
  } finally {
    loading.value = false
  }
}
</script>
