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

app.MapGet("/{vanity}", async (string vanity, HttpResponse response, IAzureTableStorageService service) =>
{
    try
    {
        var resultUrl = await service.GetUrlFromVanityAsync(vanity);

        response.StatusCode = 302;
        response.Redirect(resultUrl);
    }
    catch (RequestFailedException requestFailedException)
    {
        await HandleRequestFailedException(response, requestFailedException);
    }
});

app.MapGet("/api/all", async (IAzureTableStorageService service, HttpResponse response) =>
{
    try
    {
        var urls = service.GetAllShortenedUrls();

        response.StatusCode = 200;
        await response.WriteAsJsonAsync(urls);
    }
    catch (RequestFailedException requestFailedException)
    {
        await HandleRequestFailedException(response, requestFailedException);
    }
});

app.MapPost("/api/save", async (IAzureTableStorageService service, UrlRequest urlRequest,
    HttpRequest request, HttpResponse response) =>
{
    try
    {
        ValidateAuthHeader(request);

        await service.SaveUrl(urlRequest);

        response.StatusCode = 204;
    }
    catch (RequestFailedException requestFailedException)
    {
        await HandleRequestFailedException(response, requestFailedException);
    }
    catch (UnauthorizedException unauthorizedException)
    {
        response.StatusCode = 401;
        await response.WriteAsJsonAsync(new { message = unauthorizedException.Message });
    }
});

app.Run();

static void AddServices(IServiceCollection services)
{
    services.AddSingleton<IAzureTableStorageService, AzureTableStorageService>();
}

void ValidateAuthHeader(HttpRequest request)
{
    if (!request.Headers.ContainsKey("Authorization")
        || request.Headers.Authorization != builder.Configuration["SecurityToken"])
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
