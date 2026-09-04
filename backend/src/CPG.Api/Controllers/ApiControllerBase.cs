using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Shared base for all API controllers.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Placeholder response for Phase 1 scaffolding. Handlers land with their user stories
    /// (US-01..US-04).
    /// </summary>
    protected ObjectResult NotImplementedYet(string userStory) => Problem(
        title: "Not implemented",
        detail: $"This endpoint is scaffolded and will be implemented with {userStory}.",
        statusCode: StatusCodes.Status501NotImplemented);
}
