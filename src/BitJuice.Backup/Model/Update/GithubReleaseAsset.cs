using System.Text.Json.Serialization;

namespace BitJuice.Backup.Model.Update;

public class GithubReleaseAsset
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; }
}