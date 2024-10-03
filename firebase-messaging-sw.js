importScripts(
    'https://www.gstatic.com/firebasejs/10.13.2/firebase-app-compat.js'
);
importScripts(
    'https://www.gstatic.com/firebasejs/10.13.2/firebase-messaging-compat.js'
);
const firebaseConfig = {
    apiKey: "AIzaSyCb6iGiDt9cJrCntYapA24dOgIGEWWxVoA",
    authDomain: "pdbutik-d5391.firebaseapp.com",
    projectId: "pdbutik-d5391",
    storageBucket: "pdbutik-d5391.appspot.com",
    messagingSenderId: "24897794233",
    appId: "1:24897794233:web:543234cdd2fd866e8f2b68",
    measurementId: "G-M3FJSHSEMB"
};

const app = firebase.initializeApp(firebaseConfig);

const messaging = firebase.messaging();

messaging.onBackgroundMessage(function (payload) {
    console.log('[firebase-messaging-sw.js] Received background message', payload);

    const notificationTitle = payload.notification.title;
    const notificationOptions = {
        body: payload.notification.body,
    };

    self.registration.showNotification(notificationTitle, notificationOptions);
});