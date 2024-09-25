using System.ComponentModel.DataAnnotations;

namespace Common.DataTransferObjects;

public record AddCurrencyDto([property: Required] string Name, string UrlValue);