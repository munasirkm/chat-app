<script setup>
import { computed } from 'vue'
import AppHeader from './components/AppHeader.vue'
import JoinForm from './components/JoinForm.vue'
import ChatSidebar from './components/ChatSidebar.vue'
import ChatMain from './components/ChatMain.vue'
import { useChatApp } from './composables/useChatApp.js'

function conversationKey(a, b) {
  return [a, b].sort((x, y) => x - y).join('-')
}

const {
  currentUser,
  users,
  conversations,
  selectedUserId,
  connectionRef,
  connectionError,
  typingFromUserId,
  isJoined,
  selectedUser,
  currentConversationMessages,
  setConversationHistory,
  onSentMessage,
  onJoin,
  selectUser,
  isOnline
} = useChatApp(conversationKey)

const activeConversationKey = computed(() => {
  const user = currentUser.value
  const other = selectedUser.value
  return user && other ? conversationKey(user.userId, other.id) : ''
})
</script>

<template>
  <div class="app">
    <AppHeader :current-user="currentUser" :connection-error="connectionError" />

    <JoinForm v-if="!isJoined" @join="onJoin" />

    <div v-else class="layout">
      <ChatSidebar
        :conversations="conversations"
        :users="users"
        :current-user="currentUser"
        :selected-user-id="selectedUserId"
        :is-online="isOnline"
        @select-user="selectUser"
      />
      <ChatMain
        :current-user="currentUser"
        :selected-user="selectedUser"
        :connection-ref="connectionRef"
        :current-conversation-messages="currentConversationMessages"
        :set-conversation-history="setConversationHistory"
        :conversation-key="activeConversationKey"
        :on-sent-message="onSentMessage"
        :typing-from-user-id="typingFromUserId"
      />
    </div>
  </div>
</template>

<style scoped>
.app {
  min-height: 100vh;
}

.layout {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 1rem;
  min-height: 60vh;
}
</style>
