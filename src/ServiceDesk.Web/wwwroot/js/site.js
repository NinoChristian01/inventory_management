/* ==========================================================================
   ServiceDesk shell behaviour. Progressive enhancement only: every page works without it.
   One IIFE per concern. Public helpers hang off window.sd:
     sd.toast(message, kind)       kind: success | info | warning | danger
     sd.fetchJson(url, options)    sends the antiforgery token on POST/PUT/PATCH/DELETE
     sd.labelTables(container)     call after inserting rows into a .sd-stack table
   ========================================================================== */

// ----- Namespace and safe storage -----
(function (window) {
    'use strict';
    var sd = window.sd = window.sd || {};

    sd.store = {
        get: function (key) {
            try { return window.localStorage.getItem(key); } catch (e) { return null; }
        },
        set: function (key, value) {
            try { window.localStorage.setItem(key, value); } catch (e) { /* storage blocked: ignore */ }
        }
    };
})(window);

// ----- Sidebar collapse (desktop) -----
(function (window, document) {
    'use strict';
    var root = document.documentElement;
    var toggle = document.querySelector('[data-sd-sidebar-toggle]');
    if (!toggle) { return; }

    function sync() {
        var collapsed = root.classList.contains('sd-nav-collapsed');
        toggle.setAttribute('aria-pressed', collapsed ? 'true' : 'false');
        toggle.setAttribute('aria-label', collapsed ? 'Expand sidebar' : 'Collapse sidebar');
    }

    toggle.addEventListener('click', function () {
        var collapsed = root.classList.toggle('sd-nav-collapsed');
        window.sd.store.set('sd.sidebar', collapsed ? 'collapsed' : 'expanded');
        sync();
    });
    sync();
})(window, document);

// ----- Toasts -----
(function (window, document) {
    'use strict';
    var container = document.getElementById('sdToasts');
    var icons = {
        success: 'check-circle-fill',
        info: 'info-circle-fill',
        warning: 'exclamation-triangle-fill',
        danger: 'exclamation-octagon-fill'
    };

    function activate(el) {
        el.addEventListener('hidden.bs.toast', function () { el.remove(); });
        if (window.bootstrap && window.bootstrap.Toast) {
            window.bootstrap.Toast.getOrCreateInstance(el).show();
        }
    }

    window.sd.toast = function (message, kind) {
        if (!container) { return; }
        kind = icons[kind] ? kind : 'info';
        var isError = kind === 'danger';

        var el = document.createElement('div');
        el.className = 'toast sd-toast sd-toast-' + kind;
        el.setAttribute('role', isError ? 'alert' : 'status');
        el.setAttribute('aria-live', isError ? 'assertive' : 'polite');
        el.setAttribute('aria-atomic', 'true');
        el.setAttribute('data-bs-delay', '6000');
        if (isError) { el.setAttribute('data-bs-autohide', 'false'); }

        var row = document.createElement('div');
        row.className = 'd-flex';
        var body = document.createElement('div');
        body.className = 'toast-body';
        var icon = document.createElement('i');
        icon.className = 'bi bi-' + icons[kind] + ' sd-toast-icon';
        icon.setAttribute('aria-hidden', 'true');
        var text = document.createElement('span');
        text.textContent = message;
        var close = document.createElement('button');
        close.type = 'button';
        close.className = 'btn-close me-2 m-auto';
        close.setAttribute('data-bs-dismiss', 'toast');
        close.setAttribute('aria-label', 'Close');

        body.appendChild(icon);
        body.appendChild(text);
        row.appendChild(body);
        row.appendChild(close);
        el.appendChild(row);
        container.appendChild(el);
        activate(el);
    };

    if (container) {
        Array.prototype.forEach.call(container.querySelectorAll('[data-sd-autoshow]'), activate);
    }
})(window, document);

// ----- Confirm dialog for destructive actions (data-confirm-* attributes) -----
(function (window, document) {
    'use strict';
    var modalEl = document.getElementById('sdConfirm');
    if (!modalEl || !window.bootstrap || !window.bootstrap.Modal) { return; }

    var modal = window.bootstrap.Modal.getOrCreateInstance(modalEl);
    var titleEl = modalEl.querySelector('.sd-confirm-title');
    var textEl = modalEl.querySelector('.sd-confirm-text');
    var iconEl = modalEl.querySelector('[data-sd-confirm-icon] i');
    var okBtn = modalEl.querySelector('[data-sd-confirm-ok]');
    var cancelBtn = modalEl.querySelector('[data-sd-confirm-cancel]');
    var pending = null;

    document.addEventListener('click', function (e) {
        var trigger = e.target.closest ? e.target.closest('[data-confirm-title]') : null;
        if (!trigger || trigger.hasAttribute('data-sd-confirmed')) { return; }
        e.preventDefault();

        var danger = (trigger.getAttribute('data-confirm-kind') || 'danger') === 'danger';
        pending = trigger;
        modalEl.setAttribute('data-kind', danger ? 'danger' : 'default');
        titleEl.textContent = trigger.getAttribute('data-confirm-title');
        textEl.textContent = trigger.getAttribute('data-confirm-text') || '';
        okBtn.textContent = trigger.getAttribute('data-confirm-ok') || 'Confirm';
        cancelBtn.textContent = trigger.getAttribute('data-confirm-cancel') || 'Cancel';
        okBtn.className = 'btn ' + (danger ? 'btn-danger' : 'btn-primary');
        iconEl.className = 'bi ' + (danger ? 'bi-exclamation-triangle' : 'bi-question-circle');
        modal.show();
    });

    // The safe choice gets focus first.
    modalEl.addEventListener('shown.bs.modal', function () { cancelBtn.focus(); });

    okBtn.addEventListener('click', function () {
        var trigger = pending;
        pending = null;
        modal.hide();
        if (!trigger) { return; }

        if (trigger.form) {
            trigger.setAttribute('data-sd-confirmed', '');
            if (trigger.form.requestSubmit) {
                trigger.form.requestSubmit(trigger);
            } else {
                trigger.click();
            }
            trigger.removeAttribute('data-sd-confirmed');
        } else if (trigger.href) {
            window.location.href = trigger.href;
        }
    });
})(window, document);

