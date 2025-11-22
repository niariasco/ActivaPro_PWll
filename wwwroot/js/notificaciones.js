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
        <div class="mt-2">
          ${!n.leido ? `<button class="btn btn-sm btn-outline-primary mark-read" data-id="${n.idNotificacion}">Marcar leída</button>` : ''}
          ${n.idTicket ? `<a class="btn btn-sm btn-link" href="/Tickets/Details/${n.idTicket}">Ver Ticket</a>` : ''}
        </div>
      </li>`;
}

async function loadPanel() {
    const r = await fetch("/Notificaciones/Panel");
    const data = await r.json();
    list.innerHTML = data.map(render).join('');
    updateBadge();
}

async function updateBadge() {
    const r = await fetch("/Notificaciones/Unread");
    const data = await r.json();
    if (badge) badge.textContent = data.count;
}

document.addEventListener("click", async e => {
    if (e.target.classList.contains("mark-read")) {
        const id = e.target.getAttribute("data-id");
        const res = await fetch("/Notificaciones/MarkRead?id=" + id, { method: "POST" });
        const json = await res.json();
        if (json.success) loadPanel();
    }
});

hub.on("notificaciones:nueva", n => {
    if (list) list.insertAdjacentHTML("afterbegin", render(n));
    updateBadge();
});

hub.on("notificaciones:actualizada", () => loadPanel());

hub.start().then(loadPanel).catch(console.error);