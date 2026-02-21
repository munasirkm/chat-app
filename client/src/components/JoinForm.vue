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
  <div class="join-form">
    <h2>Join the chat</h2>
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
        Join
      </button>
    </form>
    <p class="muted">Make sure the backend is running on port 5086 (or update Vite proxy).</p>
  </div>
</template>

<style scoped>
.join-form {
  background: #24283b;
  border-radius: 8px;
  padding: 1.5rem;
  max-width: 360px;
}

.join-form h2 {
  margin: 0 0 1rem 0;
  font-size: 1.1rem;
}

.join-form form {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}

.join-form input {
  flex: 1;
  padding: 0.5rem 0.75rem;
  border: 1px solid #414868;
  border-radius: 6px;
  background: #1a1b26;
  color: #c0caf5;
}

.join-form input:focus {
  outline: none;
  border-color: #7aa2f7;
}

.join-form button {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 6px;
  background: #7aa2f7;
  color: #1a1b26;
}

.join-form button:hover:not(:disabled) {
  background: #89b4fa;
}

.join-form button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
