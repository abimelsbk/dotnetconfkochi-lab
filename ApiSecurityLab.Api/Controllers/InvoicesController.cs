using System.Security.Claims;
using ApiSecurityLab.Api.Data;
using ApiSecurityLab.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiSecurityLab.Api.Controllers;

// ============================================================
//  VULNERABLE VERSION (branch: lab-vulnerable)
//  This controller has a Broken Object Level Authorization bug.
//  For learning on localhost only. Never deploy this code.
// ============================================================

[ApiController]
[Route("api/invoices")]
[Authorize] // Only checks "is the caller logged in?"
public class InvoicesController(AppDbContext db) : ControllerBase
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

    // GET /api/invoices/{id}  -> VULNERABLE
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var invoice = await db.Invoices.FindAsync(id);
        if (invoice is null) return NotFound();

        // BUG: nobody checks who owns this invoice.
        return Ok(invoice);
    }

    // PUT /api/invoices/{id}  -> VULNERABLE
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateInvoice(int id, UpdateInvoiceRequest request)
    {
        var invoice = await db.Invoices.FindAsync(id);
        if (invoice is null) return NotFound();

        // BUG: same mistake on write. Alice can change Bob's invoice.
        invoice.Description = request.Description;
        invoice.Amount = request.Amount;
        await db.SaveChangesAsync();
        return Ok(invoice);
    }
}
