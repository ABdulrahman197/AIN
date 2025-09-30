using AIN.Application.Dtos;
using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIN.Application.Helpers
{
    public static class Mapping
    {
        public static ReportResponse ToResponse(this Report entity, Func<string, string> toPublicUrl)
        {
            var attachments = entity.Attachments
                .Select(a => new AttachmentResponse(a.Id, a.FileName, a.ContentType, a.SizeBytes, toPublicUrl(a.StoragePath)))
                .ToList();
            return new ReportResponse(
                entity.Id,
                entity.Title,
                entity.Description,
                entity.Category,
                entity.Visibility,
                entity.Status,
                entity.Latitude,
                entity.Longitude,
                entity.CreatedAt,
                entity.ReporterId,
                entity.RoutedAuthorityId,
                attachments
            );
        }
    }
}
