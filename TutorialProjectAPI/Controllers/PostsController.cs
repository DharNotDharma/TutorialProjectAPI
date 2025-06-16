using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorialProjectAPI.Contexts;
using TutorialProjectAPI.Dtos;
using TutorialProjectAPI.Models;
using TutorialProjectAPI.Repositories;

namespace TutorialProjectAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IIdentifiableRepository<PostDB> _posts;
        private readonly IIdentifiableRepository<ReplyDB> _replies;
        private readonly MainContext _context;

        private const long PostAttachmentLimit = 2_000_000;   // 2 MB

        public PostsController(IIdentifiableRepository<PostDB> posts,
                               IIdentifiableRepository<ReplyDB> replies,
                               MainContext context)
        {
            _posts = posts;
            _replies = replies;
            _context = context;
        }

        // ────────────── CRUD ──────────────

        // POST  api/Posts           (single post, optional replies)
        [HttpPost]
        public async Task<IActionResult> Create(PostCreateDto dto)
        {
            var post = new PostDB
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Body = dto.Body,
                Replies = dto.Replies?.Select(r => new ReplyDB
                {
                    Id = Guid.NewGuid(),
                    Body = r.Body,
                    UserId = r.UserId,
                    PostId = Guid.Empty   // fixed below
                }).ToList() ?? new List<ReplyDB>()
            };

            // set PostId on each reply
            foreach (var reply in post.Replies)
                reply.PostId = post.Id;

            await _posts.AddAsync(post);
            await _posts.SaveAsync();
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }

        // GET  api/Posts
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _posts.GetAllAsync());

        // GET  api/Posts/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var post = await _posts.GetByIdAsync(id);
            return post is null ? NotFound() : Ok(post);
        }

        // PUT  api/Posts/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, PostCreateDto dto)
        {
            var post = await _posts.GetByIdAsync(id);
            if (post is null) return NotFound();

            post.Body = dto.Body;
            await _posts.SaveAsync();
            return NoContent();
        }

        // DELETE  api/Posts/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var post = await _posts.GetByIdAsync(id);
            if (post is null) return NotFound();

            _posts.Delete(post);
            await _posts.SaveAsync();
            return NoContent();
        }

        // ────────────── Attachment upload ──────────────

        // PUT  api/Posts/{postId}/attachment
        [HttpPut("{postId:guid}/attachment")]
        [RequestSizeLimit(PostAttachmentLimit)]
        public async Task<IActionResult> UploadAttachment(
            Guid postId,
            [FromForm] PostAttachmentUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("No file supplied.");

            if (dto.File.Length > PostAttachmentLimit)
                return BadRequest("Attachment exceeds 2 MB.");

            var post = await _posts.GetByIdAsync(postId);
            if (post is null) return NotFound();

            await using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);

            var img = new ImageDB
            {
                Id = Guid.NewGuid(),
                Data = ms.ToArray(),
                ContentType = dto.File.ContentType,
                Size = dto.File.Length
            };

            post.Attachment = img;
            await _posts.SaveAsync();
            return Ok(new ImageMetaDto(img.Id, img.ContentType, img.Size));
        }
    }
}
