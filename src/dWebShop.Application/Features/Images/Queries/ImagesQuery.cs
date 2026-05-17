using MediatR;

namespace dWebShop.Application.Features.Images.Queries
{
    public class ImageFileDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Filename { get; set; } = string.Empty;
    }

    public class GetImagesQuery : IRequest<List<ImageFileDto>>
    {
        public string FolderPath { get; set; } = string.Empty;
        public string SubFolder { get; set; } = string.Empty;
    }

    public class GetImagesQueryHandler : IRequestHandler<GetImagesQuery, List<ImageFileDto>>
    {
        public Task<List<ImageFileDto>> Handle(GetImagesQuery request, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(request.FolderPath))
                return Task.FromResult(new List<ImageFileDto>());

            var files = Directory.GetFiles(request.FolderPath)
                .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                            || file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                            || file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                            || file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                            || file.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                .Select(file => new FileInfo(file))
                .OrderByDescending(f => f.LastWriteTime)
                .Take(100);

            var response = files.Select(f => new ImageFileDto
            {
                Name = f.Name,
                Date = f.LastWriteTime,
                Filename = $"{request.SubFolder}/{f.Name}"
            }).ToList();

            return Task.FromResult(response);
        }
    }
}
