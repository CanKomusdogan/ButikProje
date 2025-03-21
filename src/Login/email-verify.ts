﻿document.addEventListener('DOMContentLoaded', function () {
    $('#verificationModal').modal('show');

    const codeDigit1 = document.getElementById('codeDigitFirst') as HTMLInputElement;
    const codeDigit2 = document.getElementById('codeDigitSecond') as HTMLInputElement;
    const codeDigit3 = document.getElementById('codeDigitThird') as HTMLInputElement;
    const codeDigit4 = document.getElementById('codeDigitFourth') as HTMLInputElement;
    const codeDigit5 = document.getElementById('codeDigitFifth') as HTMLInputElement;
    const codeDigit6 = document.getElementById('codeDigitSixth') as HTMLInputElement;

    const verificationResultDisplay = document.getElementById('verificationResultDisplay');

    document.getElementById('verifyButton')?.addEventListener('click', function () {

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
                } else {
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

    const form = document.querySelector('#emailVerifyForm') as HTMLFormElement;
    const inputs = form.querySelectorAll('.verify-input') as NodeListOf<HTMLInputElement>;

    function handleInput(e: any) {
        const input = e.target as HTMLInputElement;
        const nextInput = input.nextElementSibling as HTMLInputElement;
        if (nextInput && input.value) {
            nextInput.focus();
            if (nextInput.value) {
                nextInput.select();
            }
        }
    }

    function handlePaste(e: any) {
        e.preventDefault();
        const paste = e.clipboardData.getData('text');
        inputs.forEach((input, i) => {
            input.value = paste[i] || "";
        });
    }

    function handleBackspace(e: KeyboardEvent) {
        const input = e.target as HTMLInputElement;
        if (input.value) {
            input.value = "";
            return;
        }

        (input.previousElementSibling as HTMLInputElement).focus();
    }

    function handleArrowLeft(e: KeyboardEvent) {
        const previousInput = (e.target as Element).previousElementSibling as HTMLInputElement;
        if (!previousInput) return;
        previousInput.focus();
    }

    function handleArrowRight(e: KeyboardEvent) {
        const nextInput = (e.target as Element).nextElementSibling as HTMLInputElement;
        if (!nextInput) return;
        nextInput.focus();
    }

    form.addEventListener('input', handleInput);
    inputs[0].addEventListener('paste', handlePaste);

    inputs.forEach((input) => {
        input.addEventListener('focus', (e) => {
            setTimeout(() => {
                (e.target as HTMLInputElement).select();
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