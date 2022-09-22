const urlCard = document.getElementById('url-card');

fetch('api/urls')
    .then(response => response.json())
    .then(data => renderCards(data));

function copyUrl(element) {
    navigator.clipboard.writeText(element.dataset.url);
}

function renderCards(data) {
    const cardsDiv = document.getElementById('cards');

    data.forEach(url => {
        const instance = document.importNode(urlCard.content, true);

        instance.querySelector('.name').innerHTML = url.name;

        const shortUrl = `${document.URL}${url.vanity}`;

        const vanity = instance.querySelector('.vanity');
        vanity.href = shortUrl;
        vanity.innerHTML = url.vanity;

        instance.querySelector('.url').innerHTML = url.url;
        instance.querySelector('.visits').innerHTML = url.visits;

        instance.querySelector('.copy-short-url-button').dataset.url = shortUrl;

        cardsDiv.appendChild(instance);
    });
}
