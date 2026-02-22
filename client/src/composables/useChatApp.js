import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { fetchUsers, fetchConversations } from '../api/chatApi.js'
import { createChatConnection } from '../services/signalr.js'

export function useChatApp(conversationKey) {
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

  return {
    currentUser,
    users,
    conversations,
    selectedUserId,
    connectionRef,
    connectionError,
    conversationMessages,
    onlineUserIds,
    typingFromUserId,
    isJoined,
    selectedUser,
    currentConversationMessages,
    setConversationHistory,
    onSentMessage,
    onJoin,
    selectUser,
    isOnline
  }
}

