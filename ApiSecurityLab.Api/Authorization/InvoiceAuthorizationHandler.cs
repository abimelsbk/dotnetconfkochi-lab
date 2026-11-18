using System.Security.Claims;
using ApiSecurityLab.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace ApiSecurityLab.Api.Authorization;

// The rule we want: "you must own this invoice".
public class InvoiceOwnerRequirement : IAuthorizationRequirement { }

public class InvoiceAuthorizationHandler
    : AuthorizationHandler<InvoiceOwnerRequirement, Invoice>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InvoiceOwnerRequirement requirement,
        Invoice invoice)
    {
        // Identity comes from the validated token, never from the request body or URL.
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is not null && invoice.OwnerId == userId)
        {
            context.Succeed(requirement);
        }

        // If we don't call Succeed, access is denied.
        return Task.CompletedTask;
    }
}
