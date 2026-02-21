<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { fetchUsers, fetchConversations } from './api/chatApi.js'
import { createChatConnection } from './services/signalr.js'
import ChatPanel from './components/ChatPanel.vue'
import JoinForm from './components/JoinForm.vue'

function conversationKey(a, b) {
  return [a, b].sort((x, y) => x - y).join('-')
}

const currentUser = ref(null)
const users = ref([])
const conversations = ref([])
const selectedUserId = ref(null)
const connectionRef = ref(null)
const connectionError = ref('')
const conversationMessages = ref({})
const onlineUserIds = ref(new Set())
const typingFromUserId = ref(null)
let typingClearTimer = null

const isJoined = computed(() => !!currentUser.value?.userId)

const selectedUser = computed(() => {
  const fromConv = conversations.value.find(c => c.otherUserId === selectedUserId.value)
  if (fromConv) return { id: fromConv.otherUserId, name: fromConv.otherUserName }
  return users.value.find(u => u.id === selectedUserId.value) || null
})

const currentConversationMessages = computed(() => {
  if (!currentUser.value || !selectedUser.value || selectedUser.value.id === currentUser.value.userId)
    return []
  const key = conversationKey(currentUser.value.userId, selectedUser.value.id)
  return conversationMessages.value[key] || []
})

function addMessageToConversation(msg) {
  if (msg.type === 'typing') {
    const senderId = parseInt(msg.senderId, 10)
    if (selectedUserId.value === senderId) {
      typingFromUserId.value = senderId
      if (typingClearTimer) clearTimeout(typingClearTimer)
      typingClearTimer = setTimeout(() => { typingFromUserId.value = null }, 3000)
    }
    return
  }
  if (msg.type !== 'chat' || !msg.senderId || !msg.receiverId) return
  typingFromUserId.value = null
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
  loadConversations()
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
  loadConversations()
}

async function loadUsers() {
  try {
    users.value = await fetchUsers()
  } catch (e) {
    console.error('Load users failed', e)
  }
}

async function loadConversations() {
  if (!currentUser.value?.userId) return
  try {
    conversations.value = await fetchConversations(currentUser.value.userId)
  } catch (e) {
    console.error('Load conversations failed', e)
  }
}

function isOnline(userId) {
  return onlineUserIds.value.has(userId)
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
    if (msg.type === 'typing') {
      addMessageToConversation(msg)
      return
    }
    if (msg.type === 'chat') addMessageToConversation(msg)
  })

  conn.onUserOnline((userId) => {
    onlineUserIds.value = new Set(onlineUserIds.value).add(userId)
  })
  conn.onUserOffline((userId) => {
    const next = new Set(onlineUserIds.value)
    next.delete(userId)
    onlineUserIds.value = next
  })

  try {
    await conn.start()
    const result = await conn.join(userName)
    if (!result?.success) {
      connectionError.value = 'Join failed'
      return
    }
    currentUser.value = { userId: result.userId, userName: userName }
    const onlineIds = await conn.getOnlineUserIds()
    onlineUserIds.value = new Set(Array.isArray(onlineIds) ? onlineIds : [])
    await loadUsers()
    await loadConversations()
    if (conversations.value.length > 0 && !selectedUserId.value) {
      selectedUserId.value = conversations.value[0].otherUserId
    } else if (users.value.length > 1 && !selectedUserId.value) {
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

watch(selectedUserId, () => {
  typingFromUserId.value = null
})

onMounted(() => {
  if (isJoined.value) {
    loadUsers()
    loadConversations()
  }
})

onUnmounted(() => {
  if (typingClearTimer) clearTimeout(typingClearTimer)
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
          <section class="sidebar-section">
            <h2>Recent chats</h2>
            <ul class="user-list">
              <li
                v-for="c in conversations"
                :key="c.otherUserId"
                :class="{ active: selectedUserId === c.otherUserId }"
                @click="selectUser(c.otherUserId)"
              >
                <span class="name-row">
                  <span class="status-dot" :class="{ online: isOnline(c.otherUserId) }" />
                  {{ c.otherUserName }}
                </span>
                <span v-if="c.lastMessagePreview" class="preview muted">{{ c.lastMessagePreview }}</span>
                <span class="time muted">{{ c.lastMessageAt ? new Date(c.lastMessageAt).toLocaleTimeString() : '' }}</span>
              </li>
            </ul>
            <p v-if="!conversations.length" class="muted">No recent chats.</p>
          </section>
          <section class="sidebar-section">
            <h2>All users</h2>
            <ul class="user-list">
              <li
                v-for="u in users"
                :key="u.id"
                :class="{ active: selectedUserId === u.id }"
                @click="selectUser(u.id)"
              >
                <span class="name-row">
                  <span class="status-dot" :class="{ online: isOnline(u.id) }" />
                  {{ u.name }}
                  <span v-if="u.id === currentUser.userId" class="muted">(you)</span>
                </span>
              </li>
            </ul>
            <p v-if="!users.length" class="muted">No other users yet.</p>
          </section>
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
            :typing-from-user-id="typingFromUserId"
          />
          <div v-else class="placeholder">
            <p class="muted">Select a conversation or user to start chatting.</p>
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
  grid-template-columns: 260px 1fr;
  gap: 1rem;
  min-height: 60vh;
}

.sidebar {
  background: #24283b;
  border-radius: 8px;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.sidebar-section h2 {
  margin: 0 0 0.5rem 0;
  font-size: 0.9rem;
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
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}

.user-list li:hover {
  background: #343b58;
}

.user-list li.active {
  background: #414868;
}

.name-row {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #565f89;
  flex-shrink: 0;
}

.status-dot.online {
  background: #9ece6a;
}

.preview {
  font-size: 0.8rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
}

.time {
  font-size: 0.75rem;
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
