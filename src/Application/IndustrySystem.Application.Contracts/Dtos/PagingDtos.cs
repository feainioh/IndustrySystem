namespace IndustrySystem.Application.Contracts.Dtos;

public record PagingRequest(int PageIndex =1, int PageSize =20, string? Keyword = null);

public class PagedResult<T>
{
 public int TotalCount { get; init; }
 public List<T> Items { get; init; } = new();
}
