using AIN.Application.Dtos;
using AIN.Application.Interfaces.IServices;
using static AIN.Core.Enums.enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AIN.Application.Helpers;

namespace AIN.Api.endpoints
{
    public static class ReportEndpoints
    {
        public static RouteGroupBuilder MapReportEndpoints(this RouteGroupBuilder app)
        {
            app.MapPost("/api/reports", async (
                ReportCreateRequest req,
                [FromServices] IReportService reports,
                CancellationToken ct) =>
            {
                var entity = await reports.CreateAsync(req, ct);
                return Results.Created($"/api/reports/{entity.Id}", new { entity.Id });
            });

            app.MapGet("/api/reports/{id:guid}", async (
                Guid id,
                [FromServices] IReportService reports,
                CancellationToken ct) =>
            {
                var entity = await reports.GetAsync(id, ct);
                if (entity == null) return Results.NotFound();

                string ToUrl(string path) => $"/uploads/{System.IO.Path.GetFileName(path)}";
                var response = entity.ToResponse(ToUrl);

                return Results.Ok(response);
            });

            app.MapGet("/api/feed", async (
                int page,
                int pageSize,
                [FromServices] IReportService reports,
                CancellationToken ct) =>
            {
                page = page == 0 ? 1 : page;
                pageSize = pageSize == 0 ? 20 : Math.Min(pageSize, 100);

                var list = await reports.GetPublicFeedAsync(page, pageSize, ct);
                string ToUrl(string path) => $"/uploads/{System.IO.Path.GetFileName(path)}";
                var response = list.Select(r => r.ToResponse(ToUrl));

                return Results.Ok(response);
            });

            app.MapPatch("/api/reports/{id:guid}/status", async (
                Guid id,
                ReportStatus status,
                [FromServices] IReportService reports,
                CancellationToken ct) =>
            {
                await reports.UpdateStatusAsync(id, status, ct);
                return Results.NoContent();
            });

            app.MapGet("/api/reports/{id:guid}/interactions", async (
                Guid id,
                HttpContext context,
                [FromServices] IReportService reports,
                CancellationToken ct) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid? currentUserId = Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : null;

                var report = await reports.GetWithInteractionsAsync(id, currentUserId, ct);
                if (report == null) return Results.NotFound();

                return Results.Ok(report);
            });

            app.MapPut("/api/reports/{id:guid}", async (
                Guid id,
                ReportUpdateRequest request,
                HttpContext context,
                [FromServices] IReportService reports,
                CancellationToken ct) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var report = await reports.UpdateAsync(id, request, userId, ct);
                if (report == null) return Results.NotFound();

                return Results.Ok(new { message = "Report updated successfully" });
            }).RequireAuthorization();

            app.MapDelete("/api/reports/{id:guid}", async (
                Guid id,
                HttpContext context,
                [FromServices] IReportService reports,
                CancellationToken ct) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var success = await reports.DeleteAsync(id, userId, ct);
                if (!success) return Results.NotFound();

                return Results.NoContent();
            }).RequireAuthorization();

            app.MapPost("/api/reports/{id:guid}/like", async (
                Guid id,
                HttpContext context,
                [FromServices] ILikeService likeService,
                CancellationToken ct) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var likeRequest = new LikeRequest { ReportId = id };
                var isLiked = await likeService.ToggleLikeAsync(userId, likeRequest, ct);

                return Results.Ok(new { isLiked, message = isLiked ? "Report liked" : "Report unliked" });
            }).RequireAuthorization();

            app.MapPost("/api/reports/{id:guid}/comments", async (
                Guid id,
                CommentRequest request,
                HttpContext context,
                [FromServices] ICommentService commentService,
                CancellationToken ct) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                request.ReportId = id;
                var comment = await commentService.AddCommentAsync(userId, request, ct);

                return Results.Created($"/api/comments/{comment.Id}", comment);
            }).RequireAuthorization();

            app.MapGet("/api/reports/{id:guid}/comments", async (
                Guid id,
                int page,
                int pageSize,
                [FromServices] ICommentService commentService,
                CancellationToken ct) =>
            {
                page = page == 0 ? 1 : page;
                pageSize = pageSize == 0 ? 20 : Math.Min(pageSize, 100);

                var skip = (page - 1) * pageSize;
                var comments = await commentService.GetCommentsAsync(id, skip, pageSize, ct);
                return Results.Ok(comments);
            });

            app.MapPut("/api/comments/{commentId:guid}", async (
                Guid commentId,
                CommentUpdateRequest request,
                HttpContext context,
                [FromServices] ICommentService commentService,
                CancellationToken ct) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var comment = await commentService.UpdateCommentAsync(userId, commentId, request, ct);
                if (comment == null) return Results.NotFound();

                return Results.Ok(comment);
            }).RequireAuthorization();

            app.MapDelete("/api/comments/{commentId:guid}", async (
                Guid commentId,
                HttpContext context,
                [FromServices] ICommentService commentService,
                CancellationToken ct) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var success = await commentService.DeleteCommentAsync(userId, commentId, ct);
                if (!success) return Results.NotFound();

                return Results.NoContent();
            }).RequireAuthorization();

            app.MapGet("/api/users/{userId:guid}/reports", async (
                Guid userId,
                int page,
                int pageSize,
                [FromServices] IReportService reports,
                CancellationToken ct) =>
            {
                page = page == 0 ? 1 : page;
                pageSize = pageSize == 0 ? 20 : Math.Min(pageSize, 100);

                var userReports = await reports.GetUserReportsAsync(userId, page, pageSize, ct);
                return Results.Ok(userReports);
            });

            return app;
        }
    }
}
