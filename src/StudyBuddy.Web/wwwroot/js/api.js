// 🌐 API helper - use these instead of calling fetch directly!

export async function apiGet(url) {
    try {
        const res = await fetch(url);
        return await res.json();
    } catch (e) {
        console.log(e);
        return null;
    }
}

export async function apiPost(url, body) {
    try {
        const res = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });
        return await res.json();
    } catch (e) {
        console.log(e);
        return null;
    }
}

export async function apiDelete(url) {
    try {
        await fetch(url, { method: "DELETE" });
        return true;
    } catch (e) {
        console.log(e);
        return false;
    }
}
