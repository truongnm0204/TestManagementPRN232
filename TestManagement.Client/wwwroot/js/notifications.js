// Realtime notification: lấy token, connect NotificationHub, cập nhật chuông
(function () {
    "use strict";

    const apiBase = document.querySelector('meta[name="api-base"]')?.content;
    if (!apiBase) return;

    const countEl = document.getElementById("notif-count");
    const listEl = document.getElementById("notif-list");
    const emptyEl = document.getElementById("notif-empty");
    let token = null;
    let unread = 0;

    // Lấy JWT từ endpoint Client (token nằm ở server-side session)
    async function getToken() {
        try {
            const res = await fetch("/Auth/Token");
            if (!res.ok) return null;
            const data = await res.json();
            return data.token;
        } catch { return null; }
    }

    // Gọi API kèm Bearer token
    function apiFetch(path, options = {}) {
        options.headers = Object.assign({}, options.headers, { "Authorization": "Bearer " + token });
        return fetch(apiBase + path, options);
    }

    function updateBadge() {
        if (unread > 0) {
            countEl.textContent = unread > 99 ? "99+" : unread;
            countEl.classList.remove("d-none");
        } else {
            countEl.classList.add("d-none");
        }
    }

    function renderItem(n) {
        if (emptyEl) emptyEl.classList.add("d-none");
        const li = document.createElement("li");
        li.innerHTML =
            '<a class="dropdown-item ' + (n.isRead ? "" : "fw-semibold") + '" href="#" data-id="' + n.id + '">' +
            '<div>' + escapeHtml(n.title) + '</div>' +
            '<small class="text-muted">' + escapeHtml(n.content || "") + '</small>' +
            '</a>';
        li.querySelector("a").addEventListener("click", function (e) {
            e.preventDefault();
            markRead(n.id, li.querySelector("a"));
        });
        return li;
    }

    function escapeHtml(s) {
        const div = document.createElement("div");
        div.textContent = s;
        return div.innerHTML;
    }

    async function markRead(id, anchor) {
        await apiFetch("api/notifications/" + id + "/read", { method: "POST" });
        if (anchor && anchor.classList.contains("fw-semibold")) {
            anchor.classList.remove("fw-semibold");
            unread = Math.max(0, unread - 1);
            updateBadge();
        }
    }

    // Tải danh sách + số chưa đọc ban đầu
    async function loadInitial() {
        const [listRes, countRes] = await Promise.all([
            apiFetch("api/notifications"),
            apiFetch("api/notifications/unread-count")
        ]);
        if (countRes.ok) {
            unread = (await countRes.json()).count || 0;
            updateBadge();
        }
        if (listRes.ok) {
            const items = await listRes.json();
            const headerLi = listEl.querySelector("li");
            items.forEach(n => listEl.appendChild(renderItem(n)));
        }
    }

    // Đánh dấu tất cả đã đọc
    document.getElementById("notif-mark-all")?.addEventListener("click", async function (e) {
        e.preventDefault();
        e.stopPropagation();
        await apiFetch("api/notifications/read-all", { method: "POST" });
        unread = 0;
        updateBadge();
        listEl.querySelectorAll(".dropdown-item.fw-semibold").forEach(a => a.classList.remove("fw-semibold"));
    });

    async function start() {
        token = await getToken();
        if (!token) return;

        await loadInitial();

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(apiBase + "hubs/notifications", { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .build();

        // Nhận thông báo realtime từ server
        connection.on("ReceiveNotification", function (n) {
            unread++;
            updateBadge();
            listEl.appendChild(renderItem(n));
            // Prepend lên đầu (sau header)
            const header = listEl.querySelector("li");
            listEl.insertBefore(listEl.lastChild, header.nextSibling);
        });

        try { await connection.start(); }
        catch (err) { console.error("SignalR connect failed:", err); }
    }

    start();
})();
