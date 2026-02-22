<script setup>
import ChatPanel from './ChatPanel.vue'

defineProps({
  currentUser: { type: Object, required: true },
  selectedUser: { type: Object, default: null },
  connectionRef: { type: Object, default: null },
  currentConversationMessages: { type: Array, default: () => [] },
  setConversationHistory: { type: Function, required: true },
  conversationKey: { type: String, default: '' },
  onSentMessage: { type: Function, default: null },
  typingFromUserId: { type: Number, default: null }
})
</script>

<template>
  <main class="chat-main">
    <ChatPanel
      v-if="selectedUser && selectedUser.id !== currentUser.userId"
      :current-user="currentUser"
      :other-user="selectedUser"
      :connection="connectionRef"
      :messages="currentConversationMessages"
      :set-conversation-history="setConversationHistory"
      :conversation-key="conversationKey"
      :on-sent-message="onSentMessage"
      :typing-from-user-id="typingFromUserId"
    />
    <div v-else class="placeholder">
      <p class="muted">Select a conversation or user to start chatting.</p>
    </div>
  </main>
</template>

<style scoped>
.chat-main {
  background: #24283b;
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  min-height: 400px;
}

.placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  flex: 1;
  padding: 2rem;
}
</style>
