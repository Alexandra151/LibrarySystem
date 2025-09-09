using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;            
using LibrarySystem.Infrastructure;
using LibrarySystem.Presentation.Middleware;
using LibrarySystem.Presentation.Auth;
using LibrarySystem.Presentation.Swagger;
using LibrarySystem.Presentation.GraphQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

var users = new List<AppUser>
{
    new("admin", "admin123", "Admin"),
    new("librarian", "librarian123", "Librarian"),
    new("member", "member123", "Member")
};
builder.Services.AddSingleton(users);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LibrarySystem API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Wpisz: Bearer {twój token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.OperationFilter<AddRequiredHeadersOperationFilter>();
});

builder.Services.AddInfrastructure();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LibrarySystem.Presentation v1");
        c.RoutePrefix = "swagger";

        c.UseRequestInterceptor(
            @"(req) => {
                if (!req.headers['X-Client-Name']) {
                    req.headers['X-Client-Name'] = 'Swagger';
                }
                if (!req.headers['X-Request-Id']) {
                    const makeId = () => (window.crypto?.randomUUID?.() ?? (Date.now().toString()));
                    req.headers['X-Request-Id'] = makeId();
                }
                return req;
            }");

        c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}

app.UseHeaderProcessing();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGraphQL("/graphql");

app.Run();

public partial class Program { }
