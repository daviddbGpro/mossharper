using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using MossHarper.Services;

namespace MossHarper.Pages;

[EnableRateLimiting("contact")]
public sealed class ContactModel(IContactMailer mailer) : PageModel
{
    [BindProperty]
    public Enquiry Input { get; set; } = new();

    [BindProperty]
    public string? Website { get; set; }

    public string? SuccessMessage { get; private set; }

    public void OnGet(bool again = false)
    {
        if (again)
            TempData.Remove(nameof(SuccessMessage));
        else
            SuccessMessage = TempData[nameof(SuccessMessage)] as string;
    }

    public async Task<IActionResult> OnPostSendAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(Website))
            return ContactResult();

        if (!ModelState.IsValid)
            return ContactResult();

        var sent = await mailer.SendAsync(
            Input.Name.Trim(), Input.Email.Trim(), Input.Organisation?.Trim() ?? "",
            Input.Message.Trim(), cancellationToken);

        if (!sent)
        {
            ModelState.AddModelError(string.Empty, "We could not send your enquiry just now. Please try again later.");
            return ContactResult();
        }

        const string successMessage = "Thank you. Your enquiry has been received.";
        if (Request.Headers.ContainsKey("HX-Request"))
        {
            SuccessMessage = successMessage;
            return Partial("_ContactForm", this);
        }

        TempData[nameof(SuccessMessage)] = successMessage;
        return RedirectToPage();
    }

    private IActionResult ContactResult() => Request.Headers.ContainsKey("HX-Request")
        ? Partial("_ContactForm", this)
        : Page();

    public sealed class Enquiry
    {
        [Required(ErrorMessage = "Enter your name.")]
        [StringLength(120)]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Enter your email address.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(254)]
        public string Email { get; set; } = "";

        [StringLength(150)]
        public string? Organisation { get; set; }

        [Required(ErrorMessage = "Tell us about your enquiry.")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "Your message must be between 10 and 5,000 characters.")]
        public string Message { get; set; } = "";
    }
}
