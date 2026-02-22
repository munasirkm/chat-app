import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { useTypingIndicator } from './useTypingIndicator.js'

describe('useTypingIndicator', () => {
  let sendTypingFn

  beforeEach(() => {
    vi.useFakeTimers()
    sendTypingFn = vi.fn()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('calls sendTypingFn(true) when onTyping is invoked', () => {
    const { onTyping } = useTypingIndicator(sendTypingFn)
    onTyping()
    expect(sendTypingFn).toHaveBeenCalledWith(true)
  })

  it('calls sendTypingFn(false) after debounce delay when onTyping is invoked', () => {
    const { onTyping } = useTypingIndicator(sendTypingFn)
    onTyping()
    sendTypingFn.mockClear()
    vi.advanceTimersByTime(800)
    expect(sendTypingFn).toHaveBeenCalledWith(false)
  })

  it('stopAndCleanup calls sendTypingFn(false) and clears pending timer', () => {
    const { onTyping, stopAndCleanup } = useTypingIndicator(sendTypingFn)
    onTyping()
    sendTypingFn.mockClear()
    stopAndCleanup()
    expect(sendTypingFn).toHaveBeenCalledWith(false)
    vi.advanceTimersByTime(1000)
    expect(sendTypingFn).toHaveBeenCalledTimes(1)
  })
})
