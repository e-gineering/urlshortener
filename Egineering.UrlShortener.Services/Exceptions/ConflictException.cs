namespace Egineering.UrlShortener.Services.Exceptions;

[ExcludeFromCodeCoverage]
public class ConflictException : Exception
{
    public ConflictException(string vanity) : base($"The entry with vanity '{vanity}' already exists") { }
}
