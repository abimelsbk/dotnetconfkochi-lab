namespace ApiSecurityLab.Api.Models;

// No OwnerId here, on purpose. The client must never decide the owner.
public record UpdateInvoiceRequest(string Description, decimal Amount);
