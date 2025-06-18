using Microsoft.AspNetCore.Mvc;
using TutorialProjectAPI.Contexts;
using TutorialProjectAPI.Models;
using TutorialProjectAPI.Repositories;

namespace TutorialProjectAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IIdentifiableRepository<ImageDB> _imagesRepo;

    public ImagesController(IIdentifiableRepository<ImageDB> imagesRepo)
    {
        _imagesRepo = imagesRepo;
    }

    // GET api/Images/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        // no tracking needed for read-only streaming
        var img = await _imagesRepo.GetByIdAsync(id);   // ← repo, not _db

        if (img is null) return NotFound();

        return File(img.Data, img.ContentType);
    }
}
