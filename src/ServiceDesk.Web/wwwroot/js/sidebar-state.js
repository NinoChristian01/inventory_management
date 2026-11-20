/* Restores the collapsed sidebar before the page paints, so it doesn't flash open.
   Loaded in <head> as a file because the Content-Security-Policy forbids inline scripts. */
(function () {
    'use strict';
    try {
        if (window.localStorage.getItem('sd.sidebar') === 'collapsed') {
            document.documentElement.classList.add('sd-nav-collapsed');
        }
    } catch (e) {
        // Storage can be blocked; the sidebar simply stays expanded.
    }
})();
