using dWebShop.Application.Dto.Images;
using MediatR;

namespace dWebShop.Application.Queries.Images;

public class GetImagesQuery : IRequest<GetImagesResult>
{
    public string FolderPath { get; set; } = "";
}

public class GetImagesResult
{
    public bool Successful { get; set; }
    public List<ImageFileDto>? Items { get; set; }
    public string? Error { get; set; }
}

public class GetImagesQueryHandler : IRequestHandler<GetImagesQuery, GetImagesResult>
{
    private static readonly HashSet<string> _imageExts =
        [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".avif"];

    public Task<GetImagesResult> Handle(GetImagesQuery request, CancellationToken ct)
    {
        try
        {
            if (!Directory.Exists(request.FolderPath))
                return Task.FromResult(new GetImagesResult { Successful = true, Items = [] });

            var items = Directory.GetFiles(request.FolderPath)
                .Where(f => _imageExts.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(Path.GetFileName)
                .Select(f =>
                {
                    var fileName = Path.GetFileName(f);
                    var folder = Path.GetFileName(request.FolderPath.TrimEnd('/', '\\'));
                    return new ImageFileDto($"{folder}/{fileName}", Path.GetFileNameWithoutExtension(f));
                })
                .ToList();

            return Task.FromResult(new GetImagesResult { Successful = true, Items = items });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new GetImagesResult { Successful = false, Error = ex.Message });
        }
    }
}
