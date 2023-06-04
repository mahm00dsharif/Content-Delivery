using Content_Delivery.Data;
using Content_Delivery.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp.Formats.Jpeg;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ContentDeliveryDBContext>(options =>
options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


//caching 
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Cache-Control", "public, max-age=31536000");
    context.Response.Headers.Add("Expires", "Sat, 01 Jan 2023 00:00:00 GMT");

    await next();
});


// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer-when-downgrade");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");

    await next();
});

app.UseMiddleware<AntiHotlinkingMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGet("/images/{imageId}/{imageName}", async context =>
     {
         var imageId = context.Request.RouteValues["imageId"].ToString();
         var imageName = context.Request.RouteValues["imageName"].ToString();
         var imagePath = Path.Combine(app.Environment.ContentRootPath, "images", imageId, imageName);
         if (!File.Exists(imagePath))
         {
             context.Response.StatusCode = 404;
             await context.Response.WriteAsync("Image not found.");
             return;
         }
         var image = Image.Load(imagePath);
         image.Mutate(x => x.Resize(new ResizeOptions
         {
             Size = new Size(800, 0),
             Mode = ResizeMode.Max
         }));

         context.Response.ContentType = "image/jpeg";
         //context.Response.Headers.Add("Content-Encoding", "gzip");

         await image.SaveAsync(context.Response.Body, new JpegEncoder());
     });
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment
    .ContentRootPath, "images")),
    RequestPath = "/images"
});


app.Run();
