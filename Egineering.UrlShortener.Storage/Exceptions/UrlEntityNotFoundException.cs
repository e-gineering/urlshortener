using System.Diagnostics.CodeAnalysis;
namespace Egineering.UrlShortener.Storage.Exceptions;

[ExcludeFromCodeCoverage]
public class UrlEntityNotFoundException : Exception
{
    public UrlEntityNotFoundException(string vanity) : base($"The entry with vanity '{vanity}' could not be found") { }
}
