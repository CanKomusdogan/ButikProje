document.addEventListener('DOMContentLoaded', function () {
    $('#cartItems').on('click', '.removeCartItem', function () {
        var cartItemId = $(this).data('item-id');

        $.ajax({
            type: 'POST',
            url: '/Home/RemoveCartItem',
            dataType: 'json',
            data: { itemId: cartItemId },
            success: function (data) {
                if (!data.success) {
                    console.error(data.message);
                    return;
                }

                updateTotalPrice(data.newTotalPrice);

                if (!data.quantityOver1) {
                    removeCartItem(data.removedItemId);
                } else {
                    updateItemCount(data.removedItemId);
                }
            },
            error: function (error) {
                console.error("Error removing cart item:", error);
            }
        });
    });

    function updateTotalPrice(newPrice) {
        $('#totalPrice').text(newPrice);
    }

    function removeCartItem(itemId) {
        $('#item-' + itemId).remove();
    }

    function updateItemCount(itemId) {
        let itemCountElement = $('#itemCount-' + itemId);
        let itemCount = parseInt(itemCountElement.text(), 10);
        itemCountElement.text((itemCount - 1).toString());
    }
});