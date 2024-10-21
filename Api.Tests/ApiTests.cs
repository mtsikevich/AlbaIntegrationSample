using System.Security.Claims;
using Alba;

namespace Api.Tests;

public class ApiTests(IntegrationFixture fixture) : IntegrationContext(fixture)
{
    [Fact(DisplayName = "Given an alba host is running.")]
    public async Task SomeUnitTest()
    {
        await Host.Scenario(c =>
        {
            c.Get.Url("/");
            c.WithRequestHeader("hh", "vv");
            c.StatusCodeShouldBeOk();
        });
    }

    [Fact(DisplayName = "Given you visit a secure endpoint.")]
    public async Task SecureTest()
    {
        await Host.Scenario(c =>
        {
            c.Get.Url("/secure");
            c.WithClaim(new Claim("cl", "cv"));
            c.StatusCodeShouldBeOk();
        });
    }

    [Fact(DisplayName = "Given you visit a secure endpoint.")]
    public async Task PeopleListTest()
    {
        await Host.Scenario(c =>
        {
            c.Get.Url("/people");
            c.StatusCodeShouldBeOk();
        });
    }
    
    [Fact(DisplayName = "Given you visit a secure endpoint.")]
    public async Task PeopleAddTest()
    {
        await Host.Scenario(c =>
        {
            c.WithRequestHeader("cl", "cv");
            c.Get.Url("/people/add");
            c.StatusCodeShouldBe(201);
            c.StatusCodeShouldBeSuccess();
        });
    }
}