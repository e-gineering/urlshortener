
const setupMessage = document.getElementById('setup-message');
const setupForm = document.getElementById('setup-form');
const keyInput = document.getElementById('key-input');

// Initialize MDC Text Field
const textField = new mdc.textField.MDCTextField(document.querySelector('.mdc-text-field'));

// Initialize MDC Ripple on button
const button = new mdc.ripple.MDCRipple(document.querySelector('.mdc-button'));

function checkSetup() {
    const storedKey = localStorage.getItem('eg_key');
    if (storedKey) {
        setupMessage.textContent = "Everything is good! The key is already set up.";
        setupForm.style.display = 'none';
    } else {
        setupMessage.textContent = "";
        setupForm.style.display = 'block';
    }
}

setupForm.addEventListener('submit', function (e) {
    e.preventDefault();
    const key = keyInput.value.trim();
    if (key) {
        localStorage.setItem('eg_key', key);
        checkSetup();
    }
});

checkSetup();