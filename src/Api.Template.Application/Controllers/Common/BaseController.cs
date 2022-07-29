using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Gateway.Application.Helpers.Authentication;
using Gateway.Data.Constants.Common;
using Gateway.ViewModel.Common.Response;
using Gateway.ViewModel.Common.Exception;

namespace Gateway.Application.Controllers.Common;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class BaseController: ControllerBase
{
    protected new IPrincipal Principal
    {
        get
        {
            try
            {
                var userId = GetValueOfClaim(ClaimConstants.UserId);
                if (string.IsNullOrEmpty(userId))
                    throw new Exception();

                return new Principal()
                {
                    UserId = Guid.Parse(userId),
                    FullName = GetValueOfClaim(ClaimConstants.FullName),
                    UserName = GetValueOfClaim(ClaimConstants.UserName),
                    Role = GetValueOfClaim(ClaimConstants.Role),
                };
            }
            catch (Exception)
            {
                throw new ServiceException("Invalid token");
            }
        }
    }
    
    public BaseController()
    {
    }

    private string GetValueOfClaim(string claim)
    {
        if (!HttpContext.User.HasClaim(_ => _.Type.Equals(claim)))
            return String.Empty;

        return HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(claim))?.Value ?? String.Empty;
    }
    
    protected ResultResponseModel BuildResultResponse(object data, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        HttpContext.Response.StatusCode = (int)statusCode;
        return new ResultResponseModel(data);
    }

    protected ResultResponseModel BuildResultResponse(object data, string message, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        HttpContext.Response.StatusCode = (int) statusCode;
        return new ResultResponseModel(data, message);
    }
    
    protected PagingResponseModel BuildPagingResponse(object data, double totalCounts)
    {
        HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        return new PagingResponseModel(data, totalCounts);
    }
}