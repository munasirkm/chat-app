<script setup>
defineProps({
  message: { type: Object, required: true }
})
</script>

<template>
  <div :class="['bubble-wrap', message.isOwn ? 'own' : 'theirs']">
    <div class="bubble">{{ message.data }}</div>
    <div class="meta muted">{{ message.sentAt ? new Date(message.sentAt).toLocaleTimeString() : '' }}</div>
  </div>
</template>

<style scoped>
.bubble-wrap {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.bubble-wrap.own {
  align-items: flex-end;
}

.bubble {
  max-width: 75%;
  padding: 0.5rem 0.75rem;
  border-radius: 12px;
  word-break: break-word;
}

.bubble-wrap.theirs .bubble {
  background: #414868;
  border-bottom-left-radius: 4px;
}

.bubble-wrap.own .bubble {
  background: #7aa2f7;
  color: #1a1b26;
  border-bottom-right-radius: 4px;
}

.meta {
  font-size: 0.75rem;
  margin-top: 0.25rem;
}
</style>
