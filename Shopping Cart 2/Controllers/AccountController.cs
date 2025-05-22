using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly EmailService _emailService;

    public AccountController(EmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<IActionResult> SendWelcomeEmail(string userEmail)
    {
        var callbackUrl = Url.Action("Index", "Home", null, Request.Scheme);

        var body = $@"
            <h2>Welcome to VANWEAR</h2>
            <p>Thank you for registering!</p>
            <p>Click the link below to go to the home page:</p>
            <a href='{callbackUrl}'>Go to Home Page</a>
        ";

        await _emailService.SendEmailAsync(userEmail, "Welcome to VANWEAR", body);

        return View("EmailSentConfirmation");
    }
}
