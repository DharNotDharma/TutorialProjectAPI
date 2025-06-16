using System.ComponentModel.DataAnnotations;

namespace TutorialProjectAPI.Models
{
    public class PostDB : IIdentifiableDB
    {
        public Guid Id { get; set; }

        [Required, MaxLength(4000)]
        public string Body { get; set; } = string.Empty;

        /* ─── Foreign-key + navigation ─── */
        public Guid UserId { get; set; }
        public UserDB? User { get; set; }       // <- add this

        public Guid? AttachmentId { get; set; }
        public ImageDB Attachment { get; set; }

        public List<ReplyDB> Replies { get; set; } = new();
        
    }
}
