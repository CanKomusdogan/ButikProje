"use strict";
document.addEventListener('DOMContentLoaded', function () {
    var _a;
    $('#verificationModal').modal('show');
    const codeDigit1 = document.getElementById('codeDigitFirst');
    const codeDigit2 = document.getElementById('codeDigitSecond');
    const codeDigit3 = document.getElementById('codeDigitThird');
    const codeDigit4 = document.getElementById('codeDigitFourth');
    const codeDigit5 = document.getElementById('codeDigitFifth');
    const codeDigit6 = document.getElementById('codeDigitSixth');
    const verificationResultDisplay = document.getElementById('verificationResultDisplay');
    (_a = document.getElementById('verifyButton')) === null || _a === void 0 ? void 0 : _a.addEventListener('click', function () {
        let verifyCode = parseInt(codeDigit1.value.toString() + codeDigit2.value.toString() + codeDigit3.value.toString() + codeDigit4.value.toString() + codeDigit5.value.toString() + codeDigit6.value.toString());
        $.ajax({
            url: '/Home/VerifyEmail',
            type: 'POST',
            data: { inputVerifyCode: verifyCode },
            success: function (result) {
                if (!result.errOcurred) {
                    if (verificationResultDisplay != null) {
                        verificationResultDisplay.classList.remove('text-danger');
                        verificationResultDisplay.classList.add('text-success');
                        verificationResultDisplay.textContent = result.message;
                    }
                    setTimeout(() => {
                        $('#verificationModal').modal('hide');
                    }, 1000);
                }
                else {
                    if (verificationResultDisplay != null) {
                        verificationResultDisplay.classList.remove('text-success');
                        verificationResultDisplay.classList.add('text-danger');
                        verificationResultDisplay.textContent = result.message;
                    }
                }
            },
            error: function (error) {
                console.error(error);
            }
        });
    });
    const form = document.querySelector('#emailVerifyForm');
    const inputs = form.querySelectorAll('.verify-input');
    function handleInput(e) {
        const input = e.target;
        const nextInput = input.nextElementSibling;
        if (nextInput && input.value) {
            nextInput.focus();
            if (nextInput.value) {
                nextInput.select();
            }
        }
    }
    function handlePaste(e) {
        e.preventDefault();
        const paste = e.clipboardData.getData('text');
        inputs.forEach((input, i) => {
            input.value = paste[i] || "";
        });
    }
    function handleBackspace(e) {
        const input = e.target;
        if (input.value) {
            input.value = "";
            return;
        }
        input.previousElementSibling.focus();
    }
    function handleArrowLeft(e) {
        const previousInput = e.target.previousElementSibling;
        if (!previousInput)
            return;
        previousInput.focus();
    }
    function handleArrowRight(e) {
        const nextInput = e.target.nextElementSibling;
        if (!nextInput)
            return;
        nextInput.focus();
    }
    form.addEventListener('input', handleInput);
    inputs[0].addEventListener('paste', handlePaste);
    inputs.forEach((input) => {
        input.addEventListener('focus', (e) => {
            setTimeout(() => {
                e.target.select();
            }, 0);
        });
        input.addEventListener('keydown', (e) => {
            switch (e.key) {
                case "Backspace":
                    handleBackspace(e);
                    break;
                case "ArrowLeft":
                    handleArrowLeft(e);
                    break;
                case "ArrowRight":
                    handleArrowRight(e);
                    break;
                default:
            }
        });
    });
});
