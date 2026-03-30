using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EducAIte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FlashcardController : ControllerBase
{
    private readonly IFlashcardService _flashcardService;
    private readonly IStudentFlashcardService _studentFlashcardService;

    public FlashcardController(IFlashcardService flashcardService, IStudentFlashcardService studentFlashcardService)
    {
        _flashcardService = flashcardService;
        _studentFlashcardService = studentFlashcardService;
    }

    [HttpGet("{sqid}")]
    public async Task<IActionResult> GetById(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FlashcardResponse? flashcard = await _flashcardService.GetBySqidAsync(sqid, studentId, cancellationToken);

        if (flashcard is null)
        {
            return NotFound();
        }

        return Ok(flashcard);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FlashcardResponse> flashcards = await _flashcardService.GetMineAsync(studentId, cancellationToken);
        return Ok(flashcards);
    }

    [HttpGet("me/review-queue")]
    public async Task<IActionResult> GetReviewQueue(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FlashcardReviewItemResponse> reviewQueue = await _studentFlashcardService.GetReviewQueueAsync(studentId, cancellationToken);
        return Ok(reviewQueue);
    }

    [HttpPost("me/review/batch")]
    public async Task<IActionResult> GetReviewBatch([FromBody] GetFlashcardReviewBatchRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FlashcardReviewItemResponse> batch = await _studentFlashcardService.GetNextBatchAsync(studentId, request, cancellationToken);
        return Ok(batch);
    }

    [HttpGet("document/{documentSqid}")]
    public async Task<IActionResult> GetByDocument(string documentSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FlashcardResponse> flashcards = await _flashcardService.GetByDocumentAsync(documentSqid, studentId, cancellationToken);
        return Ok(flashcards);
    }

    [HttpGet("note/{noteSqid}")]
    public async Task<IActionResult> GetByNote(string noteSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FlashcardResponse> flashcards = await _flashcardService.GetByNoteAsync(noteSqid, studentId, cancellationToken);
        return Ok(flashcards);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFlashcardRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FlashcardResponse created = await _flashcardService.CreateAsync(request, studentId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { sqid = created.Sqid }, created);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] CreateBulkFlashcardsRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FlashcardResponse> created = await _flashcardService.CreateBulkAsync(request, studentId, cancellationToken);
        return Ok(created);
    }

    [HttpPut("{sqid}")]
    public async Task<IActionResult> Update(string sqid, [FromBody] UpdateFlashcardRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        bool updated = await _flashcardService.UpdateAsync(sqid, request, studentId, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{sqid}")]
    public async Task<IActionResult> Delete(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        bool deleted = await _flashcardService.DeleteAsync(sqid, studentId, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{sqid}/attempt")]
    public async Task<IActionResult> SubmitAttempt(string sqid, [FromBody] SubmitFlashcardAttemptRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FlashcardAttemptResultResponse result = await _studentFlashcardService.SubmitAttemptAsync(sqid, request, studentId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sqid}/evaluated-attempt")]
    public async Task<IActionResult> SubmitEvaluatedAttempt(
        string sqid,
        [FromBody] SubmitEvaluatedFlashcardAttemptRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        SubmitEvaluatedFlashcardAttemptResponse result =
            await _studentFlashcardService.SubmitEvaluatedAttemptAsync(sqid, request, studentId, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{sqid}/progress")]
    public async Task<IActionResult> GetProgress(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentFlashcardProgressResponse? progress = await _studentFlashcardService.GetProgressAsync(sqid, studentId, cancellationToken);
        if (progress is null)
        {
            return NotFound();
        }

        return Ok(progress);
    }

    [HttpPost("{sqid}/progress/start")]
    public async Task<IActionResult> StartTracking(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentFlashcardProgressResponse progress = await _studentFlashcardService.StartTrackingAsync(sqid, studentId, cancellationToken);
        return Ok(progress);
    }

    [HttpPost("{sqid}/progress/correct")]
    public async Task<IActionResult> RecordCorrect(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentFlashcardProgressResponse progress = await _studentFlashcardService.RecordCorrectAsync(sqid, studentId, cancellationToken);
        return Ok(progress);
    }

    [HttpPost("{sqid}/progress/wrong")]
    public async Task<IActionResult> RecordWrong(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentFlashcardProgressResponse progress = await _studentFlashcardService.RecordWrongAsync(sqid, studentId, cancellationToken);
        return Ok(progress);
    }

    [HttpPost("{sqid}/progress/reset")]
    public async Task<IActionResult> ResetProgress(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentFlashcardProgressResponse progress = await _studentFlashcardService.ResetProgressAsync(sqid, studentId, cancellationToken);
        return Ok(progress);
    }

    [HttpDelete("{sqid}/progress")]
    public async Task<IActionResult> ArchiveProgress(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        await _studentFlashcardService.ArchiveProgressAsync(sqid, studentId, cancellationToken);
        return NoContent();
    }

    private bool TryGetCurrentStudentId(out long studentId)
    {
        string? claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                             User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return long.TryParse(claimValue, out studentId);
    }
}
