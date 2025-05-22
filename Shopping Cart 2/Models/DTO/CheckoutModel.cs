namespace Shopping_Cart_2.Models.DTO
{
    public class CheckoutModel
    {
        [Required]
        [MaxLength(30)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Please enter valid characters only (no digits).")]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(30)]
        public string? Email { get; set; }
        [Required]
        [Phone]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Please enter a valid phone number (digits only, 10–15 characters).")]
        public string? MobileNumber { get; set; }
        [Required]
        [MaxLength(200)]
        public string? Address { get; set; }

        [Required]
        public string? PaymentMethod { get; set; }
    }
}
