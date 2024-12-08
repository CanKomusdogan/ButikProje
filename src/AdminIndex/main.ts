declare var Swiper: any;


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
            } else {
                productViewOldPrice.hidden = true;
                productViewOldPriceBefore.hidden = true;
                productViewSalePercentage.hidden = true;
            }

            productViewSwiper.removeAllSlides();
            (response.selectedProductPhotos as string[]).forEach((photo) => {
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
    const productCreateForm = document.getElementById('productCreateForm') as HTMLFormElement;
    const productViewName = document.getElementById('productViewName') as HTMLHeadingElement;
    const productViewPrice = document.getElementById('productViewPrice') as HTMLSpanElement;
    const previewProductName = document.getElementById('previewProductName') as HTMLHeadingElement;
    const previewProductPrice = document.getElementById('previewProductPrice') as HTMLSpanElement;
    const previewProductOldPrice = document.getElementById('previewProductOldPrice') as HTMLSpanElement;
    const previewProductOldPriceBefore = document.getElementById('previewProductOldPriceBefore') as HTMLSpanElement;
    const oldPricePreviewInput = document.getElementById('inputProductOldPricePreview') as HTMLInputElement;
    const inputProductNamePreview = document.getElementById('inputProductNamePreview') as HTMLInputElement;
    const inputProductPricePreview = document.getElementById('inputProductPricePreview') as HTMLInputElement;
    const inputUrlPhotoList = document.getElementById('inputUrlPhotoList') as HTMLTextAreaElement;
    const inputProductUrlPhotoPreview = document.getElementById('inputProductUrlPhotoPreview') as HTMLInputElement;
    const productViewOldPrice = document.getElementById('productViewOldPrice') as HTMLSpanElement;
    const productViewOldPriceBefore = document.getElementById('productViewOldPriceBefore') as HTMLSpanElement;
    const productViewSalePercentage = document.getElementById('productViewSalePercentage') as HTMLSpanElement;
    const productViewInfo = document.getElementById('productViewInfo') as HTMLLIElement;
    const productToRemove = document.getElementById('productToRemove') as HTMLSpanElement;
    const selectedProductIdConfirmationForm = document.getElementById('selectedProductId') as HTMLInputElement;
    const categoryToRemove = document.getElementById('categoryToRemove') as HTMLSpanElement;
    const selectedCategoryIdConfirmationForm = document.getElementById('selectedCategoryIdConfirmationForm') as HTMLInputElement;
    const categoryToEdit = document.getElementById('categoryToEdit') as HTMLSpanElement;
    const selectedCategoryIdEditForm = document.getElementById('selectedCategoryIdEditForm') as HTMLInputElement;
    const newSizeNameInput = document.getElementById('newSizeNameInput') as HTMLInputElement;
    const sizeAddingButton = document.getElementById('sizeAddingButton') as HTMLButtonElement;
    const sizeAddingModalResult = document.getElementById('sizeAddingModalResult') as HTMLSpanElement;
    const selectedSizeInput = document.getElementById('selectedSize') as HTMLInputElement;
    const productsListBox = document.getElementById('productsListBox') as HTMLInputElement;

    // Product editing vars
    const productEditForm = document.getElementById('productEditForm') as HTMLFormElement;
    const editPreviewProductName = document.getElementById('editPreviewProductName') as HTMLHeadingElement;
    const editPreviewProductPrice = document.getElementById('editPreviewProductPrice') as HTMLSpanElement;
    const editPreviewProductOldPrice = document.getElementById('editPreviewProductOldPrice') as HTMLSpanElement;
    const editPreviewProductOldPriceBefore = document.getElementById('editPreviewProductOldPriceBefore') as HTMLSpanElement;
    const newProductOldPricePreview = document.getElementById('newProductOldPricePreview') as HTMLInputElement;
    const newProductNamePreview = document.getElementById('newProductNamePreview') as HTMLInputElement;
    const newProductPricePreview = document.getElementById('newProductPricePreview') as HTMLInputElement;
    const newUrlPhotoList = document.getElementById('newUrlPhotoList') as HTMLTextAreaElement;
    const newProductUrlPhotoPreview = document.getElementById('newProductUrlPhotoPreview') as HTMLInputElement;
    const productEditingSalePercentage = document.getElementById('productEditingSalePercentage') as HTMLSpanElement;
    const newProductOnSale = document.getElementById('newProductOnSale') as HTMLInputElement;
    const newDescription = document.getElementById('newDescription') as HTMLTextAreaElement;
    const newCategory = document.getElementById('newCategory') as HTMLInputElement;
    // Product editing vars

    document.getElementById('productEditingButton')?.addEventListener('click', function () {
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
                } else {
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

    function doesProductNameExist(productName: string) {
        return $.ajax({
            url: '/Admin/DoesProductNameExist',
            type: 'POST',
            dataType: 'json',
            data: { name: productName },
            global: false
        });
    }

    newProductNamePreview.addEventListener('input', function (e) {
        const name = (e.target as HTMLInputElement).value.trim();
        if (name) {
            doesProductNameExist(name).done(function (response) {
                if (response.productNameExists) {
                    $('#productEditingNameMessage').text("Bu isim ile başka bir ürün zaten var!");
                    $('#productEditSubmitButton').prop("disabled", true);
                } else {
                    $('#productEditingNameMessage').text("");
                    $('#productEditSubmitButton').prop("disabled", false);
                }
            }).fail(function (error) {
                console.error('Error: ', error);
            });
        } else {
            $('#productEditingNameMessage').text("");
            $('#productEditSubmitButton').prop("disabled", true);
        }
    });

    inputProductNamePreview.addEventListener('input', function (e) {
        const name = (e.target as HTMLInputElement).value.trim();
        if (name) {
            doesProductNameExist(name).done(function (response) {
                if (response.productNameExists) {
                    $('#productCreatingNameMessage').text("Bu isim ile başka bir ürün zaten var!");
                    $('#productCreateSubmitButton').prop("disabled", true);
                } else {
                    $('#productCreatingNameMessage').text("");
                    $('#productCreateSubmitButton').prop("disabled", false);
                }
            }).fail(function (error) {
                console.error('Error: ', error);
            });
        } else {
            $('#productCreatingNameMessage').text("");
            $('#productCreateSubmitButton').prop("disabled", true);
        }
    });


    document.getElementById('addImgUrlButton')?.addEventListener('click', function () {
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
    document.getElementById('addNewImgUrlButton')?.addEventListener('click', function () {
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

    document.getElementById("clearFilesButton")?.addEventListener("click", function () {
        const fileInput = document.getElementById('inputProductPhotoPreview') as HTMLInputElement;

        let newFileInput = document.createElement('input');
        newFileInput.id = fileInput.id;
        newFileInput.name = fileInput.name;
        newFileInput.type = fileInput.type;
        newFileInput.classList.add(fileInput.classList.value);
        newFileInput.accept = fileInput.accept;
        newFileInput.multiple = fileInput.multiple;
        newFileInput.formNoValidate = fileInput.formNoValidate;

        inputUrlPhotoList.value = "";
        fileInput.parentNode?.replaceChild(newFileInput, fileInput);
        productAddingSwiper.removeAllSlides()
    });
    document.getElementById('clearNewFilesButton')?.addEventListener('click', function () {
        const fileInput = document.getElementById('newProductPhotoPreview') as HTMLInputElement;

        let newFileInput = document.createElement('input');
        newFileInput.id = fileInput.id;
        newFileInput.name = fileInput.name;
        newFileInput.type = fileInput.type;
        newFileInput.classList.add(fileInput.classList.value);
        newFileInput.accept = fileInput.accept;
        newFileInput.multiple = fileInput.multiple;
        newFileInput.formNoValidate = fileInput.formNoValidate;

        newUrlPhotoList.value = "";
        fileInput.parentNode?.replaceChild(newFileInput, fileInput);
        productEditingSwiper.removeAllSlides()
    });

    document.getElementById("inputProductPhotoPreview")?.addEventListener("change", function (event) {

        const files = (event.target as HTMLInputElement).files;
        if (files != null) {
            for (let i = 0; i < files.length; i++) {
                const reader = new FileReader();

                reader.onload = (function (file) {
                    return function (e) {

                        const newImage = document.createElement("img");
                        newImage.style.width = "261px";
                        newImage.style.height = "400px";
                        newImage.classList.add("img-fluid", "product-photo");
                        newImage.src = e.target?.result as string;

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
    document.getElementById('newProductPhotoPreview')?.addEventListener('change', function (event) {
        const files = (event.target as HTMLInputElement).files;

        if (files != null) {
            for (let i = 0; i < files.length; i++) {
                const reader = new FileReader();

                reader.onload = (function (file) {
                    return function (e) {

                        const newImage = document.createElement("img");
                        newImage.style.width = "261px";
                        newImage.style.height = "400px";
                        newImage.classList.add("img-fluid", "product-photo");
                        newImage.src = e.target?.result as string;

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
    document.getElementById('inputProductPhotoPreview')?.addEventListener('click', function () {
        productAddingSwiper.removeAllSlides();
        productEditingSwiper.removeAllSlides();
        productViewSwiper.removeAllSlides();
    });
    document.getElementById('newProductPhotoPreview')?.addEventListener('click', function () {
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
        } else {
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
        } else {
            $('#productEditSubmitButton').prop('disabled', true);
        }
    }

    (document.getElementById('inputProductOnSale') as HTMLInputElement).addEventListener('change', function () {
        if (this.checked) {
            oldPricePreviewInput.style.display = "block";
            previewProductOldPriceBefore.hidden = false;
            previewProductOldPrice.hidden = false;
        } else {
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
        } else {
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

    document.getElementById('productDeletingButton')?.addEventListener('click', function () {
        productToRemove.textContent = $('#productsListBox').select2('data')[0].text;
        selectedProductIdConfirmationForm.value = $('#productsListBox').select2('data')[0].id;
    });

    document.getElementById('editCategoryButton')?.addEventListener('click', function () {
        categoryToEdit.textContent = $('#categoriesListBox').select2('data')[0].text;
        selectedCategoryIdEditForm.value = $('#categoriesListBox').select2('data')[0].id;
    });

    document.getElementById('removeCategoryButton')?.addEventListener('click', function () {
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
                (response.selectedProductPhotos as string[]).forEach((photo) => {
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
                } else {
                    if (!sizeAddingModalResult.classList.contains('text-success')) {
                        sizeAddingModalResult.classList.add('text-danger');
                        sizeAddingModalResult.textContent = "Yeni beden oluşturulamadı, bu hata ile tekrar karşılaşırsanız yazılımcı ile iletişime geçiniz.";

                        if (json.message) {
                            console.error(json.message);
                        }
                    } else {
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
const productsListBox = document.getElementById('productsListBox') as HTMLInputElement;
const selectedProductId = parseInt(productsListBox.value);
$("[name='selectedProductId']").val(selectedProductId);

$('#productsListBox').on('select2:select', function (e) {
    const selectedProductId = parseInt(e.params.data.id);
    $("[name='selectedProductId']").val(selectedProductId);
});

function editModal(button: HTMLButtonElement) {
    if (button.value === "cbc") {
        $.get('/Admin/GetBannerContentForm', function (data) {
            $('#modalBody').html(data);
        });

        const firstInput = document.getElementById("bannerTitleEditor") as HTMLInputElement;
        const secondInput = document.getElementById("bannerAltTitleEditor") as HTMLInputElement;

        firstInput.addEventListener("keydown", function () {
            if (firstInput.value != null && firstInput.value != "") {
                secondInput.removeAttribute("required");
            } else {
                secondInput.setAttribute("required", "required");
            }
        });
        secondInput.addEventListener("keydown", function () {
            if (secondInput.value !== null && secondInput.value !== "") {
                firstInput.removeAttribute("required");
            } else {
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