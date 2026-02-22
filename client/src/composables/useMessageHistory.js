import { ref } from 'vue'
import { fetchMessageHistory } from '../api/chatApi.js'

/** Loads message history for a conversation. */
export function useMessageHistory() {
  const loading = ref(false)

  async function load(conversationKey, setConversationHistory, currentUserId, otherUser) {
    if (!conversationKey || !currentUserId || !otherUser?.id) return
    loading.value = true
    try {
      const list = await fetchMessageHistory(currentUserId, otherUser.id)
      setConversationHistory(conversationKey, list)
    } finally {
      loading.value = false
    }
  }

  return { loading, load }
}
