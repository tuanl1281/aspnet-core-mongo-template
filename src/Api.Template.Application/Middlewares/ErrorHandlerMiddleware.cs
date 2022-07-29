// ReSharper disable InconsistentNaming
// ReSharper disable TemplateIsNotCompileTimeConstantProblem
using Serilog;
using System.Net;
using Microsoft.AspNetCore.Http;
using Gateway.ViewModel.Common.Exception;
using Gateway.ViewModel.Common.Response;

namespace Gateway.Application.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

	public ErrorHandlerMiddleware(RequestDelegate next)
	{
		_next = next;
	}

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(httpContext, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        object result;
        switch (exception)
        {
            case ServiceException _exception:
                {
                    context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                    result = new ErrorResponseModel()
                    {
                        Succeed = false,
                        Error = new()
                        {
                            Code = _exception.Code,
                            Message = _exception.Message,
                            Details = _exception.Details
                        },
                        Data = _exception.Data
                    };

                    break;
                }
            case NotFoundException _exception:
                {
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    result = new ResultResponseModel()
                    {
                        Succeed = false,
                        Message = _exception.Message,
                    };

                    Log.Error(_exception.Message);
                    break;
                }
            default:
                {
                    var builder = exception.Message + "\n" + (exception.InnerException != null ? exception.InnerException.Message : "") + "\n ***Trace*** \n" + exception.StackTrace;
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    result = builder;
                    
                    Log.Error(builder);
                    break;
                }
        }

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result.ToString() ?? string.Empty);
    }
}