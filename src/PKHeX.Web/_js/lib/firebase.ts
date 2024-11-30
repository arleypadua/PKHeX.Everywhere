import {FirebaseOptions} from "@firebase/app";

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

export function initFirebase() {
    // disabled for now
    // if (import.meta.env.VITE_FIREBASE_ENABLED !== 'true') {
    //     return    
    // }
    
    const app = initializeApp(config)
    const auth = getAuth(app)
    
    auth.onIdTokenChanged(async (user) => {
        // if dotnet doesn't exist, return
        if (!window.DotNet || !window.DotNet.invokeMethodAsync) return
        
        try {
            if (user) {
                await DotNet.invokeMethodAsync("PKHeX.Web", "OnTokenChanged", await user.getIdToken())
            } else {
                await DotNet.invokeMethodAsync("PKHeX.Web", "OnTokenChanged", null)
            }   
        } catch {
            // skip as maybe the dotnet method doesn't exist
        }
    })
    
    window.isSignedIn = () => auth.currentUser !== null
    
    window.getAuthToken = async () => {
        const user = auth.currentUser
        if (!user) throw new Error('No user found')
        return await user.getIdToken()
    }
    
    window.signInAnonymously = async () => {
        const userCredential = await signInAnonymously(auth);
        return await userCredential.user.getIdToken();
    }
    
    window.getSignedInUser = () => {
        if (!auth.currentUser) throw new Error('No user found')
        return {
            id: auth.currentUser.uid,
            email: auth.currentUser.email,
            isAnonymous: auth.currentUser.isAnonymous,
        }
    }
    
    window.signOut = async () => {
        await auth.signOut();
    }
}