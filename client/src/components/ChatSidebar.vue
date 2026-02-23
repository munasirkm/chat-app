<script setup>
defineProps({
  conversations: { type: Array, default: () => [] },
  users: { type: Array, default: () => [] },
  currentUser: { type: Object, default: null },
  selectedUserId: { type: Number, default: null },
  isOnline: { type: Function, required: true }
})

const emit = defineEmits(['select-user'])
</script>

<template>
  <aside class="chat-sidebar">
    <section class="sidebar-section">
      <h2>Recent chats</h2>
      <ul class="user-list">
        <li
          v-for="c in conversations"
          :key="c.otherUserId"
          :class="{ active: selectedUserId === c.otherUserId }"
          @click="emit('select-user', c.otherUserId)"
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
          @click="emit('select-user', u.id)"
        >
          <span class="name-row">
            <span class="status-dot" :class="{ online: isOnline(u.id) }" />
            {{ u.name }}
            <span v-if="u.id === currentUser?.userId" class="muted">(you)</span>
          </span>
        </li>
      </ul>
      <p v-if="!users.length" class="muted">No other users yet.</p>
    </section>
  </aside>
</template>

<style scoped>
.chat-sidebar {
  background: var(--bg-card);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
}

.sidebar-section h2 {
  margin: 0 0 0.5rem 0;
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--text);
}

.user-list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.user-list li {
  padding: 0.5rem 0.75rem;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}

.user-list li:hover {
  background: var(--bg-surface);
}

.user-list li.active {
  background: var(--accent);
  color: var(--text);
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
  background: var(--border);
  flex-shrink: 0;
}

.status-dot.online {
  background: var(--online);
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
</style>
