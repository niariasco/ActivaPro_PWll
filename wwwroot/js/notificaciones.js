let hub = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notificaciones")
    .withAutomaticReconnect()
    .build();

const badge = document.getElementById("notif-count");
const list = document.getElementById("notif-list");

function fmt(dt) { return new Date(dt + "Z").toLocaleString(); }

function render(n) {
    return `<li class="notif-item ${n.leido ? 'read' : 'unread'}" data-id="${n.idNotificacion}">
        <div class="d-flex justify-content-between">
          <span class="badge ${n.accion==='TicketStateChange'?'bg-info':'bg-secondary'}">${n.accion}</span>
          <small class="text-muted">${fmt(n.fechaEnvio)}</small>
        </div>
        <div class="mt-1">${n.mensaje}</div>
        <div class="mt-2 d-flex flex-wrap gap-1">
          ${!n.leido ? `<button class="btn btn-sm btn-outline-primary mark-read" data-id="${n.idNotificacion}">Marcar leída</button>` : ''}
          ${n.idTicket ? `<a class="btn btn-sm btn-link p-0" href="/Ticketes/Details/${n.idTicket}">Ver Ticket</a>` : ''}
        </div>
      </li>`;
}

async function loadPanel() {
    const r = await fetch("/Notificaciones/Panel");
    const data = await r.json();
    list.innerHTML = data.map(render).join('');
    if (data.length === 0) {
        emptyEl().style.display = 'block';
    } else {
        emptyEl().style.display = 'none';
    }
    updateBadge();
}

async function updateBadge() {
    const r = await fetch("/Notificaciones/Unread");
    const data = await r.json();
    if (badge) badge.textContent = data.count;
}

async function markRead(id) {
    const r = await fetch("/Notificaciones/MarkRead?id=" + id, { method: "POST" });
    const res = await r.json();
    if (res.success) {
        loadPanel();
    }
}

function initHub() {
    hub.on("notificaciones:nueva", n => {
        if (list) {
            list.insertAdjacentHTML("afterbegin", render(n));
            emptyEl().style.display = 'none';
        }
        updateBadge();
    });

    hub.on("notificaciones:actualizada", () => loadPanel());

    hub.start().then(loadPanel).catch(console.error);
}

document.addEventListener("click", async e => {
    if (e.target.classList.contains("mark-read")) {
        const id = e.target.getAttribute("data-id");
        markRead(id);
    }
});

document.addEventListener("DOMContentLoaded", () => {
    initHub();
});