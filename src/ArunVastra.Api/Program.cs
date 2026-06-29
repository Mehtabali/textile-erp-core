using System.Text;
using ArunVastra.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

const string CorsPolicyName = "AngularClient";

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Arun Vastra API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT bearer token only."
    });

    options.EnableAnnotations();
});
builder.Services.AddInfrastructure(builder.Configuration);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Arun Vastra API v1");
    });
//}

app.UseCors(CorsPolicyName);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
