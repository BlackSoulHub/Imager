using Imager.Domain.Models;
using Imager.Web.Filters;
using Imager.Web.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Imager.Web.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    [NonAction]
    public IActionResult Error(string message, int statusCode)
    {
        return Json(new BaseErrorResponse
        {
            Message = message
        }, statusCode);
    }
    
    [NonAction]
    public IActionResult ServerError()
    {
        return Json(new BaseErrorResponse
        {
            Message = "Внутреняя ошибка сервера"
        }, 500);
    }
    
    [NonAction]
    private IActionResult Json(object content, int statusCode = 200)
    {
        return new JsonResult(content)
        {
            StatusCode = statusCode,
        };
    }
    
    [NonAction]
    public IActionResult NotAccess()
    {
        return Error("У вас нет сюда доступа", 403);
    }
    
    [NonAction]
    public async Task<AccessTokenModel> GetUserDataAsync()
    {
        if (HttpContext.Items.TryGetValue("UserData", out object? tokenObject))
        {
            if (tokenObject is null)
            {
                Response.StatusCode = 500;
                await Response.WriteAsJsonAsync(new
                {
                    ErrorMessage = "Ошибка авторизации"
                });
                throw new NullReferenceException(
                    $"{nameof(GetUserDataAsync)} вызван у метода без аттрибута {nameof(AuthorizationAttribute)}");
            }

            if (tokenObject is not AccessTokenModel tokenData)
            {
                Response.StatusCode = 500;
                await Response.WriteAsJsonAsync(new
                {
                    ErrorMessage = "Ошибка авторизации"
                });
                throw new ArgumentOutOfRangeException(
                    $"{nameof(tokenObject)} не соответствует {nameof(AccessTokenModel)}");
            }

            return tokenData;
        }
        
        Response.StatusCode = 500;
        await Response.WriteAsJsonAsync(new
        {
            ErrorMessage = "Ошибка авторизации"
        });
        throw new NullReferenceException(
            $"{nameof(GetUserDataAsync)} вызван у метода без аттрибута {nameof(AuthorizationAttribute)}");
    }
}