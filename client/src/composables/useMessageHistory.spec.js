import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useMessageHistory } from './useMessageHistory.js'

import { fetchMessageHistory } from '../api/chatApi.js'

vi.mock('../api/chatApi.js', () => ({
  fetchMessageHistory: vi.fn()
}))

describe('useMessageHistory', () => {
  beforeEach(() => {
    vi.mocked(fetchMessageHistory).mockReset()
  })

  it('exposes loading ref initially false', () => {
    const { loading } = useMessageHistory()
    expect(loading.value).toBe(false)
  })

  it('load does nothing when conversationKey is missing', async () => {
    const setConversationHistory = vi.fn()
    const { load, loading } = useMessageHistory()
    await load('', setConversationHistory, 1, { id: 2 })
    expect(fetchMessageHistory).not.toHaveBeenCalled()
    expect(setConversationHistory).not.toHaveBeenCalled()
    expect(loading.value).toBe(false)
  })

  it('load does nothing when otherUser has no id', async () => {
    const setConversationHistory = vi.fn()
    const { load } = useMessageHistory()
    await load('1-2', setConversationHistory, 1, {})
    expect(fetchMessageHistory).not.toHaveBeenCalled()
  })

  it('load fetches history and calls setConversationHistory', async () => {
    const setConversationHistory = vi.fn()
    const mockList = [{ id: 1, senderId: 1, receiverId: 2, data: 'Hi', sentAt: new Date().toISOString() }]
    vi.mocked(fetchMessageHistory).mockResolvedValue(mockList)

    const { load, loading } = useMessageHistory()
    const loadPromise = load('1-2', setConversationHistory, 1, { id: 2 })

    expect(loading.value).toBe(true)
    await loadPromise
    expect(loading.value).toBe(false)
    expect(fetchMessageHistory).toHaveBeenCalledWith(1, 2)
    expect(setConversationHistory).toHaveBeenCalledWith('1-2', mockList)
  })
})
