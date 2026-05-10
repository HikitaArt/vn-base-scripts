// ── Состояние ──────────────────────────────
let currentPage = 'page-home';

// ── Навигация между страницами ─────────────
function navigateTo(pageId) {
    if (currentPage === pageId) return;

    // Скрыть текущую страницу
    const currentEl = document.getElementById(currentPage);
    if (currentEl) {
        currentEl.classList.remove('active');
    }

    // Показать новую
    const targetEl = document.getElementById(pageId);
    if (targetEl) {
        targetEl.classList.remove('active');
        void targetEl.offsetWidth; // рефлоу для перезапуска анимации
        targetEl.classList.add('active');
        currentPage = pageId;
    }

    // Обновить активный пункт в боковой панели
    document.querySelectorAll('.sidebar-link').forEach(link => {
        link.classList.remove('active-link');
    });
    const activeLink = document.querySelector(`.sidebar-link[data-target="${pageId}"]`);
    if (activeLink) {
        activeLink.classList.add('active-link');
    }

    // Прокрутка наверх
    window.scrollTo({ top: 0, behavior: 'smooth' });

    // Обновить хеш в URL
    history.pushState(null, null, '#' + pageId);
}

// ── Закрытие по Escape (больше не нужно для меню, но можно оставить для других целей, если понадобится) ──
// Оставлено на будущее

// ── Инициализация при загрузке ──────────────
function init() {
    const hash = window.location.hash.replace('#', '');
    const validPages = [
        'page-home',
        'page-getting-started',
        'page-interface',
        'page-tag-system',
        'page-links'
    ];
    if (hash && validPages.includes(hash) && hash !== 'page-home') {
        navigateTo(hash);
    }

    // Обработка кнопок "назад/вперед" браузера
    window.addEventListener('popstate', function() {
        const newHash = window.location.hash.replace('#', '');
        if (newHash && document.getElementById(newHash)) {
            const targetEl = document.getElementById(newHash);
            const currentEl = document.getElementById(currentPage);
            if (currentEl) currentEl.classList.remove('active');
            if (targetEl) {
                targetEl.classList.remove('active');
                void targetEl.offsetWidth;
                targetEl.classList.add('active');
                currentPage = newHash;
            }
            // Обновить сайдбар
            document.querySelectorAll('.sidebar-link').forEach(link => link.classList.remove('active-link'));
            const activeLink = document.querySelector(`.sidebar-link[data-target="${newHash}"]`);
            if (activeLink) activeLink.classList.add('active-link');
            window.scrollTo({ top: 0, behavior: 'smooth' });
        } else if (!newHash || newHash === 'page-home') {
            const targetEl = document.getElementById('page-home');
            const currentEl = document.getElementById(currentPage);
            if (currentEl) currentEl.classList.remove('active');
            if (targetEl) {
                targetEl.classList.remove('active');
                void targetEl.offsetWidth;
                targetEl.classList.add('active');
                currentPage = 'page-home';
            }
            document.querySelectorAll('.sidebar-link').forEach(link => link.classList.remove('active-link'));
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    });
}

document.addEventListener('DOMContentLoaded', init);