import {FirebaseOptions} from "@firebase/app";
import {UserCredential} from "@firebase/auth";

import { initializeApp } from 'firebase/app'
import { getAuth, signInAnonymously } from 'firebase/auth'

const config: FirebaseOptions = {
    apiKey: import.meta.env.VITE_FIREBASE_API_KEY,
    authDomain: `${import.meta.env.VITE_FIREBASE_PROJECT_NAME}.firebaseapp.com`,
    projectId: import.meta.env.VITE_FIREBASE_PROJECT_NAME,
    storageBucket: `${import.meta.env.VITE_FIREBASE_PROJECT_NAME}.firebasestorage.app`,
    messagingSenderId: import.meta.env.VITE_FIREBASE_MESSAGING_SENDER_ID,
    appId: import.meta.env.VITE_FIREBASE_APP_ID,
}

export async function initFirebase() {
    if (import.meta.env.VITE_FIREBASE_ENABLED !== 'true') {
        return    
    }
    
    const app = initializeApp(config)
    const auth = getAuth(app)
    
    await signInAnonymously(auth)
        .then(handleAnonymousSignIn)
        .catch(handleAnonymousSignInError);

    function handleAnonymousSignIn(credential: UserCredential) {
        console.log(credential);
    }

    function handleAnonymousSignInError(error: any) {
        console.log(error);
    }
}