<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { fetchUsers } from './api/chatApi.js'
import { createChatConnection } from './services/signalr.js'
import ChatPanel from './components/ChatPanel.vue'
import JoinForm from './components/JoinForm.vue'

function conversationKey(a, b) {
  return [a, b].sort((x, y) => x - y).join('-')
}

const currentUser = ref(null)
const users = ref([])
const selectedUserId = ref(null)
const connectionRef = ref(null)
const connectionError = ref('')
const conversationMessages = ref({})

const isJoined = computed(() => !!currentUser.value?.userId)

const selectedUser = computed(() =>
  users.value.find(u => u.id === selectedUserId.value) || null
)

const currentConversationMessages = computed(() => {
  if (!currentUser.value || !selectedUser.value || selectedUser.value.id === currentUser.value.userId)
    return []
  const key = conversationKey(currentUser.value.userId, selectedUser.value.id)
  return conversationMessages.value[key] || []
})

function addMessageToConversation(msg) {
  if (msg.type !== 'chat' || !msg.senderId || !msg.receiverId) return
  const a = parseInt(msg.senderId, 10)
  const b = parseInt(msg.receiverId, 10)
  const key = conversationKey(a, b)
  if (!conversationMessages.value[key]) conversationMessages.value[key] = []
  conversationMessages.value[key].push({
    id: msg.id || Date.now() + Math.random(),
    senderId: a,
    receiverId: b,
    data: msg.data || '',
    sentAt: msg.sentAt || new Date().toISOString(),
    isOwn: a === currentUser.value?.userId
  })
}

function setConversationHistory(key, list) {
  const myId = currentUser.value?.userId
  conversationMessages.value[key] = (list || []).map(m => ({
    ...m,
    isOwn: m.senderId === myId
  }))
}

function onSentMessage(msg) {
  addMessageToConversation(msg)
}

async function loadUsers() {
  try {
    users.value = await fetchUsers()
  } catch (e) {
    console.error('Load users failed', e)
  }
}

async function onJoin({ userName }) {
  connectionError.value = ''
  const conn = createChatConnection()
  connectionRef.value = conn

  conn.onReceiveMessage((msg) => {
    if (msg.type === 'error') {
      connectionError.value = msg.data || 'Error'
      return
    }
    if (msg.type === 'chat') addMessageToConversation(msg)
  })

  try {
    await conn.start()
    const result = await conn.join(userName)
    if (!result?.success) {
      connectionError.value = 'Join failed'
      return
    }
    currentUser.value = { userId: result.userId, userName: userName }
    await loadUsers()
    if (users.value.length > 1 && !selectedUserId.value) {
      selectedUserId.value = users.value.find(u => u.id !== result.userId)?.id ?? users.value[0]?.id
    }
  } catch (err) {
    connectionError.value = err.message || 'Connection failed'
    console.error(err)
  }
}

function selectUser(id) {
  selectedUserId.value = id
}

onMounted(() => {
  if (isJoined.value) loadUsers()
})

onUnmounted(() => {
  connectionRef.value?.connection?.stop()
})
</script>

<template>
  <div class="app">
    <header class="header">
      <h1>RealChat</h1>
      <p v-if="currentUser" class="muted">
        Signed in as <strong>{{ currentUser.userName }}</strong> (ID: {{ currentUser.userId }})
      </p>
      <p v-if="connectionError" class="error">{{ connectionError }}</p>
    </header>

    <JoinForm v-if="!isJoined" @join="onJoin" />

    <template v-else>
      <div class="layout">
        <aside class="sidebar">
          <h2>Users</h2>
          <ul class="user-list">
            <li
              v-for="u in users"
              :key="u.id"
              :class="{ active: selectedUserId === u.id }"
              @click="selectUser(u.id)"
            >
              {{ u.name }}
              <span v-if="u.id === currentUser.userId" class="muted">(you)</span>
            </li>
          </ul>
          <p v-if="!users.length" class="muted">No other users yet. Open another browser tab and join with a different name.</p>
        </aside>
        <main class="main">
          <ChatPanel
            v-if="selectedUser && selectedUser.id !== currentUser.userId"
            :current-user="currentUser"
            :other-user="selectedUser"
            :connection="connectionRef"
            :messages="currentConversationMessages"
            :set-conversation-history="setConversationHistory"
            :conversation-key="selectedUser && currentUser ? conversationKey(currentUser.userId, selectedUser.id) : ''"
            :on-sent-message="onSentMessage"
          />
          <div v-else class="placeholder">
            <p class="muted">Select a user to start chatting.</p>
          </div>
        </main>
      </div>
    </template>
  </div>
</template>

<style scoped>
.app {
  min-height: 100vh;
}

.header {
  margin-bottom: 1.5rem;
}

.header h1 {
  margin: 0 0 0.25rem 0;
  font-size: 1.5rem;
}

.layout {
  display: grid;
  grid-template-columns: 220px 1fr;
  gap: 1rem;
  min-height: 60vh;
}

.sidebar {
  background: #24283b;
  border-radius: 8px;
  padding: 1rem;
}

.sidebar h2 {
  margin: 0 0 0.75rem 0;
  font-size: 0.95rem;
  font-weight: 600;
}

.user-list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.user-list li {
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  cursor: pointer;
}

.user-list li:hover {
  background: #343b58;
}

.user-list li.active {
  background: #414868;
}

.main {
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
