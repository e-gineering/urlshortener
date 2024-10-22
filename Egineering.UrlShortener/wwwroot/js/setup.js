
const setupMessage = document.getElementById('setup-message');
const setupForm = document.getElementById('setup-form');
const keyInput = document.getElementById('key-input');

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