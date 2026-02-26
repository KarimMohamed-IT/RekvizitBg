/* ===================================================================
   РЕКВИЗИТ БГ — Site JavaScript (Security & Performance Enhanced)
   =================================================================== */

document.addEventListener('DOMContentLoaded', function () {

    // ===== NAVBAR SCROLL EFFECT =====
    (function () {
        var nav = document.getElementById('mainNav');
        if (!nav) return;

        var ticking = false;
        function onScroll() {
            if (!ticking) {
                requestAnimationFrame(function () {
                    if (window.scrollY > 60) {
                        nav.classList.add('scrolled');
                    } else {
                        nav.classList.remove('scrolled');
                    }
                    ticking = false;
                });
                ticking = true;
            }
        }
        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll();
    })();

    // ===== BACK TO TOP BUTTON =====
    (function () {
        var btn = document.getElementById('backToTop');
        if (!btn) return;

        var ticking = false;
        window.addEventListener('scroll', function () {
            if (!ticking) {
                requestAnimationFrame(function () {
                    if (window.scrollY > 400) {
                        btn.classList.add('visible');
                    } else {
                        btn.classList.remove('visible');
                    }
                    ticking = false;
                });
                ticking = true;
            }
        }, { passive: true });

        btn.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    })();

    // ===== SCROLL REVEAL (IntersectionObserver) =====
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

            function step(timestamp) {
                if (!startTime) startTime = timestamp;
                var progress = Math.min((timestamp - startTime) / duration, 1);
                var eased = 1 - Math.pow(1 - progress, 3);
                var current = Math.floor(num * eased);
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
                tabs.forEach(function (t) { t.classList.remove('active'); });
                document.querySelectorAll('.admin-tab-content').forEach(function (c) { c.classList.remove('active'); });
                tab.classList.add('active');
                var target = document.getElementById(tab.getAttribute('data-tab'));
                if (target) target.classList.add('active');
            });
        });
    })();

    // ===== UPLOAD AREAS (Drag & Drop + Preview) =====
    (function () {
        var uploadConfigs = [
            { area: 'catUploadArea', input: 'catImageInput', preview: 'catPreview' },
            { area: 'subUploadArea', input: 'subImageInput', preview: 'subPreview' },
            { area: 'prodUploadArea', input: 'prodImageInput', preview: 'prodPreview' }
        ];

        var MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
        var ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

        uploadConfigs.forEach(function (cfg) {
            var area = document.getElementById(cfg.area);
            var input = document.getElementById(cfg.input);
            var preview = document.getElementById(cfg.preview);
            if (!area || !input) return;

            area.addEventListener('click', function (e) {
                if (e.target !== input) input.click();
            });

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
                    var file = e.dataTransfer.files[0];
                    if (validateFile(file)) {
                        input.files = e.dataTransfer.files;
                        showPreview(file, preview);
                    }
                }
            });

            input.addEventListener('change', function () {
                if (input.files.length) {
                    var file = input.files[0];
                    if (validateFile(file)) {
                        showPreview(file, preview);
                    } else {
                        input.value = '';
                    }
                }
            });
        });

        function validateFile(file) {
            if (!ALLOWED_TYPES.includes(file.type)) {
                showToast('Позволени са само изображения (JPG, PNG, WebP, GIF).', 'error');
                return false;
            }
            if (file.size > MAX_FILE_SIZE) {
                showToast('Файлът е прекалено голям. Максимум 5MB.', 'error');
                return false;
            }
            return true;
        }

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

    // ===== CONTACT FORM — CLIENT-SIDE VALIDATION + SUBMIT PROTECTION =====
    (function () {
        var form = document.getElementById('contactForm');
        if (!form) return;

        var submitBtn = document.getElementById('submitBtn');
        var isSubmitting = false;

        // Email regex for client-side validation
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        var phoneRegex = /^[\d\+\-\s\(\)]+$/;

        form.addEventListener('submit', function (e) {
            // Prevent double submission
            if (isSubmitting) {
                e.preventDefault();
                return;
            }

            // Client-side validation
            var name = form.querySelector('[name="Name"]');
            var email = form.querySelector('[name="Email"]');
            var phone = form.querySelector('[name="Phone"]');
            var message = form.querySelector('[name="Message"]');
            var errors = [];

            if (!name || name.value.trim().length < 2) {
                errors.push('Моля, въведете вашето име (минимум 2 символа).');
            }
            if (!email || !emailRegex.test(email.value.trim())) {
                errors.push('Моля, въведете валиден имейл адрес.');
            }
            if (!phone || !phoneRegex.test(phone.value.trim())) {
                errors.push('Моля, въведете валиден телефонен номер.');
            }
            if (!message || message.value.trim().length < 10) {
                errors.push('Съобщението трябва да е поне 10 символа.');
            }

            if (errors.length > 0) {
                e.preventDefault();
                showToast(errors[0], 'error');
                return;
            }

            // Mark as submitting
            isSubmitting = true;
            if (submitBtn) {
                submitBtn.innerHTML = '<span class="spinner"></span> Изпращане...';
                submitBtn.disabled = true;
                submitBtn.style.opacity = '0.7';
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

    // ===== SMOOTH SCROLL FOR ANCHOR LINKS =====
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

    // ===== LAZY LOAD IMAGES =====
    (function () {
        if ('loading' in HTMLImageElement.prototype) return; // Native support
        var images = document.querySelectorAll('img[loading="lazy"]');
        if (!images.length) return;

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    var img = entry.target;
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                    }
                    observer.unobserve(img);
                }
            });
        });
        images.forEach(function (img) { observer.observe(img); });
    })();

});