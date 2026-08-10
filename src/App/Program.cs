using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string Conn() => Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING")
    ?? throw new InvalidOperationException("STORAGE_CONNECTION_STRING not set");

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapPut("/notes/{id}", async (string id, HttpRequest req) =>
{
    var container = new BlobContainerClient(Conn(), "notes");
    await container.CreateIfNotExistsAsync();
    using var reader = new StreamReader(req.Body);
    await container.GetBlobClient(id).UploadAsync(
        BinaryData.FromString(await reader.ReadToEndAsync()), overwrite: true);
    return Results.NoContent();
});

app.MapGet("/notes/{id}", async (string id) =>
{
    var blob = new BlobContainerClient(Conn(), "notes").GetBlobClient(id);
    if (!await blob.ExistsAsync()) return Results.NotFound();
    var content = await blob.DownloadContentAsync();
    return Results.Text(content.Value.Content.ToString());
});

app.Run();
