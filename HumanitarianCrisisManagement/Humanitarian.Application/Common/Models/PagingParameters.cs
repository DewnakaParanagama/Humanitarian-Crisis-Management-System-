namespace Humanitarian.Application.Common.Models;

public class PagingParameters
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1
            ? DefaultPageSize
            : Math.Min(value, MaxPageSize);
    }
}