using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace RaceDay.Api.Storage;
public sealed class BlobImageStore(IConfiguration configuration) {
    private BlobContainerClient Container(string name) {
        var connection = configuration["AzureBlob:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("AzureBlob:ConnectionString is required for image uploads.");
        return new BlobContainerClient(connection, name);
    }
    public async Task<string> UploadAsync(IFormFile file, string containerName, CancellationToken token) {
        if (file.Length is 0 or > 5_242_880) throw new ArgumentException("Choose an image smaller than 5 MB.");
        var allowed = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) { ["image/jpeg"]=".jpg", ["image/png"]=".png", ["image/webp"]=".webp" };
        if (!allowed.TryGetValue(file.ContentType, out var ext)) throw new ArgumentException("Only JPG, PNG and WEBP images are supported.");
        // Validate the file signature, not only the user-controlled Content-Type.
        await using var input = file.OpenReadStream();
        var header = new byte[12]; var n = await input.ReadAsync(header, token);
        var signatureOk = file.ContentType.ToLowerInvariant() switch {
            "image/jpeg" => n >= 3 && header[0]==0xff && header[1]==0xd8 && header[2]==0xff,
            "image/png" => n >= 8 && header.AsSpan(0,8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}),
            "image/webp" => n >= 12 && System.Text.Encoding.ASCII.GetString(header,0,4)=="RIFF" && System.Text.Encoding.ASCII.GetString(header,8,4)=="WEBP",
            _ => false
        };
        if (!signatureOk) throw new ArgumentException("The uploaded file does not appear to be a valid image.");
        input.Position = 0;
        var container = Container(containerName); await container.CreateIfNotExistsAsync(cancellationToken:token);
        var name = Guid.NewGuid().ToString("N") + ext;
        await container.GetBlobClient(name).UploadAsync(input, new BlobUploadOptions { HttpHeaders=new BlobHttpHeaders { ContentType=file.ContentType } },token);
        return name;
    }
    public async Task<(Stream Stream,string ContentType)?> GetAsync(string containerName,string? name,CancellationToken token) {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var blob=Container(containerName).GetBlobClient(name);
        if (!(await blob.ExistsAsync(token)).Value) return null;
        var response=await blob.DownloadStreamingAsync(cancellationToken:token);
        return (response.Value.Content, response.Value.Details.ContentType ?? "application/octet-stream");
    }
}
