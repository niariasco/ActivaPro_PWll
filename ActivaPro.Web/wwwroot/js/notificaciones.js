const Notifs = (() => {
    let hub = null, initialized = false;
    let loading = false;
    let skip = 0;
    const take = 25; // tamaño página

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
        switch (accion) {
            case 'Login': return '<i class="bi bi-box-arrow-in-right text-light"></i>';
            case 'Logout': return '<i class="bi bi-box-arrow-left text-light"></i>';
            case 'TicketStateChange': return '<i class="bi bi-arrow-left-right text-light"></i>';
            case 'TicketEvent': return '<i class="bi bi-pencil-square text-light"></i>';
            default: return '<i class="bi bi-info-circle text-light"></i>';
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
    <div class="d-flex flex-column" style="gap:4px;">
      <span class="d-flex align-items-center" style="gap:6px;">
        ${icono(n.accion)} ${badgeTipo(n.accion)}
      </span>
      <span>${n.mensaje}</span>
    </div>
    <small>${fmtFecha(n.fechaEnvio)}</small>
  </div>
  <div class="mt-2 d-flex justify-content-between align-items-center">
    ${
        n.ticketId && n.accion !== 'Eliminación'
            ? `<a href="/Ticketes/Edit/${n.ticketId}" class="btn btn-sm btn-outline-light notif-btn-ticket">Ticket</a>`
            : '<span></span>'
    }
    ${
        n.leido
            ? '<span class="text-secondary" style="font-size:.7rem;"><i class="bi bi-check2-circle"></i> Leída</span>'
            : `<button class="btn btn-sm btn-outline-light notif-btn-mark" onclick="Notifs.markRead(${n.id})">
                <i class="bi bi-eye"></i> Marcar
               </button>`
    }
  </div>
</li>`;
    }

    function clearPanel() {
        const list = el('notif-list');
        list.innerHTML = '';
    }

    function load(reset = false) {
        if (loading) return;
        loading = true;
        if (reset) { skip = 0; clearPanel(); }

        fetch(`/Notificaciones/Panel?skip=${skip}&take=${take}`)
            .then(r => {
                if (!r.ok) throw new Error(`HTTP ${r.status}`);
                return r.json();
            })
            .then(data => {
                const list = el('notif-list');
                if (!Array.isArray(data) || data.length === 0) {
                    if (skip === 0) el('notif-empty').style.display = 'block';
                    return;
                }
                el('notif-empty').style.display = 'none';
                data.forEach(row => list.insertAdjacentHTML('beforeend', renderItem(row)));
                if (data.length === take) {
                    // hay más
                    ensureLoadMoreButton();
                } else {
                    removeLoadMoreButton();
                }
                skip += data.length;
            })
            .catch(err => console.error('Error cargando notificaciones:', err))
            .finally(() => {
                loading = false;
                updateCount();
            });
    }

    function ensureLoadMoreButton() {
        if (el('notif-load-more')) return;
        const btn = document.createElement('button');
        btn.id = 'notif-load-more';
        btn.type = 'button';
        btn.className = 'btn btn-sm btn-outline-light w-100 mt-1';
        btn.textContent = 'Cargar más';
        btn.onclick = () => load(false);
        el('notif-dropdown').appendChild(btn);
    }

    function removeLoadMoreButton() {
        const btn = el('notif-load-more');
        if (btn) btn.remove();
    }

    function updateCount() {
        fetch('/Notificaciones/Unread')
            .then(r => r.json())
            .then(data => refreshCountFrom(data.count ?? data.Count ?? 0))
            .catch(err => console.error('Error contador:', err));
    }

    function refreshCountFrom(count) {
        const c = el('notif-count');
        c.textContent = count;
        c.style.display = 'inline-block';
        c.classList.toggle('bg-secondary', count === 0);
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
                        const btn = li.querySelector('button.notif-btn-mark');
                        if (btn) {
                            btn.outerHTML = '<span class="text-secondary" style="font-size:.7rem;"><i class="bi bi-check2-circle"></i> Leída</span>';
                        }
                    }
                    refreshCountFrom(res.unread ?? 0);
                }
            })
            .catch(err => console.error('Error marcar leída:', err));
    }

    function connectHub() {
        if (typeof signalR === 'undefined') {
            console.warn('SignalR no disponible, modo sin tiempo real.');
            load(true);
            return;
        }
        hub = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/notificaciones')
            .withAutomaticReconnect()
            .build();

        hub.on('notificaciones:nueva', dto => {
            const list = el('notif-list');
            list.insertAdjacentHTML('afterbegin', renderItem(dto));
            el('notif-empty').style.display = 'none';
            updateCount();
        });

        hub.on('notificaciones:actualizada', info => {
            const id = info.IdNotificacion ?? info.idNotificacion;
            const item = el(`notif-${id}`);
            if (item) {
                item.classList.remove('unread');
                item.classList.add('read');
                const btn = item.querySelector('button.notif-btn-mark');
                if (btn) {
                    btn.outerHTML = '<span class="text-secondary" style="font-size:.7rem;"><i class="bi bi-check2-circle"></i> Leída</span>';
                }
            }
            updateCount();
        });

        hub.start()
            .then(() => { initialized = true; load(true); })
            .catch(err => { console.error('Error SignalR:', err); load(true); });
    }

    function init() {
        if (initialized) return;
        connectHub();
    }

    return { init, load, markRead };
})();

document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('notif-list')) {
        Notifs.init();
    }
});
