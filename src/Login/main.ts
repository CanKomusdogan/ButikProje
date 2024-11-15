function changeHash() {
    window.location.hash = '';
    setTimeout(() => {
        window.location.hash = 'passwordchange';
    }, 0);
}
function resetHash() {
    window.location.hash = '';
}

document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('passwordChangeButton')?.addEventListener('click', () => {
        changeHash();
    });
    document.getElementById('passwordChangeClose')?.addEventListener('click', () => {
        resetHash();
    });

    const urlHash = location.hash;

    if (urlHash === '#passwordchange') {
        const passwordChangeModal = $('#passwordChangeModal');
        passwordChangeModal.removeClass('fade').modal('show').addClass('fade');
    }
});

const elements = {
    loginPwd: document.getElementById('loginPassword') as HTMLInputElement,
    loginEmail: document.getElementById('loginEmail') as HTMLInputElement,
    checkState: document.getElementById('pwdShow') as HTMLInputElement,
    signUpPwd: document.getElementById('signUpPassword') as HTMLInputElement,
    signUpEmail: document.getElementById('signUpEmail') as HTMLInputElement,
    signUpName: document.getElementById('signUpName') as HTMLInputElement,
    signUpSurname: document.getElementById('signUpLastName') as HTMLInputElement,
    agreesCheckBox: document.getElementById('agrees') as HTMLInputElement,
};

function toggle() {
    elements.loginPwd.type = elements.checkState.checked ? "text" : "password";
}

function validatePassword(password: string) {
    const minLength = 8;
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasDigit = /\d/.test(password);

    return password.length >= minLength && hasUpperCase && hasLowerCase && hasDigit;
}

function checkLoginValidity() {
    const isValid = elements.loginPwd.checkValidity() && elements.loginEmail.checkValidity() && validatePassword(elements.loginPwd.value);
    $('#loginButton').prop('disabled', !isValid);
}

function checkRegisterValidity() {
    const { signUpPwd, signUpEmail, signUpName, signUpSurname, agreesCheckBox } = elements;
    const isValid = [signUpPwd, signUpEmail, signUpName, signUpSurname, agreesCheckBox].every(el => el.checkValidity()) && validatePassword(signUpPwd.value);
    $('#registerButton').prop('disabled', !isValid);
}

[elements.loginPwd, elements.loginEmail].forEach(el => el.addEventListener('input', checkLoginValidity));

['signUpPwd', 'signUpEmail', 'signUpName', 'signUpSurname', 'agreesCheckBox'].forEach(id => {
    elements[id as keyof typeof elements].addEventListener('input', checkRegisterValidity);
});

elements.agreesCheckBox.addEventListener('change', checkRegisterValidity);
