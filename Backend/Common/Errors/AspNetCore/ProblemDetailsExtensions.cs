using Microsoft.AspNetCore.Mvc;

namespace Backend.Common.Errors.AspNetCore;

public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Extension helpers for <see cref="ProblemDetails"/> instances.
    /// </summary>
    public static IActionResult ToActionResult(this ProblemDetails problemDetails)
    {
        /// <summary>
        /// Converts the given <see cref="ProblemDetails"/> into an <see cref="IActionResult"/>
        /// with an <see cref="ObjectResult"/> and its <see cref="ObjectResult.StatusCode"/>
        /// set to the <see cref="ProblemDetails.Status"/> value.
        /// </summary>
        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };
    }
}
