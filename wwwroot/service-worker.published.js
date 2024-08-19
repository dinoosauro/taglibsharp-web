const cacheName = 'taglibsharpweb-cache';
self.importScripts('./service-worker-assets.js');
self.addEventListener('install', e => {
    caches.open(cacheName).then((cache) => cache.addAll(self.assetsManifest.assets.filter(file => file.url.indexOf("service-worker") === -1 && file.url.indexOf("updatecode") === -1).map(file => `${self.location.href.substring(0, self.location.href.lastIndexOf("/") + 1)}${file.url}`)));
});
self.addEventListener('activate', e => self.clients.claim());
self.addEventListener('fetch', event => {
    const req = event.request;
    if (req.url.indexOf("updatecode") !== -1) event.respondWith(fetch(req)); else event.respondWith(networkFirst(req));
});

async function networkFirst(req) {
    try {
        const networkResponse = await fetch(req);
        const cache = await caches.open(cacheName);
        await cache.delete(req);
        await cache.put(req, networkResponse.clone());
        return networkResponse;
    } catch (error) {
        const cachedResponse = await caches.match(req);
        return cachedResponse;
    }
}