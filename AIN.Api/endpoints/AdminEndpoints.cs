using AIN.Application.Dtos;
using AIN.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIN.Api.Endpoints
{
    public static class AdminEndpoints
    {
        public static RouteGroupBuilder MapAdminEndpoints(this RouteGroupBuilder app)
        {
            // Get all users (Admin only)
            app.MapGet("/users", async (
                int page,
                int pageSize,
                [FromServices] IAdminService adminService,
                CancellationToken ct) =>
            {
                page = page == 0 ? 1 : page;
                pageSize = pageSize == 0 ? 20 : Math.Min(pageSize, 100);
                var skip = (page - 1) * pageSize;

                var users = await adminService.GetAllUsersAsync(skip, pageSize, ct);
                var totalCount = await adminService.GetTotalUsersCountAsync(ct);

                return Results.Ok(new
                {
                    users,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }).RequireAuthorization();

            // Get user by ID (Admin only)
            app.MapGet("/users/{userId:guid}", async (
                Guid userId,
                [FromServices] IAdminService adminService,
                CancellationToken ct) =>
            {
                var user = await adminService.GetUserByIdAsync(userId, ct);
                if (user == null) return Results.NotFound();

                return Results.Ok(user);
            }).RequireAuthorization();

            // Update user (Admin only)
            app.MapPut("/users/{userId:guid}", async (
                Guid userId,
                UserUpdateRequest request,
                [FromServices] IAdminService adminService,
                CancellationToken ct) =>
            {
                var success = await adminService.UpdateUserAsync(userId, request, ct);
                if (!success) return Results.NotFound();

                return Results.Ok(new { message = "User updated successfully" });
            }).RequireAuthorization();

            // Delete user (Admin only)
            app.MapDelete("/users/{userId:guid}", async (
                Guid userId,
                [FromServices] IAdminService adminService,
                CancellationToken ct) =>
            {
                var success = await adminService.DeleteUserAsync(userId, ct);
                if (!success) return Results.NotFound();

                return Results.NoContent();
            }).RequireAuthorization();

            return app;
        }
    }
}
