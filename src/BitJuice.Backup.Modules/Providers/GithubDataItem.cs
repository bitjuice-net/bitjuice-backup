using System;
using System.Collections.Generic;
using System.IO;
using BitJuice.Backup.Model;
using Octokit;

namespace BitJuice.Backup.Modules.Providers;

public class GithubDataItem : IDataItem
{
    private readonly GitHubClient client;
    private readonly Repository repository;
    public string Name { get; }
    public string VirtualPath { get; }

    public GithubDataItem(GitHubClient client, Repository repository)
    {
        this.client = client;
        this.repository = repository;
        Name = $"{repository.Owner.Name}/{repository.Name}.zip";
        VirtualPath = $"github/{repository.Owner.Login}/{repository.Name}.zip";
    }

    // TODO: add async support
    public Stream GetStream()
    {
        var stream = new MemoryStream();

        var url = $"{repository.HtmlUrl}/archive/refs/heads/{repository.DefaultBranch}.zip";
        var response = client.Connection.GetRaw(new Uri(url), null).GetAwaiter().GetResult();
        stream.Write(response.Body);

        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}