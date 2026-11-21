using Microsoft.AspNetCore.Mvc;

namespace WebApiSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("search")]
    public IActionResult Search([FromQuery] string? term, [FromQuery] string? filter)
    {
        return Ok(new { Term = term, Filter = filter });
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateUserDto dto)
    {
        return Ok(new
        {
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.Notes
        });
    }

    [HttpPost("feedback")]
    public IActionResult SubmitFeedback([FromForm] FeedbackForm form)
    {
        return Ok(new
        {
            form.Subject,
            form.Comments
        });
    }
}

public class CreateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
}

public class FeedbackForm
{
    public string? Subject { get; set; }
    public string? Comments { get; set; }
}
