"use strict";

console.log("Started", self);

self.addEventListener("install", function (event) {
    self.skipWaiting();
    console.log("Installed", event);
});

self.addEventListener("activate", function (event) {
    console.log("Activated", event);
});

self.addEventListener("push", function (event) {
    console.log("Push message received", event);
    var title = "martincostello.com";
    event.waitUntil(
      self.registration.showNotification(title, {
          body: "Hello!",
          icon: "https://martincostello.com/favicon-96x96.png",
          tag: "martincostello.com"
      }));
});

self.addEventListener("notificationclick", function (event) {
    console.log("Notification click: tag ", event.notification.tag);
    event.notification.close();
    var url = "https://martincostello.com/";
    event.waitUntil(
        clients.matchAll({
            type: "window"
        })
        .then(function (windowClients) {
            for (var i = 0; i < windowClients.length; i++) {
                var client = windowClients[i];
                if (client.url === url && "focus" in client) {
                    return client.focus();
                }
            }
            if (clients.openWindow) {
                return clients.openWindow(url);
            }
        })
    );
});
