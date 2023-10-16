using Imager.Domain.Errors;
using Imager.Services.Interfaces;
using Imager.Web.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Imager.Web.Filters;

public class AuthorizationAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<AuthorizationAttribute>>() 
                     ?? throw new NullReferenceException("Authorization logger is null");
        
        if (!context.HttpContext.Request.IsHttps)
        {
            logger.LogInformation("Запрос с авторизацией был отправлен с помощью HTTP");
            context.Result = new JsonResult(new BaseErrorResponse
            {
                Message = "Доступ к методам с авторизацией только по HTTPS"
            })
            {
                StatusCode = 401
            };
            return;
        }

        var accessToken = context.HttpContext.Request.Headers["Authorization"];
        if (accessToken.Equals(StringValues.Empty))
        {
            logger.LogInformation("Авторизационный токен не был отправлен");
            context.Result = new JsonResult(new BaseErrorResponse
            {
                Message = "Токен доступа не найден"
            })
            {
                StatusCode = 401
            };
            return;
        }

        var jwtTokenService = context.HttpContext.RequestServices.GetService<IJwtTokenService>();
        if (jwtTokenService is null)
        {
            logger.LogError("Сервис JWT токенов не был получен");
            context.Result = new JsonResult(new BaseErrorResponse
            {
                Message = "Внутреняя ошибка сервера"
            })
            {
                StatusCode = 500
            };
            return;
        }

        var parseAccessTokenResult = await jwtTokenService.ParseAccessTokenAsync(accessToken.ToString());

        if (parseAccessTokenResult.IsFailure)
        {
            var error = parseAccessTokenResult.UnwrapError();
            if (error is JwtTokenError.TokenExpired)
            {
                context.Result = new JsonResult(new BaseErrorResponse
                {
                    Message = "Токен истёк"
                })
                {
                    StatusCode = 401
                };
                return;
            }

            if (error is JwtTokenError.IncorrectTokenFormat)
            {
                context.Result = new JsonResult(new BaseErrorResponse
                {
                    Message = "Ошибка валидации токена"
                })
                {
                    StatusCode = 403
                };
            }
        }

        var tokenData = parseAccessTokenResult.Unwrap();
        context.HttpContext.Items.Add("UserData", tokenData);
    }
}