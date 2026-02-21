import * as signalR from '@microsoft/signalr'
import { getBaseUrl } from '../config.js'

/**
 * SignalR chat connection: connect, join as user, send/receive messages.
 */
export function createChatConnection() {
  const base = getBaseUrl()
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${base}/hubs/chat`, { withCredentials: true })
    .withAutomaticReconnect()
    .build()

  return {
    connection,

    async start() {
      await connection.start()
    },

    async join(userName) {
      return connection.invoke('Join', userName)
    },

    async sendMessage(receiverId, data) {
      await connection.invoke('SendMessage', {
        type: 'chat',
        senderId: null,
        receiverId: String(receiverId),
        data
      })
    },

    async setTyping(receiverId, isTyping = true) {
      await connection.invoke('SetTyping', {
        type: 'typing',
        senderId: null,
        receiverId: String(receiverId),
        data: isTyping ? 'typing' : null
      })
    },

    onReceiveMessage(handler) {
      connection.on('ReceiveMessage', handler)
    },

    offReceiveMessage() {
      connection.off('ReceiveMessage')
    }
  }
}
