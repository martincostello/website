// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

console.log("Started Service Worker.", self);

self.addEventListener("install", function (event) {
    event.waitUntil(
        caches.open("martincostello.com").then(function (cache) {
            return cache.addAll([
                "/",
                "/assets/css/site.css",
                "/assets/css/site.min.css",
                "/assets/js/site.js",
                "/assets/js/site.min.js",
                "/assets/img/browserstack.svg"
            ]);
        }).then(function () {
            return self.skipWaiting();
        })
    );
    console.log("Installed Service Worker.");
});

self.addEventListener("activate", function (event) {
    console.log("Activated Service Worker.");
});

self.addEventListener("fetch", function (event) {
    event.respondWith(
        caches.match(event.request)
            .then(function (response) {
                return response || fetch(event.request);
            })
    );
});
