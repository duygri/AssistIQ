namespace AssistIQ.Application.Common;

/// <summary>
/// Generic paged result envelope returned by list endpoints.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Data,
    int Total,
    int Page,
    int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)Total / PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}
