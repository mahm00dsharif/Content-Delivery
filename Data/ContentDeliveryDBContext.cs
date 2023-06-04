using Microsoft.EntityFrameworkCore;
using Image = Content_Delivery.Models.Image;

namespace Content_Delivery.Data
{
    public class ContentDeliveryDBContext : DbContext
    {
        public ContentDeliveryDBContext(DbContextOptions<ContentDeliveryDBContext>
            options) : base(options)
        {
        }

        public DbSet<Image> Images { get; set; }
    }
}
