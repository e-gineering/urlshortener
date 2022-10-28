using System.Diagnostics.CodeAnalysis;

namespace Egineering.UrlShortener.Storage.Exceptions;

[ExcludeFromCodeCoverage]
public class ConflictException : Exception
{
    public ConflictException(string vanity) : base($"The entry with vanity '{vanity}' already exists") { }
}
