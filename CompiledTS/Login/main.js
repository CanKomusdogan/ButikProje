"use strict";
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
    var _a, _b, _c;
    (_a = document.getElementById('passwordChangeButton')) === null || _a === void 0 ? void 0 : _a.addEventListener('click', () => {
        changeHash();
    });
    (_b = document.getElementById('passwordChangeClose')) === null || _b === void 0 ? void 0 : _b.addEventListener('click', () => {
        resetHash();
    });
    const urlHash = location.hash;
    if (urlHash === '#passwordchange') {
        const passwordChangeModal = $('#passwordChangeModal');
        passwordChangeModal.removeClass('fade').modal('show').addClass('fade');
    }
    (_c = document.getElementById('pwdShow')) === null || _c === void 0 ? void 0 : _c.addEventListener('change', toggle);
});
const elements = {
    loginPwd: document.getElementById('loginPassword'),
    loginEmail: document.getElementById('loginEmail'),
    checkState: document.getElementById('pwdShow'),
    signUpPwd: document.getElementById('signUpPassword'),
    signUpEmail: document.getElementById('signUpEmail'),
    signUpName: document.getElementById('signUpName'),
    signUpSurname: document.getElementById('signUpLastName'),
    agreesCheckBox: document.getElementById('agrees'),
};
function toggle() {
    elements.loginPwd.type = elements.checkState.checked ? "text" : "password";
}
function validatePassword(password) {
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
    elements[id].addEventListener('input', checkRegisterValidity);
});
elements.agreesCheckBox.addEventListener('change', checkRegisterValidity);
