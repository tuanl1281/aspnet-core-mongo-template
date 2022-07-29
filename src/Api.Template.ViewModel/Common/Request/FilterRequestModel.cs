using Microsoft.AspNetCore.Mvc;

namespace Gateway.ViewModel.Common.Request;

public class CommonFilterRequest
{
    [FromQuery(Name = "fromDate")]
    public DateTime FromDate { get; set; } = DateTime.Now;

    [FromQuery(Name = "toDate")]
    public DateTime ToDate { get; set; } = DateTime.Now;

    [FromQuery(Name = "pageIndex")]
    public int PageIndex { get; set; } = 0;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = Int32.MaxValue;
}

public class PagingFilterRequest
{
    [FromQuery(Name = "pageIndex")]
    public int PageIndex { get; set; } = 0;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = Int32.MaxValue;
}

public class DateTimeFilterRequest
{
    [FromQuery(Name = "fromDate")]
    public DateTime FromDate { get; set; } = DateTime.Now;

    [FromQuery(Name = "toDate")]
    public DateTime ToDate { get; set; } = DateTime.Now;
}