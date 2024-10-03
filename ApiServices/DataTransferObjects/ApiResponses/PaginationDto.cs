namespace ApiServices.DataTransferObjects.ApiResponses;

public record PaginationDto<T>(IEnumerable<T> Data, int Page, int Size, int TotalLenght);