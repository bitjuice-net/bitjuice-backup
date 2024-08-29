using System.Collections.Generic;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Logging;
using Octokit;

namespace BitJuice.Backup.Modules.Providers
{
    [ModuleName("github-provider")]
    public class GithubProvider: ModuleBase<GithubConfig>, IProvider
    {
        private readonly ILogger<GithubProvider> logger;

        public GithubProvider(ILogger<GithubProvider> logger)
        {
            this.logger = logger;
        }

        public async IAsyncEnumerable<IDataItem> GetAsync()
        {
            var productHeader = new ProductHeaderValue("BitJuice.Backup");
            var github = new GitHubClient(productHeader)
            {
                Credentials = Config.AuthType switch
                {
                    AuthenticationType.Anonymous => Credentials.Anonymous,
                    _ => string.IsNullOrWhiteSpace(Config.Login)
                        ? new Credentials(Config.Password, Config.AuthType)
                        : new Credentials(Config.Login, Config.Password, Config.AuthType)
                }
            };
            var repositories = await github.Repository.GetAllForCurrent();
            foreach (var repository in repositories)
            {
                logger.LogInformation("Processing repository: {repository}", repository.FullName);
                yield return new GithubDataItem(github, repository);
            }
        }
    }
}
