import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import AppHeader from './AppHeader.vue'

describe('AppHeader', () => {
  it('renders RealChat title', () => {
    const wrapper = mount(AppHeader)
    expect(wrapper.find('h1').text()).toBe('RealChat')
  })

  it('shows signed-in user when currentUser is provided', () => {
    const wrapper = mount(AppHeader, {
      props: {
        currentUser: { userId: 1, userName: 'Alice' }
      }
    })
    const p = wrapper.findAll('p').find(n => n.text().includes('Alice'))
    expect(p).toBeTruthy()
    expect(p.text()).toContain('Signed in as')
    expect(p.text()).toContain('Alice')
    expect(p.text()).toContain('1')
  })

  it('shows connection error when connectionError is provided', () => {
    const wrapper = mount(AppHeader, {
      props: {
        connectionError: 'Connection failed'
      }
    })
    expect(wrapper.find('.error').text()).toBe('Connection failed')
  })

  it('does not show user or error when both are empty', () => {
    const wrapper = mount(AppHeader)
    const paragraphs = wrapper.findAll('p')
    expect(paragraphs).toHaveLength(0)
  })
})
