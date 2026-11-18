namespace ApiSecurityLab.Api.Models;

public class Invoice
{
    public int Id { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
