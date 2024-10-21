using System.IdentityModel.Tokens.Jwt;
using Alba;
using Alba.Security;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.LocalStack;
using Testcontainers.PostgreSql;

namespace Api.Tests;

public class IntegrationFixture : IAsyncLifetime
{
    private readonly LocalStackContainer _localstack = new LocalStackBuilder()
        .WithName("localstack")
        .WithStartupCallback((container, ct) =>
            container.ExecAsync(new[] { "awslocal", "s3api", "create-bucket", "--bucket", "images" }, ct))
        .Build();

    private readonly PostgreSqlContainer _peopleDbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithHostname("h2")
        .WithCleanUp(true)
        .Build();

    private readonly PostgreSqlContainer _productsDbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithHostname("h3")
        .WithCleanUp(true)
        .Build();

    public IAlbaHost Host { get; set; } = null!;

    public async Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "sample");
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "ai!");

        await _localstack.StartAsync();

        await _peopleDbContainer.StartAsync();
        await _productsDbContainer.StartAsync();

        var securityStub = new AuthenticationStub()
            .With("foo", "bar")
            .WithName("Stugy")
            .With(JwtRegisteredClaimNames.Email, "test@test.com");

        Host = await AlbaHost.For<Program>(configure =>
        {
            configure.ConfigureTestServices(services =>
            {
                services.RemoveDbContext<ProductDb>();
                services.AddDbContext<ProductDb>(builder => builder.UseNpgsql(_productsDbContainer.GetConnectionString()));
                
                services.RemoveDbContext<PeopleDb>();
                services.AddDbContext<PeopleDb>(builder => builder.UseNpgsql(_peopleDbContainer.GetConnectionString()));

                services.EnsureDbCreated<ProductDb>();
                services.EnsureDbCreated<PeopleDb>();
            });
        }, securityStub);
    }

    public async Task DisposeAsync()
    {
        await Host.DisposeAsync();
        await _peopleDbContainer.DisposeAsync();
        await _productsDbContainer.DisposeAsync();
    }
}

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services) where T : DbContext
    {
        var descriptor = services.SingleOrDefault(x => x.ServiceType == typeof(DbContextOptions<T>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    public static void EnsureDbCreated<T>(this IServiceCollection services) where T : DbContext
    {
        using var scope = services.BuildServiceProvider().CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var context = serviceProvider.GetRequiredService<T>();
        context.Database.EnsureCreated();
    }
}

[CollectionDefinition("Col")]
public class IntegrationCollection : ICollectionFixture<IntegrationFixture>
{
}

[Collection("Col")] //calling the definition directly
public class MyServiceTests
{
    private readonly IAlbaHost _host;

    public MyServiceTests(IntegrationFixture fixture)
    {
        _host = fixture.Host;
    }
}

[Collection("Col")]
public abstract class IntegrationContext
{
    public IAlbaHost Host { get; set; }

    protected IntegrationContext(IntegrationFixture fixture)
    {
        Host = fixture.Host;
    }
}