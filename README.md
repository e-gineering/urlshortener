# URL Shortener

### QR Codes
To generate a QR code, base64 encode a URL and make a GET request to `/api/qr/{url}` or a POST request to `/api/qr` with the base64 encoded URL in the payload. For example:
```
curl https://yoururl.com/api/qr -k -X POST -d '{"url":"d3d3LmUtZ2luZWVyaW5nLmNvbS9jb250YWN0LXVz"}' -H 'content-type: application/json'
```
