$(function() {
    $('.mobile_trigger').on('click', function () {
        var parent = $(this).parent();
        
        $(parent).toggleClass('is_active');
        $('ul', parent).slideToggle();
    });
});