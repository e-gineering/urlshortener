const urlCard = document.getElementById('url-card');

fetch('api/urls')
    .then(response => response.json())
    .then(data => renderCards(data));

function copyUrl(element) {
    navigator.clipboard.writeText(element.dataset.url);
}

function toggleQRCode(element) {
    const card = element.closest('.mdl-card');
    const qrCodeImage = card.querySelector('.qr-code');

    if (qrCodeImage.style.display === 'block') {
        qrCodeImage.style.display = 'none';
        return;
    }

    const shortUrl = element.dataset.url;
    fetch(`/api/qr/${encodeURIComponent(shortUrl)}`)
        .then(response => response.blob())
        .then(blob => {
            const url = window.URL.createObjectURL(blob);
            qrCodeImage.src = url;
            qrCodeImage.style.display = 'block';
        })
        .catch(error => console.error('Error generating QR code:', error));
}

function renderCards(data) {
    const cardsDiv = document.getElementById('cards');

    data.forEach(url => {
        const card = document.importNode(urlCard.content, true);

        card.querySelector('.name').innerHTML = url.name;

        const shortUrl = `${document.URL}${url.vanity}`;

        const vanity = card.querySelector('.vanity');
        vanity.href = shortUrl;
        vanity.innerHTML = url.vanity;

        card.querySelector('.url').innerHTML = url.url;
        card.querySelector('.visits').innerHTML = url.visits;

        card.querySelector('.copy-short-url-button').dataset.url = shortUrl;
        card.querySelector('.qr-code-button').dataset.url = shortUrl;

        addQRCodeElement(card);

        cardsDiv.appendChild(card);
    });
}

function addQRCodeElement(card) {
    const qrCodeImage = document.createElement('img');
    qrCodeImage.className = 'qr-code';
    qrCodeImage.src = '';
    qrCodeImage.alt = 'QR Code';
    qrCodeImage.style.display = 'none';
    card.querySelector('.mdl-card__supporting-text').appendChild(qrCodeImage);
}