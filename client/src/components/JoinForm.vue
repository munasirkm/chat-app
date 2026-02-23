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
  </div>
</template>

<style scoped>
.join-form {
  background: var(--bg-card);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 1.5rem;
  max-width: 360px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
}

.join-form h2 {
  margin: 0 0 1rem 0;
  font-size: 1.1rem;
  color: var(--text);
}

.join-form form {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}

.join-form input {
  flex: 1;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--input-bg);
  color: var(--text);
}

.join-form input:focus {
  outline: none;
  border-color: var(--accent);
  box-shadow: 0 0 0 2px rgba(213, 189, 175, 0.25);
}

.join-form button {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 8px;
  background: var(--accent);
  color: var(--text);
}

.join-form button:hover:not(:disabled) {
  background: var(--accent-hover);
}

.join-form button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
