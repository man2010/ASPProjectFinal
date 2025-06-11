// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

jQuery(document).ready(function ($) {
    // DataTables Initialization
    $('.table').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/fr-FR.json'
        },
        paging: true,
        searching: true,
        ordering: true,
        pageLength: 10
    });

    // Dynamic Order Details
    $('#addDetail').on('click', function () {
        var detail = $('.order-detail:first').clone(true, true);
        detail.find('input').val('');
        detail.find('select').val('');
        detail.appendTo('#orderDetails').addClass('animate-fade-in');
    });

    $(document).on('click', '.remove-detail', function () {
        if ($('.order-detail').length > 1) {
            $(this).closest('.order-detail').remove();
        }
    });

    // Form Validation for Negative Numbers
    $('input[type="number"]').on('input', function () {
        if (parseFloat($(this).val()) < 0) {
            $(this).val('');
        }
    });
});