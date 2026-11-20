/* UI kit page only: demonstrates sd.fetchJson and sd.toast. */
(function (window, document) {
    'use strict';
    var sd = window.sd || {};

    var healthButton = document.querySelector('[data-sd-health-check]');
    if (healthButton && sd.fetchJson && sd.toast) {
        healthButton.addEventListener('click', function () {
            healthButton.disabled = true;
            sd.fetchJson('/health')
                .then(function (report) {
                    sd.toast('Health check finished: ' + report.status + '.', report.status === 'Healthy' ? 'success' : 'warning');
                })
                .catch(function (error) {
                    sd.toast(error.message, 'danger');
                })
                .then(function () {
                    healthButton.disabled = false;
                });
        });
    }

    var toastButton = document.querySelector('[data-sd-client-toast]');
    if (toastButton && sd.toast) {
        toastButton.addEventListener('click', function () {
            sd.toast('This toast was created in the browser, without a page reload.', 'info');
        });
    }
})(window, document);
