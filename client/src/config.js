/**
 * API and SignalR base URL.
 * With Vite dev proxy, use '' so requests go to the dev server and get proxied to the backend.
 * For production build, set to the backend origin (e.g. window.location.origin if served with API).
 */
export const getBaseUrl = () => {
  if (import.meta.env.DEV) return ''
  return import.meta.env.VITE_API_URL || ''
}
