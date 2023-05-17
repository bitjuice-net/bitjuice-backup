using BitJuice.Backup.Model;
using Octokit;

namespace BitJuice.Backup.Modules.Providers;

public class GithubConfig : IModuleConfig
{
    public AuthenticationType AuthType { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    
    public void Validate()
    {
    }
}