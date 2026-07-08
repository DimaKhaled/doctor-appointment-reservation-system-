// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Reusable confirmation modal handler.
// Usage: add class "js-confirm-submit" and data-confirm-message="..." to a submit button inside a <form>.
document.addEventListener("DOMContentLoaded", function () {
    var loginToastEl = document.getElementById("loginToast");
    if (loginToastEl) {
        var loginToast = new bootstrap.Toast(loginToastEl);
        loginToast.show();
    }

    var confirmModalEl = document.getElementById("confirmModal");
    if (!confirmModalEl) {
        return;
    }

    var confirmModal = new bootstrap.Modal(confirmModalEl);
    var messageEl = document.getElementById("confirmModalMessage");
    var acceptButton = document.getElementById("confirmModalAcceptButton");
    var pendingForm = null;

    document.querySelectorAll(".js-confirm-submit").forEach(function (button) {
        button.addEventListener("click", function (event) {
            event.preventDefault();
            pendingForm = button.closest("form");
            messageEl.textContent = button.getAttribute("data-confirm-message") || "Are you sure?";
            confirmModal.show();
        });
    });

    acceptButton.addEventListener("click", function () {
        confirmModal.hide();
        if (pendingForm) {
            pendingForm.requestSubmit();
        }
    });
});
