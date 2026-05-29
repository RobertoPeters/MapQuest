using MapQuest.Data;
using MapQuest.Interfaces;
using MapQuest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace MapQuest.UserService;

public static class Api
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] FilteredDataRequest dataRequest, [FromServices] IUserService userService, ClaimsPrincipal user) =>
        {
            var users = await userService.GetUsersAsync(dataRequest);
            return Results.Ok(users);
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapPost("/login", async ([FromBody] LoginRequest loginRequest, [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            var user = await signInManager.UserManager.FindByEmailAsync(loginRequest.Email);
            if (user == null)
            {
                user = await signInManager.UserManager.FindByNameAsync(loginRequest.Email);
            }

            if (user != null)
            {
                var result = await signInManager.PasswordSignInAsync(user, loginRequest.Password, true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return Results.Ok();
                }
            }

            return Results.BadRequest(new { error = "Ongeldige loginpoging." });
        })
        .AllowAnonymous();

        group.MapGet("/logout", async ([FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Ok();
        });

        return app;
    }
}