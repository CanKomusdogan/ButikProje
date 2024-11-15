"use strict";
document.addEventListener('DOMContentLoaded', function () {
    var _a;
    $('#verificationPwdModal').modal('show');
    const codeDigit1 = document.getElementById('pwdCodeDigitFirst');
    const codeDigit2 = document.getElementById('pwdCodeDigitSecond');
    const codeDigit3 = document.getElementById('pwdCodeDigitThird');
    const codeDigit4 = document.getElementById('pwdCodeDigitFourth');
    const codeDigit5 = document.getElementById('pwdCodeDigitFifth');
    const codeDigit6 = document.getElementById('pwdCodeDigitSixth');
    const newPassword = document.getElementById('newPassword');
    const verificationResultDisplay = document.getElementById('pwdVerificationResultDisplay');
    (_a = document.getElementById('pwdVerifyButton')) === null || _a === void 0 ? void 0 : _a.addEventListener('click', function () {
        let verifyCode = parseInt(codeDigit1.value.toString() + codeDigit2.value.toString() + codeDigit3.value.toString() + codeDigit4.value.toString() + codeDigit5.value.toString() + codeDigit6.value.toString());
        $.ajax({
            url: '/Home/VerifyPasswordChangeCode',
            type: 'POST',
            data: { inputVerifyCode: verifyCode, newPassword: newPassword.value },
            success: function (result) {
                if (!result.errOcurred) {
                    if (verificationResultDisplay != null) {
                        verificationResultDisplay.classList.remove('text-danger');
                        verificationResultDisplay.classList.add('text-success');
                        verificationResultDisplay.textContent = result.message;
                    }
                    setTimeout(() => {
                        $('#pwdVerificationModal').modal('hide');
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
    const form = document.querySelector('#pwdEmailVerifyForm');
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
