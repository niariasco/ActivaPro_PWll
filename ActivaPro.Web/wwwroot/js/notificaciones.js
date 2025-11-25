const Notifs = (() => {
    let hub = null;
    let initialized = false;
    let loading = false;

    function el(id) { return document.getElementById(id); }

    function mapDto(n) {
        return {
            id: n.idNotificacion ?? n.IdNotificacion,
            ticketId: n.idTicket ?? n.IdTicket,
            accion: n.accion ?? n.Accion,
            mensaje: n.mensaje ?? n.Mensaje,
            leido: n.leido ?? n.Leido,
            fechaEnvio: n.fechaEnvio ?? n.FechaEnvio
        };
    }

    function fmtFecha(iso) {
        if (!iso) return '';
        const d = new Date(iso);
        return d.toLocaleDateString() + ' ' + d.toLocaleTimeString();
    }

    function icono(accion) {
        const azul = '#1E4E8C', gris = '#6c757d';
        switch (accion) {
            case 'Login': return `<i class="bi bi-box-arrow-in-right" style="color:${gris};"></i>`;
            case 'Logout': return `<i class="bi bi-box-arrow-left" style="color:${gris};"></i>`;
            case 'TicketStateChange': return `<i class="bi bi-arrow-left-right" style="color:${azul};"></i>`;
            case 'TicketEvent': return `<i class="bi bi-pencil-square" style="color:${azul};"></i>`;
            default: return `<i class="bi bi-info-circle" style="color:${gris};"></i>`;
        }
    }

    function badgeTipo(accion) {
        switch (accion) {
            case 'Login': return '<span class="badge notif-badge-gray">LOGIN</span>';
            case 'Logout': return '<span class="badge notif-badge-gray">LOGOUT</span>';
            case 'TicketStateChange': return '<span class="badge notif-badge-blue">ESTADO</span>';
            case 'TicketEvent': return '<span class="badge notif-badge-blue">EVENTO</span>';
            default: return '<span class="badge notif-badge-gray">EVENTO</span>';
        }
    }

    function renderItem(raw) {
        const n = mapDto(raw);
        return `
<li id="notif-${n.id}" class="notif-item ${n.leido ? 'read' : 'unread'}">
  <div class="d-flex justify-content-between align-items-start">
    <div class="fw-semibold d-flex flex-column" style="gap:2px;">
      <span class="d-flex align-items-center" style="gap:6px;">
        ${icono(n.accion)} ${badgeTipo(n.accion)}
      </span>
      <span>${n.mensaje}</span>
    </div>
    <small class="text-muted ms-2">${fmtFecha(n.fechaEnvio)}</small>
  </div>
  <div class="mt-2 d-flex justify-content-between align-items-center">
    ${
        n.ticketId && n.accion !== 'Eliminación'
            ? `<a href="/Ticketes/Edit/${n.ticketId}" class="btn btn-sm btn-outline-primary notif-btn">Acceder Ticket</a>`
            : '<span></span>'
    }
    ${
        n.leido
            ? '<button class="btn btn-sm btn-outline-secondary" disabled><i class="bi bi-check2-circle"></i> Leída</button>'
            : `<button class="btn btn-sm btn-outline-primary notif-btn" onclick="Notifs.markRead(${n.id})">
                <i class="bi bi-eye"></i> Marcar como leída
               </button>`
    }
  </div>
</li>`;
    }

    function load(skip = 0, take = 15) {
        if (loading) return;
        loading = true;
        fetch(`/Notificaciones/Panel?skip=${skip}&take=${take}`)
            .then(r => {
                if (!r.ok) throw new Error(`HTTP ${r.status}`);
                return r.json();
            })
            .then(data => {
                const list = el('notif-list');
                list.innerHTML = '';
                if (!Array.isArray(data) || data.length === 0) {
                    el('notif-empty').style.display = 'block';
                } else {
                    el('notif-empty').style.display = 'none';
                    data.forEach(d => list.insertAdjacentHTML('beforeend', renderItem(d)));
                }
            })
            .catch(err => console.error('Error loading notifications:', err))
            .finally(() => loading = false);
        updateCount();
    }

    function updateCount() {
        fetch('/Notificaciones/Unread')
            .then(r => r.json())
            .then(data => refreshCountFrom(data.count ?? data.Count ?? 0))
            .catch(err => console.error('Error loading unread count:', err));
    }

    function refreshCountFrom(count) {
        const c = el('notif-count');
        c.textContent = count;
        c.style.display = count > 0 ? 'inline-block' : 'inline-block'; // siempre visible junto a la campana
    }

    function markRead(id) {
        fetch(`/Notificaciones/MarkRead?id=${id}`, { method: 'POST' })
            .then(r => r.json())
            .then(res => {
                if (res.success) {
                    const li = el(`notif-${id}`);
                    if (li) {
                        li.classList.remove('unread');
                        li.classList.add('read');
                        const btn = li.querySelector('button.btn-outline-primary');
                        if (btn) {
                            btn.classList.remove('btn-outline-primary');
                            btn.classList.add('btn-outline-secondary');
                            btn.innerHTML = '<i class="bi bi-check2-circle"></i> Leída';
                            btn.disabled = true;
                        }
                    }
                    refreshCountFrom(res.unread ?? res.Unread ?? 0);
                }
            })
            .catch(err => console.error('Error marking read:', err));
    }

    function connectHub() {
        if (typeof signalR === 'undefined') {
            console.warn('SignalR client not found. Fallback to manual load.');
            load();
            return;
        }

        hub = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/notificaciones')
            .withAutomaticReconnect()
            .build();

        hub.on('notificaciones:nueva', dto => {
            const list = el('notif-list');
            list.insertAdjacentHTML('afterbegin', renderItem(dto));
            updateCount();
        });

        hub.on('notificaciones:actualizada', info => {
            const id = info.IdNotificacion ?? info.idNotificacion;
            const item = el(`notif-${id}`);
            if (item) {
                item.classList.remove('unread');
                item.classList.add('read');
                const btn = item.querySelector('button.btn-outline-primary');
                if (btn) {
                    btn.classList.remove('btn-outline-primary');
                    btn.classList.add('btn-outline-secondary');
                    btn.innerHTML = '<i class="bi bi-check2-circle"></i> Leída';
                    btn.disabled = true;
                }
            }
            updateCount();
        });

        hub.start()
            .then(() => { initialized = true; load(); })
            .catch(err => { console.error('SignalR connect error:', err); load(); });
    }

    function init() {
        if (initialized) return;
        connectHub();
    }

    return {
        init,
        load,
        markRead,
        refreshCountFrom,
        renderItem
    };
})();

document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('notif-list')) {
        Notifs.init();
    }
});
