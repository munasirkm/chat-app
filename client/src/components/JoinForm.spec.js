import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import JoinForm from './JoinForm.vue'

describe('JoinForm', () => {
  it('renders heading and input', () => {
    const wrapper = mount(JoinForm)
    expect(wrapper.find('h2').text()).toBe('Join the chat')
    expect(wrapper.find('input').exists()).toBe(true)
    expect(wrapper.find('button[type="submit"]').text()).toBe('Join')
  })

  it('emits join with userName when form is submitted', async () => {
    const wrapper = mount(JoinForm)
    await wrapper.find('input').setValue('Alice')
    await wrapper.find('form').trigger('submit.prevent')
    expect(wrapper.emitted('join')).toHaveLength(1)
    expect(wrapper.emitted('join')[0]).toEqual([{ userName: 'Alice' }])
  })

  it('trims whitespace from userName', async () => {
    const wrapper = mount(JoinForm)
    await wrapper.find('input').setValue('  Bob  ')
    await wrapper.find('form').trigger('submit.prevent')
    expect(wrapper.emitted('join')[0]).toEqual([{ userName: 'Bob' }])
  })

  it('does not emit when input is empty', async () => {
    const wrapper = mount(JoinForm)
    await wrapper.find('form').trigger('submit.prevent')
    expect(wrapper.emitted('join')).toBeFalsy()
  })

  it('submit button is disabled when input is empty', () => {
    const wrapper = mount(JoinForm)
    expect(wrapper.find('button[type="submit"]').attributes('disabled')).toBeDefined()
  })
})
