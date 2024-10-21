using Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddDbContext<ProductDb>(
    dbBuilder => dbBuilder.UseNpgsql(builder.Configuration.GetConnectionString("ProductDb")));

builder.Services.AddDbContext<PeopleDb>(
    dbBuilder => dbBuilder.UseNpgsql(builder.Configuration.GetConnectionString("PeopleDb")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/people", ([FromServices] PeopleDb peopleDb) =>
{
    var result = peopleDb.People.ToList();
    return Results.Ok(result);
});
app.MapGet("/people/add", async ([FromServices] PeopleDb peopleDb) =>
{
    peopleDb.People.Add(new Person { Name = "Some Guy" });
    await peopleDb.SaveChangesAsync();
    var first = await peopleDb.People.FirstOrDefaultAsync();
    return Results.Created();
});
app.MapGet("/secure", [Microsoft.AspNetCore.Authorization.Authorize] (HttpContext httpContext) =>
{
    var claims = httpContext.User.Claims.Select(c => new { c.Type, c.Value });
    return Results.Json(claims);
});

app.Run();
