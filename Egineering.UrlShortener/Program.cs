using Egineering.UrlShortener.DTOs.Request;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AddServices(builder.Services);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapGet("/", async (HttpContext httpContext) =>
{
    httpContext.Response.Headers.ContentType = "text/html; charset=UTF-8";
    await httpContext.Response.SendFileAsync("wwwroot/index.html");
});

app.MapGet("/setup", async (HttpContext httpContext) =>
{
    httpContext.Response.Headers.ContentType = "text/html; charset=UTF-8";
    await httpContext.Response.SendFileAsync("wwwroot/setup.html");
});

app.MapGet("/{vanity}", async (string vanity, HttpContext httpContext, IAzureTableStorageService service) =>
{
    try
    {
        var resultUrl = await service.GetUrlFromVanityAsync(vanity);

        httpContext.Response.StatusCode = 302;
        httpContext.Response.Redirect(resultUrl);
    }
    catch (RequestFailedException requestFailedException)
    {
        await HandleRequestFailedException(httpContext.Response, requestFailedException);
    }
});

app.MapGet("/api/urls", async (IAzureTableStorageService service, HttpContext httpContext) =>
{
    try
    {
        var urls = service.GetAllPublicUrls();

        httpContext.Response.StatusCode = 200;
        await httpContext.Response.WriteAsJsonAsync(urls);
    }
    catch (RequestFailedException requestFailedException)
    {
        await HandleRequestFailedException(httpContext.Response, requestFailedException);
    }
});

app.MapPost("/api/urls", async (UrlRequest urlRequest, IAzureTableStorageService service,
    HttpContext httpContext) =>
{
    try
    {
        ValidateAuthHeader(httpContext.Request);

        var vanity = await service.AddUrl(urlRequest);

        httpContext.Response.StatusCode = 201;
        var shortenedUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{vanity}";
        await httpContext.Response.WriteAsJsonAsync(new { shortenedUrl });
    }
    catch (RequestFailedException requestFailedException)
    {
        await HandleRequestFailedException(httpContext.Response, requestFailedException);
    }
    catch (UnauthorizedException unauthorizedException)
    {
        httpContext.Response.StatusCode = 401;
        await httpContext.Response.WriteAsJsonAsync(new { message = unauthorizedException.Message });
    }
    catch (ConflictException conflictException)
    {
        httpContext.Response.StatusCode = 409;
        await httpContext.Response.WriteAsJsonAsync(new { message = conflictException.Message });
    }
});

app.MapPut("/api/urls", async (UrlRequest urlRequest, IAzureTableStorageService service,
    HttpContext httpContext) =>
{
    try
    {
        ValidateAuthHeader(httpContext.Request);

        await service.ReplaceUrl(urlRequest);

        httpContext.Response.StatusCode = 204;
    }
    catch (RequestFailedException requestFailedException)
    {
        await HandleRequestFailedException(httpContext.Response, requestFailedException);
    }
    catch (UnauthorizedException unauthorizedException)
    {
        httpContext.Response.StatusCode = 401;
        await httpContext.Response.WriteAsJsonAsync(new { message = unauthorizedException.Message });
    }
    catch (UrlEntityNotFoundException urlEntityNotFoundException)
    {
        httpContext.Response.StatusCode = 404;
        await httpContext.Response.WriteAsJsonAsync(new { message = urlEntityNotFoundException.Message });
    }
});

app.MapPatch("api/{vanity}/toggle-public", async (string vanity, IAzureTableStorageService service,
       HttpContext httpContext) =>
{
    try
    {
        ValidateAuthHeader(httpContext.Request);

        await service.TogglePublic(vanity);

        httpContext.Response.StatusCode = 204;
    }
    catch (RequestFailedException requestFailedException)
    {
        await HandleRequestFailedException(httpContext.Response, requestFailedException);
    }
    catch (UnauthorizedException unauthorizedException)
    {
        httpContext.Response.StatusCode = 401;
        await httpContext.Response.WriteAsJsonAsync(new { message = unauthorizedException.Message });
    }
    catch (UrlEntityNotFoundException urlEntityNotFoundException)
    {
        httpContext.Response.StatusCode = 404;
        await httpContext.Response.WriteAsJsonAsync(new { message = urlEntityNotFoundException.Message });
    }
});

app.MapGet("api/qr/{url}", (string url, IQRCodeService service) =>
{
    return GenerateQRCode(url, service);
});

app.MapPost("api/qr", (QRCodeGenerationRequest request, IQRCodeService service) =>
{
    return GenerateQRCode(request.Url, service);
});

app.Run();

static void AddServices(IServiceCollection services)
{
    services.AddSingleton<IAzureTableStorageService, AzureTableStorageService>();
    services.AddSingleton<IQRCodeService, QRCodeService>();
}

void ValidateAuthHeader(HttpRequest request)
{
    if (!request.Headers.ContainsKey(Constants.Authorization)
        || request.Headers.Authorization != builder.Configuration[Constants.SecurityToken])
    {
        throw new UnauthorizedException();
    }
}

static async Task HandleRequestFailedException(HttpResponse response, RequestFailedException exception)
{
    response.StatusCode = exception.Status;

    var message = exception.Message.Split("\n").First();

    await response.WriteAsJsonAsync(new { message });
}

IResult GenerateQRCode(string url, IQRCodeService service)
{
    var qrCodeBytes = service.GenerateQRCode(url);

    var mimeType = "image/png";

    return Results.File(qrCodeBytes, mimeType, "qr.png");
}