// ----- Double-submit guard -----
(function (window, document) {
    'use strict';
    var selector = 'button[type="submit"], button:not([type]), input[type="submit"]';

    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!(form instanceof window.HTMLFormElement) || form.hasAttribute('data-sd-no-guard')) { return; }
        // Disable after the browser has collected the form data, and only if validation let it through.
        window.setTimeout(function () {
            if (e.defaultPrevented) { return; }
            Array.prototype.forEach.call(form.querySelectorAll(selector), function (button) {
                button.disabled = true;
                button.setAttribute('aria-busy', 'true');
            });
        }, 0);
    });

    // Coming back with the browser's Back button restores a disabled page; re-enable it.
    window.addEventListener('pageshow', function (e) {
        if (!e.persisted) { return; }
        Array.prototype.forEach.call(document.querySelectorAll('[aria-busy="true"]'), function (button) {
            button.disabled = false;
            button.removeAttribute('aria-busy');
        });
    });
})(window, document);

// ----- Phone tables: copy each header's text into its cells so .sd-stack rows can become cards -----
(function (window, document) {
    'use strict';
    function labelTables(container) {
        Array.prototype.forEach.call((container || document).querySelectorAll('table.sd-stack'), function (table) {
            var headers = Array.prototype.map.call(table.querySelectorAll('thead th'), function (th) {
                return th.textContent.trim();
            });
            Array.prototype.forEach.call(table.querySelectorAll('tbody tr'), function (row) {
                Array.prototype.forEach.call(row.children, function (cell, i) {
                    if (headers[i] && !cell.hasAttribute('data-label')) {
                        cell.setAttribute('data-label', headers[i]);
                    }
                });
            });
            table.classList.add('has-labels');
        });
    }

    window.sd.labelTables = labelTables;
    labelTables(document);
})(window, document);

// ----- JSON requests that carry the antiforgery token -----
(function (window, document) {
    'use strict';
    var meta = document.querySelector('meta[name="request-verification-token"]');

    window.sd.fetchJson = function (url, options) {
        options = options || {};
        var method = (options.method || 'GET').toUpperCase();
        var headers = new window.Headers(options.headers || {});
        headers.set('Accept', 'application/json');
        if (meta && method !== 'GET' && method !== 'HEAD') {
            headers.set('RequestVerificationToken', meta.getAttribute('content'));
        }
        if (options.body && typeof options.body !== 'string' && !(options.body instanceof window.FormData)) {
            headers.set('Content-Type', 'application/json');
            options.body = JSON.stringify(options.body);
        }
        options.method = method;
        options.headers = headers;
        options.credentials = 'same-origin';

        return window.fetch(url, options).then(function (response) {
            var type = response.headers.get('Content-Type') || '';
            var read = type.indexOf('json') >= 0 ? response.json() : Promise.resolve(null);
            return read.then(function (data) {
                if (!response.ok) {
                    var error = new Error((data && (data.detail || data.title)) || 'The request failed (' + response.status + '). Try again.');
                    error.status = response.status;
                    error.data = data;
                    throw error;
                }
                return data;
            });
        });
    };
})(window, document);

// ----- Client-side tables (DataTables) -----
// Pages load the DataTables script after this file, so set tables up once the document is ready.
(function (window, document) {
    'use strict';

    function init() {
        if (!window.DataTable) { return; }
        Array.prototype.forEach.call(document.querySelectorAll('table[data-sd-datatable]'), function (table) {
            var pageLength = parseInt(table.getAttribute('data-sd-page-length') || '10', 10);
            // DataTables mutates options, so each table gets its own object.
            var dataTable = new window.DataTable(table, {
                pageLength: pageLength,
                lengthMenu: [5, 10, 25, 50],
                order: [],
                autoWidth: false,
                language: {
                    search: '',
                    searchPlaceholder: 'Search this table',
                    lengthMenu: 'Show _MENU_ rows',
                    info: 'Showing _START_ to _END_ of _TOTAL_',
                    infoEmpty: 'No rows to show',
                    infoFiltered: '(filtered from _MAX_)',
                    zeroRecords: 'No rows match your search.',
                    emptyTable: 'Nothing here yet.'
                }
            });
            // Rows drawn later (paging, search, server data) need phone labels too.
            dataTable.on('draw', function () { window.sd.labelTables(table.parentNode); });
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})(window, document);

// ----- "Go back" buttons (hidden until there is somewhere to go back to) -----
(function (window, document) {
    'use strict';
    if (window.history.length < 2) { return; }
    Array.prototype.forEach.call(document.querySelectorAll('[data-sd-back]'), function (button) {
        button.hidden = false;
        button.addEventListener('click', function () { window.history.back(); });
    });
})(window, document);
