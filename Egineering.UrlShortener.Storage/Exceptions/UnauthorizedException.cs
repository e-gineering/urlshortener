using System.Diagnostics.CodeAnalysis;

namespace Egineering.UrlShortener.Storage.Exceptions;

[ExcludeFromCodeCoverage]
public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base($"The provided auth token is invalid") { }
}
