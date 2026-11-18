using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ApiSecurityLab.Tests;

public class InvoiceAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public InvoiceAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        // Start the real API in memory, but swap JWT login for the fake test login.
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });
            });
        });
    }

    private HttpClient ClientFor(string? user)
    {
        var client = _factory.CreateClient();
        if (user is not null)
            client.DefaultRequestHeaders.Add("X-Test-User", user);
        return client;
    }

    [Fact]
    public async Task Alice_can_read_her_own_invoice()
    {
        var response = await ClientFor("alice").GetAsync("/api/invoices/101");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Bob_can_read_his_own_invoice()
    {
        var response = await ClientFor("bob").GetAsync("/api/invoices/202");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // The regression test for BOLA on read.
    [Fact]
    public async Task Alice_cannot_read_bobs_invoice()
    {
        var response = await ClientFor("alice").GetAsync("/api/invoices/202");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // The regression test for BOLA on write.
    [Fact]
    public async Task Alice_cannot_update_bobs_invoice()
    {
        var response = await ClientFor("alice").PutAsJsonAsync(
            "/api/invoices/202",
            new { description = "Changed by Alice", amount = 1 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_user_gets_401()
    {
        var response = await ClientFor(null).GetAsync("/api/invoices/101");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
