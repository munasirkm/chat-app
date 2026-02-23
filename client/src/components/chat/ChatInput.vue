<script setup>
import { ref, onUnmounted } from 'vue'
import { useTypingIndicator } from '../../composables/useTypingIndicator.js'

const props = defineProps({
  connection: { type: Object, default: null },
  otherUser: { type: Object, required: true },
  currentUser: { type: Object, required: true },
  onSentMessage: { type: Function, default: null }
})

const inputText = ref('')

function sendTyping(isTyping) {
  props.connection?.setTyping?.(props.otherUser.id, isTyping)
}

const { onTyping, stopAndCleanup } = useTypingIndicator(sendTyping)

function send() {
  const text = inputText.value?.trim()
  if (!text || !props.connection?.sendMessage) return

  stopAndCleanup()
  props.connection.sendMessage(props.otherUser.id, text)

  if (props.onSentMessage) {
    props.onSentMessage({
      type: 'chat',
      senderId: String(props.currentUser.userId),
      receiverId: String(props.otherUser.id),
      data: text
    })
  }
  inputText.value = ''
}

onUnmounted(stopAndCleanup)
</script>

<template>
  <form class="chat-input" @submit.prevent="send">
    <input
      v-model="inputText"
      type="text"
      placeholder="Type a message..."
      maxlength="4096"
      @input="onTyping"
    />
    <button type="submit" :disabled="!inputText?.trim()">Send</button>
  </form>
</template>

<style scoped>
.chat-input {
  display: flex;
  gap: 0.5rem;
  padding: 1rem;
  border-top: 1px solid var(--border);
  background: var(--bg-surface);
  border-radius: 0 0 10px 10px;
}

.chat-input input {
  flex: 1;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--input-bg);
  color: var(--text);
}

.chat-input input:focus {
  outline: none;
  border-color: var(--accent);
  box-shadow: 0 0 0 2px rgba(213, 189, 175, 0.25);
}

.chat-input button {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 8px;
  background: var(--accent);
  color: var(--text);
}

.chat-input button:hover:not(:disabled) {
  background: var(--accent-hover);
}

.chat-input button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
