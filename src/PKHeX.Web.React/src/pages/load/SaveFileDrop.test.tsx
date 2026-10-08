import { act } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { render } from '../../testing/render'
import { SaveFileDrop } from './SaveFileDrop'

const save = new File([new Uint8Array(8)], 'emerald.sav')

function dragFiles(files: File[]) {
  return { types: ['Files'], files, dropEffect: 'none' }
}

function dragText() {
  return { types: ['text/plain'], files: [], dropEffect: 'none' }
}

function dispatch(target: Element, type: string, dataTransfer: object) {
  const event = new Event(type, { bubbles: true, cancelable: true })
  Object.defineProperty(event, 'dataTransfer', { value: dataTransfer })
  act(() => {
    target.dispatchEvent(event)
  })
}

function renderDrop(props: { onOpen?: (file: File) => void; disabled?: boolean } = {}) {
  const { container } = render(
    <SaveFileDrop onOpen={props.onOpen ?? (() => {})} disabled={props.disabled}>
      <button>Open</button>
    </SaveFileDrop>,
  )
  return {
    target: container.querySelector('button')!,
    overlay: () => container.querySelector('[role="dialog"]'),
  }
}

describe('SaveFileDrop', () => {
  it('shows the overlay while a file is dragged over it', () => {
    const { target, overlay } = renderDrop()

    dispatch(target, 'dragenter', dragFiles([save]))

    expect(overlay()).not.toBeNull()
  })

  it('ignores a drag that carries no files', () => {
    const { target, overlay } = renderDrop()

    dispatch(target, 'dragenter', dragText())

    expect(overlay()).toBeNull()
  })

  it('hides the overlay when the file leaves', () => {
    const { target, overlay } = renderDrop()

    dispatch(target, 'dragenter', dragFiles([save]))
    dispatch(target, 'dragleave', dragFiles([save]))

    expect(overlay()).toBeNull()
  })

  it('opens the first dropped file and hides the overlay', () => {
    const onOpen = vi.fn()
    const { target, overlay } = renderDrop({ onOpen })

    dispatch(target, 'dragenter', dragFiles([save]))
    dispatch(target, 'drop', dragFiles([save, new File([], 'other.sav')]))

    expect(onOpen).toHaveBeenCalledExactlyOnceWith(save)
    expect(overlay()).toBeNull()
  })

  it('opens nothing while disabled', () => {
    const onOpen = vi.fn()
    const { target, overlay } = renderDrop({ onOpen, disabled: true })

    dispatch(target, 'dragenter', dragFiles([save]))
    expect(overlay()).toBeNull()

    dispatch(target, 'drop', dragFiles([save]))
    expect(onOpen).not.toHaveBeenCalled()
  })
})
