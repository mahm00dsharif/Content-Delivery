using Content_Delivery.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Image = Content_Delivery.Models.Image;

namespace Content_Delivery.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ContentDeliveryDBContext _context;
        public HomeController(IWebHostEnvironment env, ContentDeliveryDBContext context)
        {
            _env = env;
            _context = context;
        }

        [HttpPost("Upload/{dir}")]
        public async Task<IActionResult> UploadImage([FromRoute] string dir, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select an image to upload.");
            }

            if (file.Length > 10 * 1024 * 1024) // Maximum file size of 10MB
            {
                return BadRequest("The file size must be less than 10MB.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" }; // Allowed file extensions
            var extension = Path.GetExtension(file.FileName);

            if (!allowedExtensions.Contains(extension.ToLower()))
            {
                return BadRequest("Only image files with .jpg, .jpeg, .png, and .gif extensions are allowed.");
            }

            var image = new Image
            {
                FileName = Guid.NewGuid().ToString(),
                FileSize = file.Length,
                MimeType = file.ContentType,
                Description = "",
                Title = "",
            };

            // Save the image to the database


            // Save the image file to disk
            var filePath = Path.Combine(_env.ContentRootPath, "images", dir, image.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/images/{dir}/{image.FileName}";
            image.OriginalUrl = url;
            image.ShortUrl = GenerateShortUrl(url);

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            // Update the image in the database with the URL
            //_context.Images.Update(image);
            //await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Image uploaded successfully.",
                imageUrl = $"/{image.ShortUrl}"
            });
        }

        [HttpGet("{shortUrl}")]
        public async Task<IActionResult> RedirectShortUrl([FromRoute] string shortUrl)
        {
            // Look up the original URL based on the short URL
            var url = await _context.Images.FirstOrDefaultAsync(u => u.ShortUrl == shortUrl);
            if (url == null)
            {
                return NotFound();
            }

            // Redirect the user to the original URL
            return Redirect(url.OriginalUrl);
        }

        private string GenerateShortUrl(string originalUrl)
        {
            // Generate a hash of the original URL using SHA256
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(originalUrl));

            // Encode the hash as a base62 string
            var base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var base62 = new StringBuilder();
            foreach (var b in hash)
            {
                base62.Append(base62Chars[b % 62]);
            }

            // Return the first 7 characters of the base62 string as the short URL
            return base62.ToString().Substring(0, 7);
        }
    }
}
