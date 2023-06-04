namespace Content_Delivery.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string? Title { get; set; } = "";
        public string? Description { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; }
        public string? OriginalUrl { get; set; }
        public string? ShortUrl { get; set; }

    }
}
