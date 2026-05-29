<template>
  <div class="app">
    <header class="app-header">
      <div class="header-inner">
        <span class="logo-text">SocialNetwork</span>
        <nav>
          <router-link to="/login">Вход</router-link>
          <router-link to="/register">Регистрация</router-link>
        </nav>
      </div>
    </header>
    <main>
      <RouterView />
    </main>
  </div>
</template>

<script setup>
import { RouterView } from 'vue-router'
</script>

<style>
* { box-sizing: border-box; margin: 0; padding: 0; }

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  background: #f0f2f5;
  color: #1c1e21;
}

.app-header {
  background: #1877f2;
  padding: 0 1rem;
  position: sticky;
  top: 0;
  z-index: 100;
}

.header-inner {
  max-width: 960px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 56px;
}

.logo-text {
  color: white;
  font-size: 1.4rem;
  font-weight: 700;
}

nav a {
  color: white;
  text-decoration: none;
  margin-left: 1.5rem;
  font-size: 0.9rem;
  opacity: 0.9;
}
nav a:hover { opacity: 1; }

main { min-height: calc(100vh - 56px); }

.auth-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: calc(100vh - 56px);
  padding: 2rem 1rem;
}

.auth-card {
  background: white;
  border-radius: 8px;
  padding: 2rem;
  width: 100%;
  max-width: 400px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.1);
}

.auth-card.wide { max-width: 600px; }

.auth-card h2 {
  margin-bottom: 1.5rem;
  font-size: 1.5rem;
  text-align: center;
}

.form-group {
  margin-bottom: 1rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.3rem;
  font-size: 0.85rem;
  font-weight: 600;
  color: #606770;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 0.6rem 0.8rem;
  border: 1px solid #dddfe2;
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.2s;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #1877f2;
}

.btn-primary {
  width: 100%;
  padding: 0.75rem;
  background: #1877f2;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  margin-top: 0.5rem;
  transition: background 0.2s;
}

.btn-primary:hover:not(:disabled) { background: #166fe5; }
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }

.btn-secondary {
  padding: 0.6rem 1.2rem;
  background: #e4e6eb;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  cursor: pointer;
  transition: background 0.2s;
}
.btn-secondary:hover { background: #d8dadf; }

.error-msg {
  background: #fee;
  color: #c00;
  padding: 0.5rem 0.75rem;
  border-radius: 4px;
  font-size: 0.85rem;
  margin-bottom: 0.75rem;
}

.success-msg {
  background: #e8f5e9;
  color: #2e7d32;
  padding: 0.75rem;
  border-radius: 4px;
  font-size: 0.9rem;
  margin-bottom: 0.75rem;
  line-height: 1.6;
}

.auth-link {
  text-align: center;
  margin-top: 1rem;
  font-size: 0.9rem;
  color: #606770;
}

.auth-link a { color: #1877f2; text-decoration: none; }

.profile-container {
  max-width: 700px;
  margin: 2rem auto;
  padding: 0 1rem;
}

.profile-card {
  background: white;
  border-radius: 8px;
  padding: 2rem;
  box-shadow: 0 2px 12px rgba(0,0,0,0.1);
  display: flex;
  gap: 1.5rem;
}

.profile-avatar {
  width: 80px;
  height: 80px;
  background: #1877f2;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.8rem;
  font-weight: 700;
  flex-shrink: 0;
}

.profile-info h2 { margin-bottom: 0.5rem; }

.profile-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-bottom: 1rem;
  color: #606770;
  font-size: 0.9rem;
}

.profile-bio h3 {
  font-size: 1rem;
  margin-bottom: 0.3rem;
  color: #606770;
}

.profile-bio p { line-height: 1.6; }

.profile-id {
  margin-top: 1rem;
  color: #bec3c9;
}

.profile-actions {
  margin-top: 1rem;
  text-align: center;
}

.loading { text-align: center; padding: 3rem; color: #606770; }
</style>
