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
        if (!res.ok) return null; // the server refused the request (400, 404, 401...)
        return await res.json();
    } catch (e) {
        console.log(e);
        return null;
    }
}

export async function apiDelete(url) {
    try {
        const res = await fetch(url, { method: "DELETE" });
        return res.ok; // false when the server refused (404, 401...)
    } catch (e) {
        console.log(e);
        return false;
    }
}
