"use strict";
const productAddingSwiper = new Swiper('#productAddingSwiper', {
    spaceBetween: 30,
    loop: true,
    pagination: {
        el: ".swiper-pagination",
        clickable: true,
    },
});
const productEditingSwiper = new Swiper('#productEditingSwiper', {
    spaceBetween: 30,
    loop: true,
    pagination: {
        el: ".swiper-pagination",
        clickable: true,
    },
});
const productViewSwiper = new Swiper('#productViewSwiper', {
    spaceBetween: 30,
    loop: true,
    pagination: {
        el: ".swiper-pagination",
        clickable: true,
    },
});
document.addEventListener('DOMContentLoaded', function () {
    var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k, _l, _m;
    const hash = window.location.hash;
    if (hash) {
        $('.nav-pills a[href="' + hash + '"]').tab('show');
    }
    $.ajax({
        url: '/Admin/ListProductDetails',
        type: 'POST',
        dataType: 'json',
        data: { selectedProductId: $('#productsListBox option').first().val() },
        success: function (response) {
            productViewOldPrice.hidden = true;
            productViewSalePercentage.hidden = true;
            productViewName.textContent = response.selectedProductName;
            productViewInfo.textContent = response.selectedProductInfo;
            productViewPrice.textContent = response.selectedProductPrice;
            if (response.onSale) {
                productViewOldPrice.hidden = false;
                productViewOldPriceBefore.hidden = false;
                productViewSalePercentage.hidden = false;
                productViewOldPrice.textContent = response.selectedProductOldPrice;
                productViewSalePercentage.textContent = "%" + response.selectedProductSalePercentage.toString() + "-";
            }
            else {
                productViewOldPrice.hidden = true;
                productViewOldPriceBefore.hidden = true;
                productViewSalePercentage.hidden = true;
            }
            productViewSwiper.removeAllSlides();
            response.selectedProductPhotos.forEach((photo) => {
                const newImage = document.createElement("img");
                newImage.setAttribute("loading", "lazy");
                newImage.style.width = "261px";
                newImage.style.height = "400px";
                newImage.classList.add("img-fluid", "product-photo");
                newImage.src = photo;
                const newSlide = document.createElement("div");
                newSlide.classList.add("swiper-slide");
                newSlide.appendChild(newImage);
                productViewSwiper.appendSlide(newSlide);
            });
        },
        error: function (error) {
            console.error('Error:', error);
        }
    });
    // The fuck you looking at?
    const productCreateForm = document.getElementById('productCreateForm');
    const productViewName = document.getElementById('productViewName');
    const productViewPrice = document.getElementById('productViewPrice');
    const previewProductName = document.getElementById('previewProductName');
    const previewProductPrice = document.getElementById('previewProductPrice');
    const previewProductOldPrice = document.getElementById('previewProductOldPrice');
    const previewProductOldPriceBefore = document.getElementById('previewProductOldPriceBefore');
    const oldPricePreviewInput = document.getElementById('inputProductOldPricePreview');
    const inputProductNamePreview = document.getElementById('inputProductNamePreview');
    const inputProductPricePreview = document.getElementById('inputProductPricePreview');
    const inputUrlPhotoList = document.getElementById('inputUrlPhotoList');
    const inputProductUrlPhotoPreview = document.getElementById('inputProductUrlPhotoPreview');
    const productViewOldPrice = document.getElementById('productViewOldPrice');
    const productViewOldPriceBefore = document.getElementById('productViewOldPriceBefore');
    const productViewSalePercentage = document.getElementById('productViewSalePercentage');
    const productViewInfo = document.getElementById('productViewInfo');
    const productToRemove = document.getElementById('productToRemove');
    const selectedProductIdConfirmationForm = document.getElementById('selectedProductId');
    const categoryToRemove = document.getElementById('categoryToRemove');
    const selectedCategoryIdConfirmationForm = document.getElementById('selectedCategoryIdConfirmationForm');
    const categoryToEdit = document.getElementById('categoryToEdit');
    const selectedCategoryIdEditForm = document.getElementById('selectedCategoryIdEditForm');
    const newSizeNameInput = document.getElementById('newSizeNameInput');
    const sizeAddingButton = document.getElementById('sizeAddingButton');
    const sizeAddingModalResult = document.getElementById('sizeAddingModalResult');
    const selectedSizeInput = document.getElementById('selectedSize');
    const productsListBox = document.getElementById('productsListBox');
    // Product editing vars
    const productEditForm = document.getElementById('productEditForm');
    const editPreviewProductName = document.getElementById('editPreviewProductName');
    const editPreviewProductPrice = document.getElementById('editPreviewProductPrice');
    const editPreviewProductOldPrice = document.getElementById('editPreviewProductOldPrice');
    const editPreviewProductOldPriceBefore = document.getElementById('editPreviewProductOldPriceBefore');
    const newProductOldPricePreview = document.getElementById('newProductOldPricePreview');
    const newProductNamePreview = document.getElementById('newProductNamePreview');
    const newProductPricePreview = document.getElementById('newProductPricePreview');
    const newUrlPhotoList = document.getElementById('newUrlPhotoList');
    const newProductUrlPhotoPreview = document.getElementById('newProductUrlPhotoPreview');
    const productEditingSalePercentage = document.getElementById('productEditingSalePercentage');
    const newProductOnSale = document.getElementById('newProductOnSale');
    const newDescription = document.getElementById('newDescription');
    const newCategory = document.getElementById('newCategory');
    // Product editing vars
    (_a = document.getElementById('productEditingButton')) === null || _a === void 0 ? void 0 : _a.addEventListener('click', function () {
        fetch('/Admin/ListProductDetails', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ selectedProductId: parseInt(productsListBox.value) })
        })
            .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok ' + response.statusText);
            }
            return response.json();
        })
            .then(data => {
            newProductNamePreview.value = data.selectedProductName;
            editPreviewProductName.textContent = data.selectedProductName;
            newProductPricePreview.value = data.selectedProductPrice;
            editPreviewProductPrice.textContent = data.selectedProductPrice;
            newDescription.value = data.selectedProductInfo;
            newCategory.value = data.selectedProductCategory;
            if (data.onSale) {
                newProductOnSale.checked = true;
                newProductOldPricePreview.value = data.selectedProductOldPrice;
                newProductOldPricePreview.style.display = "block";
                editPreviewProductOldPriceBefore.hidden = false;
                editPreviewProductOldPrice.textContent = data.selectedProductOldPrice;
                editPreviewProductOldPrice.hidden = false;
            }
            else {
                newProductOnSale.checked = false;
                newProductOldPricePreview.value = data.selectedProductOldPrice;
                newProductOldPricePreview.style.display = "none";
                editPreviewProductOldPriceBefore.hidden = true;
                editPreviewProductOldPrice.textContent = data.selectedProductOldPrice;
                editPreviewProductOldPrice.hidden = true;
            }
        })
            .catch(error => {
            console.error('Error:', error);
        });
    });
    function doesProductNameExist(productName) {
        return $.ajax({
            url: '/Admin/DoesProductNameExist',
            type: 'POST',
            dataType: 'json',
            data: { name: productName },
            global: false
        });
    }
    newProductNamePreview.addEventListener('input', function (e) {
        const name = e.target.value.trim();
        if (name) {
            doesProductNameExist(name).done(function (response) {
                if (response.productNameExists) {
                    $('#productEditingNameMessage').text("Bu isim ile başka bir ürün zaten var!");
                    $('#productEditSubmitButton').prop("disabled", true);
                }
                else {
                    $('#productEditingNameMessage').text("");
                    $('#productEditSubmitButton').prop("disabled", false);
                }
            }).fail(function (error) {
                console.error('Error: ', error);
            });
        }
        else {
            $('#productEditingNameMessage').text("");
            $('#productEditSubmitButton').prop("disabled", true);
        }
    });
    inputProductNamePreview.addEventListener('input', function (e) {
        const name = e.target.value.trim();
        if (name) {
            doesProductNameExist(name).done(function (response) {
                if (response.productNameExists) {
                    $('#productCreatingNameMessage').text("Bu isim ile başka bir ürün zaten var!");
                    $('#productCreateSubmitButton').prop("disabled", true);
                }
                else {
                    $('#productCreatingNameMessage').text("");
                    $('#productCreateSubmitButton').prop("disabled", false);
                }
            }).fail(function (error) {
                console.error('Error: ', error);
            });
        }
        else {
            $('#productCreatingNameMessage').text("");
            $('#productCreateSubmitButton').prop("disabled", true);
        }
    });
    (_b = document.getElementById('addImgUrlButton')) === null || _b === void 0 ? void 0 : _b.addEventListener('click', function () {
        const inputProductUrlPhotoPreviewValue = inputProductUrlPhotoPreview.value;
        inputProductUrlPhotoPreview.value = "";
        inputUrlPhotoList.value += inputProductUrlPhotoPreviewValue + "\n";
        const newImage = document.createElement("img");
        newImage.style.width = "261px";
        newImage.style.height = "400px";
        newImage.classList.add("img-fluid", "product-photo");
        newImage.src = inputProductUrlPhotoPreviewValue;
        const newSlide = document.createElement("div");
        newSlide.classList.add("swiper-slide");
        newSlide.appendChild(newImage);
        productAddingSwiper.appendSlide(newSlide);
    });
    (_c = document.getElementById('addNewImgUrlButton')) === null || _c === void 0 ? void 0 : _c.addEventListener('click', function () {
        const inputProductUrlPhotoPreviewValue = newProductUrlPhotoPreview.value;
        newProductUrlPhotoPreview.value = "";
        newUrlPhotoList.value += inputProductUrlPhotoPreviewValue + "\n";
        const newImage = document.createElement("img");
        newImage.style.width = "261px";
        newImage.style.height = "400px";
        newImage.classList.add("img-fluid", "product-photo");
        newImage.src = inputProductUrlPhotoPreviewValue;
        const newSlide = document.createElement("div");
        newSlide.classList.add("swiper-slide");
        newSlide.appendChild(newImage);
        productEditingSwiper.appendSlide(newSlide);
    });
    (_d = document.getElementById("clearFilesButton")) === null || _d === void 0 ? void 0 : _d.addEventListener("click", function () {
        var _a;
        const fileInput = document.getElementById('inputProductPhotoPreview');
        let newFileInput = document.createElement('input');
        newFileInput.id = fileInput.id;
        newFileInput.name = fileInput.name;
        newFileInput.type = fileInput.type;
        newFileInput.classList.add(fileInput.classList.value);
        newFileInput.accept = fileInput.accept;
        newFileInput.multiple = fileInput.multiple;
        newFileInput.formNoValidate = fileInput.formNoValidate;
        inputUrlPhotoList.value = "";
        (_a = fileInput.parentNode) === null || _a === void 0 ? void 0 : _a.replaceChild(newFileInput, fileInput);
        productAddingSwiper.removeAllSlides();
    });
    (_e = document.getElementById('clearNewFilesButton')) === null || _e === void 0 ? void 0 : _e.addEventListener('click', function () {
        var _a;
        const fileInput = document.getElementById('newProductPhotoPreview');
        let newFileInput = document.createElement('input');
        newFileInput.id = fileInput.id;
        newFileInput.name = fileInput.name;
        newFileInput.type = fileInput.type;
        newFileInput.classList.add(fileInput.classList.value);
        newFileInput.accept = fileInput.accept;
        newFileInput.multiple = fileInput.multiple;
        newFileInput.formNoValidate = fileInput.formNoValidate;
        newUrlPhotoList.value = "";
        (_a = fileInput.parentNode) === null || _a === void 0 ? void 0 : _a.replaceChild(newFileInput, fileInput);
        productEditingSwiper.removeAllSlides();
    });
    (_f = document.getElementById("inputProductPhotoPreview")) === null || _f === void 0 ? void 0 : _f.addEventListener("change", function (event) {
        const files = event.target.files;
        if (files != null) {
            for (let i = 0; i < files.length; i++) {
                const reader = new FileReader();
                reader.onload = (function (file) {
                    return function (e) {
                        var _a;
                        const newImage = document.createElement("img");
                        newImage.style.width = "261px";
                        newImage.style.height = "400px";
                        newImage.classList.add("img-fluid", "product-photo");
                        newImage.src = (_a = e.target) === null || _a === void 0 ? void 0 : _a.result;
                        const newSlide = document.createElement("div");
                        newSlide.classList.add("swiper-slide");
                        newSlide.appendChild(newImage);
                        productAddingSwiper.appendSlide(newSlide);
                    };
                })(files[i]);
                reader.readAsDataURL(files[i]);
            }
        }
    });
    (_g = document.getElementById('newProductPhotoPreview')) === null || _g === void 0 ? void 0 : _g.addEventListener('change', function (event) {
        const files = event.target.files;
        if (files != null) {
            for (let i = 0; i < files.length; i++) {
                const reader = new FileReader();
                reader.onload = (function (file) {
                    return function (e) {
                        var _a;
                        const newImage = document.createElement("img");
                        newImage.style.width = "261px";
                        newImage.style.height = "400px";
                        newImage.classList.add("img-fluid", "product-photo");
                        newImage.src = (_a = e.target) === null || _a === void 0 ? void 0 : _a.result;
                        const newSlide = document.createElement("div");
                        newSlide.classList.add("swiper-slide");
                        newSlide.appendChild(newImage);
                        productEditingSwiper.appendSlide(newSlide);
                    };
                })(files[i]);
                reader.readAsDataURL(files[i]);
            }
        }
    });
    (_h = document.getElementById('inputProductPhotoPreview')) === null || _h === void 0 ? void 0 : _h.addEventListener('click', function () {
        productAddingSwiper.removeAllSlides();
        productEditingSwiper.removeAllSlides();
        productViewSwiper.removeAllSlides();
    });
    (_j = document.getElementById('newProductPhotoPreview')) === null || _j === void 0 ? void 0 : _j.addEventListener('click', function () {
        productAddingSwiper.removeAllSlides();
        productEditingSwiper.removeAllSlides();
        productViewSwiper.removeAllSlides();
    });
    inputProductNamePreview.addEventListener('input', function () {
        previewProductName.textContent = inputProductNamePreview.value;
    });
    newProductNamePreview.addEventListener('input', function () {
        editPreviewProductName.textContent = newProductNamePreview.value;
    });
    inputProductPricePreview.addEventListener('input', function () {
        previewProductPrice.textContent = inputProductPricePreview.value;
    });
    newProductPricePreview.addEventListener('input', function () {
        editPreviewProductPrice.textContent = newProductPricePreview.value;
    });
    checkProductCreateValidity();
    checkProductEditValidity();
    inputProductNamePreview.addEventListener('input', function () {
        checkProductCreateValidity();
    });
    inputProductPricePreview.addEventListener('input', function () {
        checkProductCreateValidity();
    });
    function checkProductCreateValidity() {
        if (inputProductNamePreview.checkValidity() && inputProductPricePreview.checkValidity()) {
            $('#productCreateSubmitButton').prop('disabled', false);
        }
        else {
            $('#productCreateSubmitButton').prop('disabled', true);
        }
    }
    newProductNamePreview.addEventListener('input', function () {
        checkProductEditValidity();
    });
    newProductPricePreview.addEventListener('input', function () {
        checkProductEditValidity();
    });
    function checkProductEditValidity() {
        if (newProductNamePreview.checkValidity() && newProductPricePreview.checkValidity()) {
            $('#productEditSubmitButton').prop('disabled', false);
        }
        else {
            $('#productEditSubmitButton').prop('disabled', true);
        }
    }
    document.getElementById('inputProductOnSale').addEventListener('change', function () {
        if (this.checked) {
            oldPricePreviewInput.style.display = "block";
            previewProductOldPriceBefore.hidden = false;
            previewProductOldPrice.hidden = false;
        }
        else {
            oldPricePreviewInput.style.display = "none";
            previewProductOldPriceBefore.hidden = true;
            previewProductOldPrice.hidden = true;
        }
    });
    newProductOnSale.addEventListener('change', function () {
        if (this.checked) {
            newProductOldPricePreview.style.display = "block";
            editPreviewProductOldPriceBefore.hidden = false;
            editPreviewProductOldPrice.hidden = false;
        }
        else {
            newProductOldPricePreview.style.display = "none";
            editPreviewProductOldPriceBefore.hidden = true;
            editPreviewProductOldPrice.hidden = true;
        }
    });
    oldPricePreviewInput.addEventListener('input', function () {
        previewProductOldPrice.textContent = this.value;
    });
    newProductOldPricePreview.addEventListener('input', function () {
        editPreviewProductOldPrice.textContent = this.value;
    });
    (_k = document.getElementById('productDeletingButton')) === null || _k === void 0 ? void 0 : _k.addEventListener('click', function () {
        productToRemove.textContent = $('#productsListBox').select2('data')[0].text;
        selectedProductIdConfirmationForm.value = $('#productsListBox').select2('data')[0].id;
    });
    (_l = document.getElementById('editCategoryButton')) === null || _l === void 0 ? void 0 : _l.addEventListener('click', function () {
        categoryToEdit.textContent = $('#categoriesListBox').select2('data')[0].text;
        selectedCategoryIdEditForm.value = $('#categoriesListBox').select2('data')[0].id;
    });
    (_m = document.getElementById('removeCategoryButton')) === null || _m === void 0 ? void 0 : _m.addEventListener('click', function () {
        categoryToRemove.textContent = $('#categoriesListBox').select2('data')[0].text;
        selectedCategoryIdConfirmationForm.value = $('#categoriesListBox').select2('data')[0].id;
    });
    $('#productsListBox').on('select2:select', function (e) {
        $.ajax({
            url: '/Admin/ListProductDetails',
            type: 'POST',
            dataType: 'json',
            data: { selectedProductId: parseInt(e.params.data.id) },
            success: function (response) {
                productViewOldPrice.hidden = true;
                productViewSalePercentage.hidden = true;
                productViewName.textContent = response.selectedProductName;
                productViewPrice.textContent = response.selectedProductPrice;
                if (response.onSale) {
                    productViewOldPrice.hidden = false;
                    productViewOldPriceBefore.hidden = false;
                    productViewSalePercentage.hidden = false;
                    productViewOldPrice.textContent = response.selectedProductOldPrice;
                    productViewSalePercentage.textContent = "%" + response.selectedProductSalePercentage.toString() + "-";
                }
                else {
                    productViewOldPrice.hidden = true;
                    productViewOldPriceBefore.hidden = true;
                    productViewSalePercentage.hidden = true;
                }
                productViewSwiper.removeAllSlides();
                response.selectedProductPhotos.forEach((photo) => {
                    const newImage = document.createElement("img");
                    newImage.setAttribute("loading", "lazy");
                    newImage.style.width = "261px";
                    newImage.style.height = "400px";
                    newImage.classList.add("img-fluid", "product-photo");
                    newImage.src = photo;
                    const newSlide = document.createElement("div");
                    newSlide.classList.add("swiper-slide");
                    newSlide.appendChild(newImage);
                    productViewSwiper.appendSlide(newSlide);
                });
            },
            error: function (error) {
                console.error('Error:', error);
            }
        });
    });
    sizeAddingButton.addEventListener('click', () => {
        fetch('/Admin/AddNewSize', {
            method: 'POST',
            body: JSON.stringify({
                name: newSizeNameInput.value
            }),
            headers: {
                "Content-type": "application/json; charset=UTF-8"
            },
        })
            .then(response => response.json())
            .then(json => {
            if (json.success) {
                sizeAddingModalResult.classList.add('text-success');
                sizeAddingModalResult.textContent = "Yeni beden başarı ile eklendi!";
            }
            else {
                if (!sizeAddingModalResult.classList.contains('text-success')) {
                    sizeAddingModalResult.classList.add('text-danger');
                    sizeAddingModalResult.textContent = "Yeni beden oluşturulamadı, bu hata ile tekrar karşılaşırsanız yazılımcı ile iletişime geçiniz.";
                    if (json.message) {
                        console.error(json.message);
                    }
                }
                else {
                    sizeAddingModalResult.classList.remove('text-success');
                    sizeAddingModalResult.classList.add('text-danger');
                    sizeAddingModalResult.textContent = "Yeni beden oluşturulamadı, bu hata ile tekrar karşılaşırsanız yazılımcı ile iletişime geçiniz.";
                    if (json.message) {
                        console.error(json.message);
                    }
                }
            }
        })
            .catch(error => {
            console.error("Network or server error:", error);
            sizeAddingModalResult.classList.add('text-danger');
            sizeAddingModalResult.textContent = "Bir hata oluştu, lütfen daha sonra tekrar deneyin.";
        });
    });
});
const productsListBox = document.getElementById('productsListBox');
const selectedProductId = parseInt(productsListBox.value);
$("[name='selectedProductId']").val(selectedProductId);
$('#productsListBox').on('select2:select', function (e) {
    const selectedProductId = parseInt(e.params.data.id);
    $("[name='selectedProductId']").val(selectedProductId);
});
function editModal(button) {
    if (button.value === "cbc") {
        $.get('/Admin/GetBannerContentForm', function (data) {
            $('#modalBody').html(data);
        });
        const firstInput = document.getElementById("bannerTitleEditor");
        const secondInput = document.getElementById("bannerAltTitleEditor");
        firstInput.addEventListener("keydown", function () {
            if (firstInput.value != null && firstInput.value != "") {
                secondInput.removeAttribute("required");
            }
            else {
                secondInput.setAttribute("required", "required");
            }
        });
        secondInput.addEventListener("keydown", function () {
            if (secondInput.value !== null && secondInput.value !== "") {
                firstInput.removeAttribute("required");
            }
            else {
                firstInput.setAttribute("required", "required");
            }
        });
    }
    else if (button.value === "cfc") {
        $.get('/Admin/GetFooterContentForm', function (data) {
            $('#modalBody').html(data);
        });
    }
}
// What? You and i both know that React is dogshit. Blame it on the language that was created in 10 days, not me.
