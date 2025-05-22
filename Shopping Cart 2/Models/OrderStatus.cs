[Table("OrderStatus")]
public class OrderStatus
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string StatusName { get; set; } = "Pending"; // Default status

    public List<Order> Orders { get; set; } = new List<Order>();
}