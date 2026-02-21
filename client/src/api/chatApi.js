import { getBaseUrl } from '../config.js'

const base = getBaseUrl()

export async function fetchUsers() {
  const res = await fetch(`${base}/api/users`)
  if (!res.ok) throw new Error('Failed to fetch users')
  return res.json()
}

export async function fetchConversations(userId) {
  const res = await fetch(`${base}/api/users/${userId}/conversations`)
  if (!res.ok) throw new Error('Failed to fetch conversations')
  return res.json()
}

export async function fetchMessageHistory(userId, otherUserId) {
  const res = await fetch(`${base}/api/conversations/${userId}/with/${otherUserId}/messages`)
  if (!res.ok) throw new Error('Failed to fetch messages')
  return res.json()
}
