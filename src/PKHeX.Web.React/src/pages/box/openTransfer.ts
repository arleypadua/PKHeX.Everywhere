import type { Engine, SaveSummary, TransferSummary } from '@pkhex-everywhere/engine'
import { loadFailure, type LoadFailure } from '../load/loadFailure'

export type TransferOpening = LoadFailure | { kind: 'opened'; summary: TransferSummary } | { kind: 'noRoute'; partner: SaveSummary }

export async function openTransfer(
  transfer: Pick<Engine['transfer'], 'open' | 'close'>,
  file: File,
  formatId?: string,
): Promise<TransferOpening> {
  let summary: TransferSummary
  try {
    summary = await transfer.open(file, undefined, formatId)
  } catch (error) {
    const failure = loadFailure(error)
    if (!failure) throw error
    return failure
  }
  if (!summary.routes.receive) {
    await transfer.close()
    return { kind: 'noRoute', partner: summary.partner }
  }
  return { kind: 'opened', summary }
}
