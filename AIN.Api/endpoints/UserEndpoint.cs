using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using AIN.Infrastructrue.Context;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;

namespace AIN.Api.Endpoints
{
    public static class UserEndpoint
    {
        public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder app)
        {
            // Authorities
            app.MapGet("/api/authorities", async (
                [FromServices] IAuthorityRepo authorities,
                
                CancellationToken ct) =>
            {

                var list = await authorities.ListAsync(ct);
                var response = list.Select(a => new { a.Id, a.Name, a.Department });

                
                return Results.Ok(response);
            });


            // Trust Points
            app.MapPost("/api/users/{id:guid}/trustpoints", async (
                Guid id,
                int delta,
                [FromServices] ITrustPointsService trust,
                
                CancellationToken ct) =>
            {
                var total = await trust.AddPointsAsync(id, delta, ct);


                return Results.Ok(new { userId = id, total });
            }) ;




            

            app.MapPost("/api/reports/{id:guid}/attachments", async (
                Guid id,
                IFormFile file,
                [FromServices] IReportRepo reportsRepo,
                [FromServices] AinDbContext db,
                [FromServices] IConfiguration config,
               
                CancellationToken ct) =>
            {
                if (file == null || file.Length == 0) return Results.BadRequest("Empty file");

                var report = await reportsRepo.GetByIdAsync(id, includeAttachments: false, ct);
                if (report == null) return Results.NotFound();

                var uploads = config.GetValue<string>("Storage:UploadsRoot") ?? "wwwroot/uploads";
                Directory.CreateDirectory(uploads);

                var safeName = Path.GetFileName(file.FileName);
                var storedName = $"{Guid.NewGuid()}_{safeName}";
                var fullPath = Path.Combine(uploads, storedName);

                await using (var stream = File.Create(fullPath))
                    await file.CopyToAsync(stream, ct);

                var attachment = new AIN.Core.Entites.Attachment
                {
                    Id = Guid.NewGuid(),
                    ReportId = id,
                    FileName = safeName,
                    ContentType = file.ContentType,
                    SizeBytes = file.Length,
                    StoragePath = fullPath
                };

                db.Attachments.Add(attachment);
                await db.SaveChangesAsync(ct);

                var url = $"/uploads/{storedName}";

             

                return Results.Created(url, new { attachment.Id, url });
            })

            .DisableAntiforgery(); 


            return app;
        }
    }
}
