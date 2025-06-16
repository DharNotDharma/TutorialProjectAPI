using System.ComponentModel.DataAnnotations;

namespace TutorialProjectAPI.Models
{
    public class ImageDB
    {
        [Key]
        public Guid Id { get; set; }

        public byte[] Data { get; set; } = Array.Empty<byte>();

        public string ContentType { get; set; } = string.Empty;

        public long Size { get; set; }
    }
}
