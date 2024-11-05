import {setupWindow} from "./window.ts";
import {initFirebase} from "./firebase.ts";

async function main() {
    setupWindow()
    await initFirebase()
}

main()