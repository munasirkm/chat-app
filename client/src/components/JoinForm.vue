<script setup>
import { ref } from 'vue'

const userName = ref('')
const emitting = ref(false)

const emit = defineEmits(['join'])

function submit() {
  const name = userName.value?.trim()
  if (!name) return
  emitting.value = true
  emit('join', { userName: name })
  emitting.value = false
}
</script>

<template>
  <div class="join-container">
    <div class="join-form">
      <h2>Join the chat</h2>
      <p class="subtitle muted">Enter your name to connect with others</p>
      <form @submit.prevent="submit">
        <input
          v-model="userName"
          type="text"
          placeholder="Your name"
          maxlength="256"
          autocomplete="username"
          :disabled="emitting"
        />
        <button type="submit" :disabled="emitting || !userName?.trim()">
          Join Chat
        </button>
      </form>
    </div>
  </div>
</template>

<style scoped>
.join-container {
  display: flex;
  justify-content: center;
  align-items: center;
  flex: 1;
  padding: 2rem 1rem;
}

.join-form {
  background: var(--bg-card);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 2.5rem;
  width: 100%;
  max-width: 400px;
  box-shadow: var(--shadow);
  text-align: center;
}

.join-form h2 {
  margin: 0 0 0.5rem 0;
  font-size: 1.5rem;
  color: var(--text);
  font-weight: 700;
}

.subtitle {
  margin: 0 0 1.5rem 0;
  font-size: 0.9rem;
}

.join-form form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.join-form input {
  width: 100%;
  padding: 0.8rem 1rem;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--input-bg);
  color: var(--text);
  font-size: 1rem;
  transition: all 0.2s;
}

.join-form input:focus {
  outline: none;
  border-color: var(--accent);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.2);
}

.join-form button {
  width: 100%;
  padding: 0.8rem 1rem;
  border: none;
  border-radius: 8px;
  background: var(--accent);
  color: white;
  font-weight: 600;
  font-size: 1rem;
  transition: all 0.2s;
}

.join-form button:hover:not(:disabled) {
  background: var(--accent-hover);
  transform: translateY(-1px);
}

.join-form button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}
</style>
