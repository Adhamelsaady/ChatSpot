namespace ChatSpot.Dtos.Responses;

public class PagedResult <T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    
    public int? PreviousPageNumber => HasPreviousPage ? PageNumber - 1 : null;
    
    public int? NextPageNumber => HasNextPage ? PageNumber + 1 : null;
}