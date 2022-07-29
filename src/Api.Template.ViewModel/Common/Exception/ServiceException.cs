namespace Gateway.ViewModel.Common.Exception;

public class ServiceException: System.Exception
{
    public string Code { get; }

    public dynamic Details { get; }
    
    public dynamic Data { get; }

    public ServiceException() : base() { }

    public ServiceException(string code) : base() { }
    
    public ServiceException(string code, string message) : base(message) { }

    public ServiceException(string code, string message, dynamic details): base(message)
    {
        Details = details;
    }
    
    public ServiceException(string code, string message, dynamic details, dynamic data): base(message)
    {
        Details = details;
        Data = data;
    }
}