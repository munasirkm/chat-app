<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { fetchMessageHistory } from '../api/chatApi.js'

const props = defineProps({
  currentUser: { type: Object, required: true },
  otherUser: { type: Object, required: true },
  connection: { type: Object, default: null },
  messages: { type: Array, default: () => [] },
  setConversationHistory: { type: Function, required: true },
  conversationKey: { type: String, default: '' },
  onSentMessage: { type: Function, default: null },
  typingFromUserId: { type: Number, default: null }
})

const inputText = ref('')
const loading = ref(false)
let typingDebounceTimer = null

const showTypingIndicator = computed(() =>
  props.typingFromUserId === props.otherUser?.id
)

async function loadHistory() {
  if (!props.conversationKey) return
  loading.value = true
  try {
    const list = await fetchMessageHistory(props.currentUser.userId, props.otherUser.id)
    props.setConversationHistory(props.conversationKey, list)
  } finally {
    loading.value = false
  }
}

function sendTyping(isTyping) {
  if (!props.connection?.connection) return
  props.connection.setTyping(props.otherUser.id, isTyping)
}

function onInput() {
  sendTyping(true)
  if (typingDebounceTimer) clearTimeout(typingDebounceTimer)
  typingDebounceTimer = setTimeout(() => sendTyping(false), 800)
}

function send() {
  const text = inputText.value?.trim()
  if (!text || !props.connection?.connection) return
  sendTyping(false)
  if (typingDebounceTimer) {
    clearTimeout(typingDebounceTimer)
    typingDebounceTimer = null
  }
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

onMounted(() => {
  loadHistory()
})

onUnmounted(() => {
  if (typingDebounceTimer) clearTimeout(typingDebounceTimer)
  sendTyping(false)
})

watch(
  () => props.otherUser?.id,
  () => loadHistory(),
  { immediate: false }
)
</script>

<template>
  <div class="chat-panel">
    <div class="chat-header">
      <h2>{{ otherUser.name }}</h2>
    </div>

    <div class="messages" v-if="!loading">
      <div
        v-for="m in messages"
        :key="m.id"
        :class="['message', m.isOwn ? 'own' : 'theirs']"
      >
        <div class="bubble">{{ m.data }}</div>
        <div class="meta muted">{{ m.sentAt ? new Date(m.sentAt).toLocaleTimeString() : '' }}</div>
      </div>
      <div v-if="showTypingIndicator" class="typing-indicator muted">
        {{ otherUser.name }} is typing...
      </div>
    </div>
    <div v-else class="muted">Loading…</div>

    <form class="input-row" @submit.prevent="send">
      <input
        v-model="inputText"
        type="text"
        placeholder="Type a message..."
        maxlength="4096"
        @input="onInput"
      />
      <button type="submit" :disabled="!inputText?.trim()">Send</button>
    </form>
  </div>
</template>

<style scoped>
.chat-panel {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
}

.chat-header {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid #414868;
}

.chat-header h2 {
  margin: 0;
  font-size: 1rem;
}

.messages {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.message {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.message.own {
  align-items: flex-end;
}

.bubble {
  max-width: 75%;
  padding: 0.5rem 0.75rem;
  border-radius: 12px;
  word-break: break-word;
}

.message.theirs .bubble {
  background: #414868;
  border-bottom-left-radius: 4px;
}

.message.own .bubble {
  background: #7aa2f7;
  color: #1a1b26;
  border-bottom-right-radius: 4px;
}

.meta {
  font-size: 0.75rem;
  margin-top: 0.25rem;
}

.typing-indicator {
  font-size: 0.85rem;
  font-style: italic;
  padding: 0.25rem 0;
}

.input-row {
  display: flex;
  gap: 0.5rem;
  padding: 1rem;
  border-top: 1px solid #414868;
}

.input-row input {
  flex: 1;
  padding: 0.5rem 0.75rem;
  border: 1px solid #414868;
  border-radius: 6px;
  background: #1a1b26;
  color: #c0caf5;
}

.input-row input:focus {
  outline: none;
  border-color: #7aa2f7;
}

.input-row button {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 6px;
  background: #7aa2f7;
  color: #1a1b26;
}

.input-row button:hover:not(:disabled) {
  background: #89b4fa;
}

.input-row button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
