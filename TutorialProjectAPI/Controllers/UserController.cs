using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutorialProjectAPI.Contexts;
using TutorialProjectAPI.Dtos;
using TutorialProjectAPI.Models;
using TutorialProjectAPI.Repositories;

namespace TutorialProjectAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IIdentifiableRepository<UserDB> _userRepository;
        private readonly MainContext _context;

        private const long AvatarLimitBytes = 524_288;   // 512 KB

        public UserController(IIdentifiableRepository<UserDB> userRepository,
                              MainContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        // ────────────── CRUD ──────────────

        // POST api/User
        [HttpPost]
        public async Task<IActionResult> Create(UserDB user)
        {
            user.Id = Guid.NewGuid();
            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        // GET api/User
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _userRepository.GetAllAsync());

        // GET api/User/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        // PUT api/User/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UserDB updated)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null) return NotFound();

            user.Username = updated.Username;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
            return NoContent();
        }

        // DELETE api/User/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null) return NotFound();

            _userRepository.Delete(user);
            await _userRepository.SaveAsync();
            return NoContent();
        }

        // ────────────── Avatar upload ──────────────

        // PUT api/User/{id}/avatar
        [HttpPut("{id:guid}/avatar")]
        [RequestSizeLimit(AvatarLimitBytes)]
        public async Task<IActionResult> UploadAvatar(Guid id,
            [FromForm] AvatarUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("No file supplied.");
            if (dto.File.Length > AvatarLimitBytes)
                return BadRequest("Avatar exceeds 512 KB.");

            // tracked load via repository
            var user = await _userRepository.GetByIdAsync(id, track: true);
            if (user == null) return NotFound();

            await using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);

            var img = new ImageDB
            {
                Id = Guid.NewGuid(),
                Data = ms.ToArray(),
                ContentType = dto.File.ContentType,
                Size = dto.File.Length
            };

            _context.Images.Add(img);   // insert image row
            user.Avatar = img;          // sets FK

            await _context.SaveChangesAsync();   // tracked, so no concurrency error

            return Ok(new ImageMetaDto(img.Id, img.ContentType, img.Size));
        }

    }
}
