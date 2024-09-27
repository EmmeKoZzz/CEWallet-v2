using System.ComponentModel.DataAnnotations;

namespace Common.DataTransferObjects;

public record AddCurrencyDto([Required] string Name);