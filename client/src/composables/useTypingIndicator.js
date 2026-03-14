/** Manages typing indicator: debounced send, cleanup on stop. */
const TYPING_DEBOUNCE_MS = 800

export function useTypingIndicator(sendTypingFn) {
  let timer = null

  function onTyping() {

    if (timer) clearTimeout(timer)
    timer = setTimeout(() => sendTypingFn(true), TYPING_DEBOUNCE_MS)
  }

  function stopAndCleanup() {
    if (timer) {
      clearTimeout(timer)
      timer = null
    }
    sendTypingFn(false)
  }

  return { onTyping, stopAndCleanup }
}
