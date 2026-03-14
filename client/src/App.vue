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

function goBack() {
  selectUser(null)
}
</script>

<template>
  <div class="app">
    <AppHeader :current-user="currentUser" :connection-error="connectionError" />

    <JoinForm v-if="!isJoined" @join="onJoin" />

    <div v-else class="layout" :class="{ 'has-selected-user': selectedUser }">
      <div class="sidebar-wrapper">
        <ChatSidebar
          :conversations="conversations"
          :users="users"
          :current-user="currentUser"
          :selected-user-id="selectedUserId"
          :is-online="isOnline"
          @select-user="selectUser"
        />
      </div>
      <div class="main-wrapper">
        <ChatMain
          :current-user="currentUser"
          :selected-user="selectedUser"
          :connection-ref="connectionRef"
          :current-conversation-messages="currentConversationMessages"
          :set-conversation-history="setConversationHistory"
          :conversation-key="activeConversationKey"
          :on-sent-message="onSentMessage"
          :typing-from-user-id="typingFromUserId"
          @back="goBack"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.layout {
  display: grid;
  grid-template-columns: 320px 1fr;
  gap: 1.5rem;
  flex: 1;
  min-height: 70vh;
}

.sidebar-wrapper {
  display: flex;
  flex-direction: column;
}

.main-wrapper {
  display: flex;
  flex-direction: column;
}

/* Mobile Responsiveness */
@media (max-width: 767px) {
  .layout {
    grid-template-columns: 1fr;
    gap: 0;
  }

  .layout.has-selected-user .sidebar-wrapper {
    display: none;
  }

  .layout:not(.has-selected-user) .main-wrapper {
    display: none;
  }
}
</style>
