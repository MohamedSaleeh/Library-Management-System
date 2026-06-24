// Library Management System - Client-side JavaScript

(function () {
    'use strict';

    // Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const closeBtn = alert.querySelector('.btn-close');
            if (closeBtn) {
                closeBtn.click();
            }
        }, 5000);
    });

    // Confirm delete actions
    const deleteButtons = document.querySelectorAll('form[action*="Delete"] button[type="submit"]');
    deleteButtons.forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
                e.preventDefault();
            }
        });
    });

    // Initialize tooltips
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Set minimum date for due date based on borrow date
    const borrowDateInput = document.getElementById('BorrowDate');
    const dueDateInput = document.getElementById('DueDate');

    if (borrowDateInput && dueDateInput) {
        borrowDateInput.addEventListener('change', function () {
            dueDateInput.min = borrowDateInput.value;
        });
    }
})();
