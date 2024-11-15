document.addEventListener('DOMContentLoaded', function () {
    const addToCartBut = document.getElementById('addToCartBut');
    const cartItemCount = document.getElementById('cartItemCount');

    addToCartBut.addEventListener('click', addToCart);

    addToCartBut.addEventListener('mouseenter', resetButton);

    const productId = document.getElementById('pid').value;

    function addToCart() {
        $.ajax({
            url: '/Home/AddToCart',
            type: 'GET',
            dataType: 'json',
            data: { productId: productId },
            success: function (data) {
                cartItemCount.textContent = JSON.parse(data.cartCount);
                updateButton("Sepete eklendi!", "bg-success", "bg-black");
            },
            error: function (error) {
                console.error("Error adding item to cart:", error.message || error);
            }
        });
    }

    function resetButton() {
        if (addToCartBut.classList.contains('bg-success')) {
            updateButton("Sepete ekle", "bg-black", "bg-success");
        }
    }

    function updateButton(text, addClass, removeClass) {
        addToCartBut.textContent = text;
        addToCartBut.classList.remove(removeClass);
        addToCartBut.classList.add(addClass);
    }
});