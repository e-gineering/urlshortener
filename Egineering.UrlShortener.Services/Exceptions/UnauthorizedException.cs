namespace Egineering.UrlShortener.Services.Exceptions;

[ExcludeFromCodeCoverage]
public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base($"The provided auth token is invalid") { }
}
