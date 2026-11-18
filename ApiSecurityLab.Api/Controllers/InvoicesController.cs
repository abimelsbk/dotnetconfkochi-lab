using System.Security.Claims;
using ApiSecurityLab.Api.Data;
using ApiSecurityLab.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiSecurityLab.Api.Controllers;

// ============================================================
//  SECURE VERSION (branch: lab-secure)
//  GET uses Fix 1: filter the query by ID AND owner.
//  PUT uses Fix 2: resource-based authorization with a handler.
// ============================================================

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoicesController(AppDbContext db, IAuthorizationService authz) : ControllerBase
{
    // GET /api/invoices/mine  -> SAFE: filtered by the caller
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyInvoices()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var invoices = await db.Invoices
            .Where(i => i.OwnerId == userId)
            .ToListAsync();
        return Ok(invoices);
    }

    // FIX 1: ask the database for this ID AND this owner.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var invoice = await db.Invoices
            .SingleOrDefaultAsync(i => i.Id == id && i.OwnerId == userId);

        // Not found and not yours get the same answer.
        // The attacker can't even tell that invoice 202 exists.
        if (invoice is null) return NotFound();

        return Ok(invoice);
    }

    // FIX 2: load the invoice, then ask the authorization service.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateInvoice(int id, UpdateInvoiceRequest request)
    {
        var invoice = await db.Invoices.FindAsync(id);
        if (invoice is null) return NotFound();

        var result = await authz.AuthorizeAsync(User, invoice, "InvoiceOwner");
        if (!result.Succeeded) return NotFound(); // use Forbid() if you prefer 403

        invoice.Description = request.Description;
        invoice.Amount = request.Amount;
        await db.SaveChangesAsync();
        return Ok(invoice);
    }
}
