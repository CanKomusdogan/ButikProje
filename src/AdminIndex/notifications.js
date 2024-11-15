import { initializeApp } from 'https://www.gstatic.com/firebasejs/10.13.2/firebase-app.js';
import { getMessaging, getToken, onMessage } from 'https://www.gstatic.com/firebasejs/10.13.2/firebase-messaging.js';

const firebaseConfig = {
    apiKey: "AIzaSyCb6iGiDt9cJrCntYapA24dOgIGEWWxVoA",
    authDomain: "pdbutik-d5391.firebaseapp.com",
    projectId: "pdbutik-d5391",
    storageBucket: "pdbutik-d5391.appspot.com",
    messagingSenderId: "24897794233",
    appId: "1:24897794233:web:543234cdd2fd866e8f2b68",
    measurementId: "G-M3FJSHSEMB"
};

const app = initializeApp(firebaseConfig);

if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/firebase-messaging-sw.js')
            .then(function (registration) {
                console.log('Service Worker registered with scope:', registration.scope);
            })
            .catch(function (err) {
                console.error('Service Worker registration failed:', err);
            });
    });
}


const messaging = getMessaging(app);

Notification.requestPermission()
    .then((permission) => {
        if (permission === 'granted') {
            console.log('Notification permission granted.');

            const notifAudio = document.getElementById('notifAudio');
            const newOrderNotif = document.getElementById('newOrderNotif');
            const newOrderNotifBody = newOrderNotif.querySelector('.toast-body');

            const newOrderNotifToast = bootstrap.Toast.getOrCreateInstance(newOrderNotif);

            getToken(messaging, { vapidKey: 'BPODaZ_LZzGZQ8Xa8lVng4sgcCIrPb1gzBUSVFwuH2DeAIpFxuI9rXIkTgRIAhhX9gtSWlELiO6lUrzN7WaByMs' }).then((currentToken) => {
                if (currentToken) {
                    fetch('/Admin/SaveDeviceToken', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify({
                            token: currentToken,
                        }),
                    }).then(response => {
                        if (!response.ok) {
                            throw new Error("Network response was not ok!");
                        }
                        return response.json();
                    }).then(data => {
                        console.log("Success: ", data);
                    }).catch(error => {
                        console.error("Error: ", error);
                    });
                } else {
                    console.log('No registration token available.');
                }
            }).catch((err) => {
                console.error('An error occurred while retrieving the token.', err);
            });

            onMessage(messaging, (payload) => {
                notifAudio.play();

                newOrderNotifBody.textContent = "Yeni siparişler var!";
                newOrderNotifToast.show();
                console.log('Message received. ', payload);
            });
        } else {
            console.log('Unable to get permission to notify.');
        }
    });