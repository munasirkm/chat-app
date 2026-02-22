<script setup>
import { computed, watch, onMounted } from 'vue'
import { useMessageHistory } from '../composables/useMessageHistory.js'
import ChatHeader from './chat/ChatHeader.vue'
import MessageBubble from './chat/MessageBubble.vue'
import ChatInput from './chat/ChatInput.vue'

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

const { loading, load } = useMessageHistory()

const showTypingIndicator = computed(() => props.typingFromUserId === props.otherUser?.id)

function loadHistory() {
  load(props.conversationKey, props.setConversationHistory, props.currentUser.userId, props.otherUser)
}

onMounted(loadHistory)

watch(
  () => props.otherUser?.id,
  () => loadHistory(),
  { immediate: false }
)
</script>

<template>
  <div class="chat-panel">
    <ChatHeader :name="otherUser.name" />

    <div v-if="!loading" class="messages">
      <MessageBubble
        v-for="m in messages"
        :key="m.id"
        :message="m"
      />
      <div v-if="showTypingIndicator" class="typing-indicator muted">
        {{ otherUser.name }} is typing...
      </div>
    </div>
    <div v-else class="loading muted">Loading…</div>

    <ChatInput
      :connection="connection"
      :other-user="otherUser"
      :current-user="currentUser"
      :on-sent-message="onSentMessage"
    />
  </div>
</template>

<style scoped>

.chat-panel {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
}

.chat-panel .messages {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.chat-panel .typing-indicator {
  font-size: 0.85rem;
  font-style: italic;
  padding: 0.25rem 0;
}

.chat-panel .loading {
  padding: 1rem;
}
</style>