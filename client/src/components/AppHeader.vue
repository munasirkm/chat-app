<script setup>
defineProps({
  currentUser: { type: Object, default: null },
  connectionError: { type: String, default: '' }
})
</script>

<template>
  <header class="app-header">
    <h1>RealChat</h1>
    <p v-if="currentUser" class="muted">
      Signed in as <strong>{{ currentUser.userName }}</strong> (ID: {{ currentUser.userId }})
    </p>
    <p v-if="connectionError" class="error">{{ connectionError }}</p>
  </header>
</template>

<style scoped>
.app-header {
  margin-bottom: 1.5rem;

}

.app-header h1 {
  margin: 0.25rem;
  font-size: 1.5rem;
  
}
</style>
