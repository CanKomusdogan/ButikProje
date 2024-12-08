declare var Swiper: any;


const swiper = new Swiper('.swiper-all', {
    loop: true,
    spaceBetween: 30,
    pagination: {
        el: ".swiper-pagination",
        clickable: true,
    },
    touchEventsTarget: 'container',
});
const swiperProduct = new Swiper('.swiper-product', {
    loop: true,
    spaceBetween: 30,
    navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
    },
    touchEventsTarget: 'container',
});

const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

(() => {
    'use strict'

    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    const forms = document.querySelectorAll('.needs-validation') as NodeListOf<HTMLFormElement>;

    // Loop over them and prevent submission
    Array.from(forms).forEach((form) => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }

            form.classList.add('was-validated');
        }, false);
    });
})();

document.addEventListener('DOMContentLoaded', function () {
    $('.select-2').select2({
        width: '300px'
    });

    const searchSubmit = document.getElementById('searchSubmit');
    let searchInputValid = false;

    $('#searchInput').on('input', function (e) {
        const input = (e.target as HTMLInputElement).value.trim();
        
        if (input) {
            if ($('#searchSubmit').hasClass("disabled") && $('#searchSubmit').hasClass("border-0")) {
                searchInputValid = true;
                $('#searchSubmit').removeClass("disabled");
                $('#searchSubmit').removeClass("border-0")
            }
        } else if (!$('#searchSubmit').hasClass("disabled") && !$('#searchSubmit').hasClass("border-0")) {
            searchInputValid = false;
            $('#searchSubmit').addClass("disabled")
            $('#searchSubmit').addClass("border-0")
        }
    });

    $('#searchForm').on('submit', function (e) {
        if (!searchInputValid) {
            e.preventDefault();
        }
    });

    $(document).ajaxStart(function () {
        $('#loadingScreen').css('display', 'flex');
    });

    $(document).ajaxStop(function () {
        $('#loadingScreen').css('display', 'none');
    });

    $('#signout').on('click', () => {
        $.ajax({
            url: '/Home/SignOut',
            type: 'POST',
            success: function (response) {
                if (response.success) {
                    $('#userDropdown').remove();
                    $('#navbarSupportedContent').append('<a href="/Home/Login" class="btn"> <i class="fa-solid fa-arrow-right-to-bracket me-2"></i> Giriş yap</a>');

                    $('#cartItemCount').text("0");

                    $('#multiModalText').text("Çıkış yapıldı.");
                    $('#multiModal').modal('show');
                } else {
                    $('#multiModalText').text("Çıkış yapılamadı.\n" + response.message);
                    $('#multiModal').modal('show');
                }
            },
            error: function (errorThrown) {
                $('#multiModalText').text("Çıkış yapılamadı.\n" + errorThrown);
                console.error(errorThrown);
            }
        });
    });

});