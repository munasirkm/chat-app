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
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  box-shadow: var(--shadow);
  height: 100%;
  overflow-y: auto;
}

.sidebar-section h2 {
  margin: 0 0 0.75rem 0;
  font-size: 0.85rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  font-weight: 700;
  color: var(--text-muted);
}

.user-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.user-list li {
  padding: 0.75rem 1rem;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.user-list li:hover {
  background: var(--bg-surface);
}

.user-list li.active {
  background: var(--bg-elevated);
  border-color: var(--border);
}

.name-row {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
  color: var(--text);
}

.status-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--border);
  flex-shrink: 0;
  box-shadow: 0 0 0 2px var(--bg-card);
}

.user-list li:hover .status-dot {
  box-shadow: 0 0 0 2px var(--bg-surface);
}

.user-list li.active .status-dot {
  box-shadow: 0 0 0 2px var(--bg-elevated);
}

.status-dot.online {
  background: var(--online);
}

.preview {
  font-size: 0.85rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
  color: var(--text-muted);
}

.time {
  font-size: 0.75rem;
  color: var(--text-muted);
  font-weight: 500;
}
</style>
