// Reusable Components Library
class UIComponents {
    // Toast Notification
    static showToast(message, type = 'info', duration = 3000) {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-icon">
                <i class="fas fa-${this.getToastIcon(type)}"></i>
            </div>
            <div class="toast-content">${message}</div>
            <button class="toast-close">&times;</button>
        `;

        document.body.appendChild(toast);

        // Add styles if not present
        if (!document.querySelector('#toast-styles')) {
            const style = document.createElement('style');
            style.id = 'toast-styles';
            style.textContent = `
                .toast {
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    background: white;
                    padding: 1rem 1.5rem;
                    border-radius: 8px;
                    box-shadow: 0 4px 15px rgba(0,0,0,0.1);
                    display: flex;
                    align-items: center;
                    gap: 1rem;
                    z-index: 9999;
                    transform: translateX(120%);
                    transition: transform 0.3s ease;
                    max-width: 400px;
                }
                .toast.show {
                    transform: translateX(0);
                }
                .toast-info { border-left: 4px solid #2962ff; }
                .toast-success { border-left: 4px solid #00c853; }
                .toast-warning { border-left: 4px solid #ffab00; }
                .toast-danger { border-left: 4px solid #ff1744; }
                .toast-icon {
                    width: 24px;
                    height: 24px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }
                .toast-info .toast-icon { color: #2962ff; }
                .toast-success .toast-icon { color: #00c853; }
                .toast-warning .toast-icon { color: #ffab00; }
                .toast-danger .toast-icon { color: #ff1744; }
                .toast-close {
                    background: none;
                    border: none;
                    font-size: 1.5rem;
                    cursor: pointer;
                    color: #718096;
                }
            `;
            document.head.appendChild(style);
        }

        // Show toast
        setTimeout(() => toast.classList.add('show'), 10);

        // Auto dismiss
        const dismiss = setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, duration);

        // Manual dismiss
        toast.querySelector('.toast-close').addEventListener('click', () => {
            clearTimeout(dismiss);
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        });
    }

    static getToastIcon(type) {
        const icons = {
            'info': 'info-circle',
            'success': 'check-circle',
            'warning': 'exclamation-triangle',
            'danger': 'times-circle'
        };
        return icons[type] || 'info-circle';
    }

    // Loading Spinner
    static showLoading(container = document.body, text = 'লোড হচ্ছে...') {
        const loading = document.createElement('div');
        loading.className = 'loading-overlay';
        loading.innerHTML = `
            <div class="loading-spinner">
                <div class="spinner"></div>
                <p>${text}</p>
            </div>
        `;

        // Add styles if not present
        if (!document.querySelector('#loading-styles')) {
            const style = document.createElement('style');
            style.id = 'loading-styles';
            style.textContent = `
                .loading-overlay {
                    position: fixed;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    background: rgba(255,255,255,0.9);
                    backdrop-filter: blur(4px);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    z-index: 99999;
                }
                .loading-spinner {
                    text-align: center;
                }
                .spinner {
                    width: 50px;
                    height: 50px;
                    border: 3px solid #e2e8f0;
                    border-top-color: #1a237e;
                    border-radius: 50%;
                    animation: spin 1s linear infinite;
                    margin: 0 auto 1rem;
                }
                @keyframes spin {
                    to { transform: rotate(360deg); }
                }
            `;
            document.head.appendChild(style);
        }

        container.appendChild(loading);
        return {
            hide: () => {
                loading.classList.add('fade-out');
                setTimeout(() => loading.remove(), 300);
            }
        };
    }

    // Modal
    static showModal(title, content, options = {}) {
        const modalId = 'modal-' + Date.now();
        const modal = document.createElement('div');
        modal.id = modalId;
        modal.className = 'modal-overlay';
        modal.innerHTML = `
            <div class="modal">
                <div class="modal-header">
                    <h3>${title}</h3>
                    <button class="modal-close">&times;</button>
                </div>
                <div class="modal-body">${content}</div>
                ${options.buttons ? `
                    <div class="modal-footer">
                        ${options.buttons.map(btn =>
            `<button class="btn btn-${btn.variant || 'primary'}" 
                                     onclick="${btn.onClick}">${btn.text}</button>`
        ).join('')}
                    </div>
                ` : ''}
            </div>
        `;

        document.body.appendChild(modal);

        // Add styles if not present
        if (!document.querySelector('#modal-styles')) {
            const style = document.createElement('style');
            style.id = 'modal-styles';
            style.textContent = `
                .modal-overlay {
                    position: fixed;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    background: rgba(0,0,0,0.5);
                    backdrop-filter: blur(4px);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    z-index: 9999;
                    animation: fadeIn 0.3s ease;
                }
                .modal {
                    background: white;
                    border-radius: 12px;
                    box-shadow: 0 20px 40px rgba(0,0,0,0.2);
                    max-width: 500px;
                    width: 90%;
                    max-height: 90vh;
                    overflow-y: auto;
                    animation: slideUp 0.3s ease;
                }
                .modal-header {
                    padding: 1.5rem;
                    border-bottom: 1px solid #e2e8f0;
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                }
                .modal-close {
                    background: none;
                    border: none;
                    font-size: 1.5rem;
                    cursor: pointer;
                    color: #718096;
                }
                .modal-body {
                    padding: 1.5rem;
                }
                .modal-footer {
                    padding: 1rem 1.5rem;
                    border-top: 1px solid #e2e8f0;
                    display: flex;
                    gap: 1rem;
                    justify-content: flex-end;
                }
            `;
            document.head.appendChild(style);
        }

        // Show modal
        setTimeout(() => modal.classList.add('show'), 10);

        // Close handlers
        const closeModal = () => {
            modal.classList.remove('show');
            setTimeout(() => modal.remove(), 300);
        };

        modal.querySelector('.modal-close').addEventListener('click', closeModal);
        modal.addEventListener('click', (e) => {
            if (e.target === modal) closeModal();
        });

        return {
            close: closeModal,
            element: modal
        };
    }

    // Form Validation
    static validateForm(form) {
        const inputs = form.querySelectorAll('[required]');
        let isValid = true;
        const errors = [];

        inputs.forEach(input => {
            if (!input.value.trim()) {
                isValid = false;
                input.classList.add('error');
                errors.push(`${input.name || input.id} প্রয়োজনীয়`);
            } else {
                input.classList.remove('error');
            }

            // Pattern validation
            if (input.pattern && input.value) {
                const regex = new RegExp(input.pattern);
                if (!regex.test(input.value)) {
                    isValid = false;
                    input.classList.add('error');
                    errors.push(input.title || 'সঠিক ফরম্যাট দিন');
                }
            }
        });

        return { isValid, errors };
    }
}

// Export to window
window.UI = UIComponents;

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function () {
    // Auto-dismiss alerts
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.classList.add('fade-out');
            setTimeout(() => alert.remove(), 300);
        }, 5000);
    });

    // Form validation
    const forms = document.querySelectorAll('form[novalidate]');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const validation = UI.validateForm(this);
            if (!validation.isValid) {
                e.preventDefault();
                UI.showToast(validation.errors.join(', '), 'danger');
            }
        });
    });
});