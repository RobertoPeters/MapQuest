using MapQuest.Interfaces;
using MapQuest.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
            var quests = await questService.GetUserQuestsAsync(dataRequest, userId);
            return Results.Ok(quests);
        });

        group.MapPost("/add", async ([FromBody] QuestAndQuestDescription quest, [FromServices] IQuestService questService, ClaimsPrincipal user) =>
        {
            await questService.AddQuestAsync(user.FindFirst(ClaimTypes.NameIdentifier)!.Value, quest.Quest, quest.QuestDescription);
            return Results.Ok(quest);
        });

        group.MapDelete("/delete", async (string questId, string? userId, [FromServices] IQuestService questService, ClaimsPrincipal user) =>
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
            await questService.DeleteQuestAsync(userId, questId);
            return Results.Ok();
        });

        group.MapGet("/description", async (string questId, string? userId, [FromServices] IQuestService questService, ClaimsPrincipal user) =>
        {
            var questDescription = await questService.GetQuestDescriptionAsync(questId, userId ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            return Results.Ok(questDescription);
        });

        return app;
    }
}