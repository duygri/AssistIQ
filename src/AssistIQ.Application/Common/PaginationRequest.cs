using System.ComponentModel.DataAnnotations;

namespace AssistIQ.Application.Common;

/// <summary>
/// Standard pagination query parameters validated and normalized before use.
/// </summary>
public sealed class PaginationRequest
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    private int _page = DefaultPage;
    private int _pageSize = DefaultPageSize;

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0.")]
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? DefaultPage : value;
    }

    [Range(1, MaxPageSize, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? DefaultPageSize
                         : value > MaxPageSize ? MaxPageSize
                         : value;
    }

    public int Skip => (Page - 1) * PageSize;
}
