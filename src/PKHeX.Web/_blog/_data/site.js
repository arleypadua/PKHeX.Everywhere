export default function () {
    const title = 'PKHeX.Web - Blog'
    const isLocal = ['serve', 'watch'].includes(process.env.ELEVENTY_RUN_MODE)
    const baseUrl = isLocal ? '/' : '/blog/'
    
    return {
        title,
        isLocal,
        baseUrl,
    };
}