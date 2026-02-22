import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import MessageBubble from './MessageBubble.vue'

describe('MessageBubble', () => {
  it('renders message data', () => {
    const wrapper = mount(MessageBubble, {
      props: {
        message: {
          data: 'Hello',
          sentAt: '2024-01-15T10:30:00Z',
          isOwn: false
        }
      }
    })
    expect(wrapper.find('.bubble').text()).toBe('Hello')
  })

  it('applies "theirs" class when message is not own', () => {
    const wrapper = mount(MessageBubble, {
      props: {
        message: { data: 'Hi', sentAt: null, isOwn: false }
      }
    })
    expect(wrapper.find('.bubble-wrap').classes()).toContain('theirs')
  })

  it('applies "own" class when message is own', () => {
    const wrapper = mount(MessageBubble, {
      props: {
        message: { data: 'Hi', sentAt: null, isOwn: true }
      }
    })
    expect(wrapper.find('.bubble-wrap').classes()).toContain('own')
  })

  it('formats sentAt as locale time string when present', () => {
    const wrapper = mount(MessageBubble, {
      props: {
        message: {
          data: 'Hi',
          sentAt: '2024-01-15T14:30:00.000Z',
          isOwn: false
        }
      }
    })
    const meta = wrapper.find('.meta')
    expect(meta.text().length).toBeGreaterThan(0)
  })
})
