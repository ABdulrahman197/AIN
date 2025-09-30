using AIN.Application.Dtos;
using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIN.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepo _commentRepo;

        public CommentService(ICommentRepo commentRepo)
        {
            _commentRepo = commentRepo;
        }

        public async Task<CommentResponse> AddCommentAsync(Guid userId, CommentRequest request, CancellationToken ct = default)
        {
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReportId = request.ReportId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepo.AddAsync(comment, ct);
            await _commentRepo.SaveChangesAsync(ct);

            // Get the comment with user details
            var commentWithUser = await _commentRepo.GetByIdAsync(comment.Id, ct);
            if (commentWithUser == null)
                throw new InvalidOperationException("Failed to create comment");

            return new CommentResponse(
                commentWithUser.Id,
                commentWithUser.ReportId,
                commentWithUser.UserId,
                commentWithUser.User?.DisplayName ?? "Unknown User",
                commentWithUser.Content,
                commentWithUser.CreatedAt,
                commentWithUser.UpdatedAt
            );
        }

        public async Task<CommentResponse?> UpdateCommentAsync(Guid userId, Guid commentId, CommentUpdateRequest request, CancellationToken ct = default)
        {
            var comment = await _commentRepo.GetByIdAsync(commentId, ct);
            if (comment == null || comment.UserId != userId)
                return null;

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _commentRepo.UpdateAsync(comment, ct);

            return new CommentResponse(
                comment.Id,
                comment.ReportId,
                comment.UserId,
                comment.User?.DisplayName ?? "Unknown User",
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt
            );
        }

        public async Task<bool> DeleteCommentAsync(Guid userId, Guid commentId, CancellationToken ct = default)
        {
            var comment = await _commentRepo.GetByIdAsync(commentId, ct);
            if (comment == null || comment.UserId != userId)
                return false;

            await _commentRepo.DeleteAsync(commentId, ct);
            return true;
        }

        public async Task<IEnumerable<CommentResponse>> GetCommentsAsync(Guid reportId, int skip, int take, CancellationToken ct = default)
        {
            var comments = await _commentRepo.GetByReportIdAsync(reportId, skip, take, ct);
            
            return comments.Select(c => new CommentResponse(
                c.Id,
                c.ReportId,
                c.UserId,
                c.User?.DisplayName ?? "Unknown User",
                c.Content,
                c.CreatedAt,
                c.UpdatedAt
            ));
        }

        public async Task<int> GetCommentCountAsync(Guid reportId, CancellationToken ct = default)
        {
            return await _commentRepo.GetCountByReportIdAsync(reportId, ct);
        }
    }
}
