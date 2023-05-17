using System.Collections.Generic;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Octokit;

namespace BitJuice.Backup.Modules.Providers
{
    [ModuleName("github-provider")]
    public class GithubProvider: ModuleBase<GithubConfig>, IProvider
    {
        public IEnumerable<IDataItem> Get()
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
            var repositories = github.Repository.GetAllForCurrent().Result;
            foreach (var repository in repositories)
                yield return new GithubDataItem(github, repository);
        }
    }
}
