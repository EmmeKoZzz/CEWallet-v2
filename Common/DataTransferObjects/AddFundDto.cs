using System.ComponentModel.DataAnnotations;

namespace Common.DataTransferObjects;

public record AddFundDto([property: Required] string Name, string LocationUrl);