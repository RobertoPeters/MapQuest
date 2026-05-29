using MapQuest.Interfaces;
using MapQuest.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace MapQuest.UserService;

public static class Api
{
    public static IEndpointRouteBuilder MapQuestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quest").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] FilteredDataRequest dataRequest, string? userId, [FromServices] IQuestService questService, ClaimsPrincipal user) =>
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                //check role Admin
                if (!user.IsInRole("Admin"))
                {
                    return Results.Forbid();
                }
            }
            else
            {
                userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            }
            var users = await questService.GetUserQuestsAsync(dataRequest, userId);
            return Results.Ok(users);
        });


        return app;
    }
}