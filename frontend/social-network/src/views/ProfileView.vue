<template>
  <div class="profile-container">
    <div v-if="loading" class="loading">Загрузка профиля...</div>
    <div v-else-if="error" class="error-msg">{{ error }}</div>
    <div v-else-if="user" class="profile-card">
      <div class="profile-avatar">{{ initials }}</div>
      <div class="profile-info">
        <h2>{{ user.first_name }} {{ user.second_name }}</h2>
        <div class="profile-meta">
          <span v-if="user.email">✉️ {{ user.email }}</span>
          <span v-if="user.birthdate">📅 {{ formatDate(user.birthdate) }}</span>
          <span v-if="user.gender">{{ user.gender === 'male' ? '👨 Мужской' : '👩 Женский' }}</span>
          <span v-if="user.city">📍 {{ user.city }}</span>
        </div>
        <div v-if="user.biography" class="profile-bio">
          <h3>Интересы / О себе</h3>
          <p>{{ user.biography }}</p>
        </div>
        <div class="profile-id">
          <small>ID: {{ user.id }}</small>
        </div>
      </div>
    </div>
    <div class="profile-actions">
      <button @click="router.push('/login')" class="btn-secondary">Войти как другой пользователь</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { userApi } from '../api'

const route = useRoute()
const router = useRouter()
const user = ref(null)
const loading = ref(true)
const error = ref('')

const initials = computed(() => {
  if (!user.value) return '?'
  return (user.value.first_name[0] || '') + (user.value.second_name[0] || '')
})

function formatDate(dateStr) {
  return new Date(dateStr).toLocaleDateString('ru-RU')
}

onMounted(async () => {
  try {
    const res = await userApi.getById(route.params.id)
    user.value = res.data
  } catch (e) {
    if (e.response?.status === 401) {
      router.push('/login')
    } else {
      error.value = e.response?.data?.message || 'Пользователь не найден'
    }
  } finally {
    loading.value = false
  }
})
</script>
