using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BitJuice.Backup.Model.Update
{
    public class GithubRelease
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }
        [JsonPropertyName("assets")]
        public List<GithubReleaseAsset> Assets { get; set; }
    }
}
