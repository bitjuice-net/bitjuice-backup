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
            var github = new GitHubClient(new ProductHeaderValue("BitJuice.Backup"))
            {
                Credentials = new Credentials(Config.Token, AuthenticationType.Bearer)
            };
            var repositories = github.Repository.GetAllForCurrent().Result;
            foreach (var repository in repositories)
                yield return new GithubDataItem(github, repository);
        }
    }
}
