using AIN.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIN.Application.Interfaces.IServices
{
    public interface ICommentService
    {
        Task<CommentResponse> AddCommentAsync(Guid userId, CommentRequest request, CancellationToken ct = default);
        Task<CommentResponse?> UpdateCommentAsync(Guid userId, Guid commentId, CommentUpdateRequest request, CancellationToken ct = default);
        Task<bool> DeleteCommentAsync(Guid userId, Guid commentId, CancellationToken ct = default);
        Task<IEnumerable<CommentResponse>> GetCommentsAsync(Guid reportId, int skip, int take, CancellationToken ct = default);
        Task<int> GetCommentCountAsync(Guid reportId, CancellationToken ct = default);
    }
}
