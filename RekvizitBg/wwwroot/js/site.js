/* ===================================================================
   РЕКВИЗИТ БГ — Site JavaScript
   =================================================================== */

document.addEventListener('DOMContentLoaded', function () {

    // ===== NAVBAR SCROLL EFFECT =====
    (function () {
        var nav = document.getElementById('mainNav');
        if (!nav) return;

        function onScroll() {
            if (window.scrollY > 60) {
                nav.classList.add('scrolled');
            } else {
                nav.classList.remove('scrolled');
            }
        }
        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll();
    })();

    // ===== BACK TO TOP BUTTON =====
    (function () {
        var btn = document.getElementById('backToTop');
        if (!btn) return;

        window.addEventListener('scroll', function () {
            if (window.scrollY > 400) {
                btn.classList.add('visible');
            } else {
                btn.classList.remove('visible');
            }
        }, { passive: true });

        btn.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    })();

    // ===== SCROLL REVEAL =====
    (function () {
        var reveals = document.querySelectorAll('.reveal');
        if (!reveals.length) return;

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        });

        reveals.forEach(function (el) {
            observer.observe(el);
        });
    })();

    // ===== HERO STAT COUNTER ANIMATION =====
    (function () {
        var statNumbers = document.querySelectorAll('.hero-stat-number, .stat-number');
        if (!statNumbers.length) return;

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    animateCounter(entry.target);
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.5 });

        statNumbers.forEach(function (el) { observer.observe(el); });

        function animateCounter(el) {
            var text = el.textContent.trim();
            var hasPlus = text.includes('+');
            var num = parseInt(text.replace(/\D/g, ''));
            if (isNaN(num)) return;

            var duration = 2000;
            var startTime = null;
            var startVal = 0;

            function step(timestamp) {
                if (!startTime) startTime = timestamp;
                var progress = Math.min((timestamp - startTime) / duration, 1);
                var eased = 1 - Math.pow(1 - progress, 3); // ease-out cubic
                var current = Math.floor(startVal + (num - startVal) * eased);
                el.textContent = current + (hasPlus ? '+' : '');
                if (progress < 1) {
                    requestAnimationFrame(step);
                }
            }
            requestAnimationFrame(step);
        }
    })();

    // ===== ADMIN TABS =====
    (function () {
        var tabs = document.querySelectorAll('.admin-tab');
        if (!tabs.length) return;

        tabs.forEach(function (tab) {
            tab.addEventListener('click', function () {
                // Deactivate all
                tabs.forEach(function (t) { t.classList.remove('active'); });
                document.querySelectorAll('.admin-tab-content').forEach(function (c) { c.classList.remove('active'); });
                // Activate clicked
                tab.classList.add('active');
                var target = document.getElementById(tab.getAttribute('data-tab'));
                if (target) target.classList.add('active');
            });
        });
    })();

    // ===== UPLOAD AREAS =====
    (function () {
        var uploadConfigs = [
            { area: 'catUploadArea', input: 'catImageInput', preview: 'catPreview' },
            { area: 'subUploadArea', input: 'subImageInput', preview: 'subPreview' },
            { area: 'prodUploadArea', input: 'prodImageInput', preview: 'prodPreview' }
        ];

        uploadConfigs.forEach(function (cfg) {
            var area = document.getElementById(cfg.area);
            var input = document.getElementById(cfg.input);
            var preview = document.getElementById(cfg.preview);
            if (!area || !input) return;

            // Click to open file picker
            area.addEventListener('click', function (e) {
                if (e.target !== input) input.click();
            });

            // Drag & drop
            area.addEventListener('dragover', function (e) {
                e.preventDefault();
                area.classList.add('dragover');
            });
            area.addEventListener('dragleave', function () {
                area.classList.remove('dragover');
            });
            area.addEventListener('drop', function (e) {
                e.preventDefault();
                area.classList.remove('dragover');
                if (e.dataTransfer.files.length) {
                    input.files = e.dataTransfer.files;
                    showPreview(input.files[0], preview);
                }
            });

            // File selected
            input.addEventListener('change', function () {
                if (input.files.length) {
                    showPreview(input.files[0], preview);
                }
            });
        });

        function showPreview(file, previewEl) {
            if (!previewEl || !file) return;
            var reader = new FileReader();
            reader.onload = function (e) {
                previewEl.innerHTML = '<img src="' + e.target.result + '" alt="Preview" />';
            };
            reader.readAsDataURL(file);
        }
    })();

    // ===== SIDEBAR TOGGLE (Catalog) =====
    (function () {
        var buttons = document.querySelectorAll('.sidebar-category-btn');
        buttons.forEach(function (btn) {
            var nextSubs = btn.parentElement.querySelector('.sidebar-subcategories');
            if (nextSubs && btn.classList.contains('active')) {
                nextSubs.classList.add('open');
            }
        });
    })();

    // ===== TOAST NOTIFICATIONS =====
    window.showToast = function (message, type) {
        var existing = document.querySelector('.toast-notification');
        if (existing) existing.remove();

        var toast = document.createElement('div');
        toast.className = 'toast-notification ' + (type || 'success');
        toast.innerHTML = '<i class="fa-solid ' + (type === 'error' ? 'fa-circle-exclamation' : 'fa-circle-check') + '"></i> ' + message;
        document.body.appendChild(toast);

        setTimeout(function () { toast.classList.add('show'); }, 100);
        setTimeout(function () {
            toast.classList.remove('show');
            setTimeout(function () { toast.remove(); }, 400);
        }, 4000);
    };

    // ===== SMOOTH SECTION SCROLL FOR ANCHOR LINKS =====
    document.querySelectorAll('a[href^="#"]').forEach(function (link) {
        link.addEventListener('click', function (e) {
            var targetId = this.getAttribute('href');
            if (targetId === '#') return;
            var target = document.querySelector(targetId);
            if (target) {
                e.preventDefault();
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });

    // ===== CONTACT FORM SUBMISSION FEEDBACK =====
    (function () {
        var form = document.getElementById('contactForm');
        if (!form) return;

        form.addEventListener('submit', function () {
            var btn = document.getElementById('submitBtn');
            if (btn) {
                btn.innerHTML = '<span class="spinner"></span> Изпращане...';
                btn.disabled = true;
            }
        });
    })();

});