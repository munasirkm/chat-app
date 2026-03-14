<script setup>
defineProps({
  name: { type: String, required: true }
})
const emit = defineEmits(['back'])
</script>

<template>
  <header class="chat-header">
    <button class="back-button" @click="emit('back')" title="Back" aria-label="Back">
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M19 12H5"></path>
        <polyline points="12 19 5 12 12 5"></polyline>
      </svg>
    </button>
    <h2>{{ name }}</h2>
  </header>
</template>

<style scoped>
.chat-header {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--border);
  background: var(--bg-surface);
  border-radius: 12px 12px 0 0;
  display: flex;
  align-items: center;
  gap: 1rem;
}

.chat-header h2 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--text);
}

.back-button {
  background: transparent;
  border: none;
  color: var(--text-muted);
  padding: 0.5rem;
  margin: -0.5rem 0 -0.5rem -0.5rem;
  border-radius: 50%;
  display: none;
  align-items: center;
  justify-content: center;
}

.back-button:hover {
  background: var(--bg-elevated);
  color: var(--text);
}

@media (max-width: 767px) {
  .back-button {
    display: flex;
  }
}
</style>
