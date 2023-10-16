/*
Задача:
    Реализовать систему сохранения и просмотра изображений. 
    Пользователи: загружают изображения, могут их просматривать, добавлять друзей, которые могут просматривать изображения пользователя.

Функциональные требования:
     - Наличие регистрации пользователя - ✔
     - Пользователь должен иметь возможность загрузить изображение - ✔
     - Пользователь должен иметь возможность просмотреть свои изображения - ✔
     - Пользователь должен иметь возможность просмотреть изображения другого пользователя, если он является его другом.
     - Пользователь должен иметь возможность добавить другого пользователя в друзья 
     (пользователь А может просматривать изображения пользователя B, если пользователь B добавил пользователя А в друзья, 
     но пользователь B не может просматривать изображения пользователя А, если тот не ответил взаимностью) - ✔
 
Не функциональные требования:
    - Приложение должно быть написано на .NET 7, C# 11, ASP.NET Core - ✔
    - Любой вид авторизации (ASP.NET Identity, можно хранить в базе логин + пароль использовать basic auth) - ✔
    - Для реализации должен быть использован EntityFrameworkCore, богатая доменная модель ✔
    - В сущности пользователя, набор картинок должен быть приватным полем.
    Наружу должно быть доступно только свойство с IReadOnlyCollection (необходимо настроить конфигурацию EF чтобы это работало) - ✔
    - Сами изображения должны храниться на локальном хранилище, путь должен быть настраиваемым через appsettings.json.
    При переносе локации хранилища и самих изображений, приложение должно работать корректно (не хранить в базе абсолютные пути) - ✔
    - Отношение пользователь-друзья (many-to-many на пользователях) должно быть реализовано через конфигурацию EF - ✔
    - Приложение должно иметь restful API, возвращать корректные коды ошибок (404, 401, 403 итд) - ✔
    - Приложение должно иметь swagger, с возможностью конфигурировать выбранный способ авторизации через UI - ✔
    - Приложение должно иметь все эндпоинты для реализации функциональных требований - ✔
*/

using System.Text.Json.Serialization;
using Imager.Database;
using Imager.Services.Implementation;
using Imager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(dbBuilder =>
{
    dbBuilder.UseSqlite("Data Source=./database.db");
});

builder.Services.AddControllers()
    .AddJsonOptions(jsonOptions =>
    {
        jsonOptions.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Imager API Docs",
        Version = "v1"
    });
    
    options.AddSecurityDefinition("Authorization", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Access Token",
        Type = SecuritySchemeType.ApiKey,
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Authorization",
                    Type = ReferenceType.SecurityScheme
                } 
            },
            new List<string>()
        }
    });
});

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IImageService, ImageService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();