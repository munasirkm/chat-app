<script setup>
defineProps({
  name: { type: String, required: true }
})
</script>

<template>
  <header class="chat-header">
    <h2>{{ name }}</h2>
  </header>
</template>

<style scoped>
.chat-header {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid #414868;
}

.chat-header h2 {
  margin: 0;
  font-size: 1rem;
}
</style>
