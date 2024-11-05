export type ContentStreamReference = {
    arrayBuffer: () => Promise<BlobPart>
}