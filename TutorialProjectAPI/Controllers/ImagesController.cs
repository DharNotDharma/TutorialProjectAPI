using Microsoft.AspNetCore.Mvc;
using TutorialProjectAPI.Contexts;
using TutorialProjectAPI.Models;

namespace TutorialProjectAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly MainContext _db;
    private readonly long _avatarLimit;
    private readonly long _postLimit;

    public ImagesController(MainContext db, IConfiguration cfg)
    {
        _db = db;
        _avatarLimit = cfg.GetValue<long>("ImageLimits:AvatarBytes");
        _postLimit = cfg.GetValue<long>("ImageLimits:PostBytes");
    }

    // GET api/images/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var img = await _db.Images.FindAsync(id);
        if (img is null) return NotFound();
        return File(img.Data, img.ContentType);
    }
}
