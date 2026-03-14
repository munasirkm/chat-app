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

const emit = defineEmits(['back'])
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
      @back="emit('back')"
    />
    <div v-else class="placeholder">
      <div class="placeholder-content">
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="var(--border)" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
        </svg>
        <p class="muted">Select a conversation or user to start chatting.</p>
      </div>
    </div>
  </main>
</template>

<style scoped>
.chat-main {
  background: var(--bg-card);
  border: 1px solid var(--border);
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 400px;
  box-shadow: var(--shadow);
  overflow: hidden;
}

.placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  flex: 1;
  padding: 2rem;
  background: var(--bg-surface);
}

.placeholder-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  text-align: center;
}
</style>
