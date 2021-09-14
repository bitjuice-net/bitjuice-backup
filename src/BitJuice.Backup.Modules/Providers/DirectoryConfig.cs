using System.Collections.Generic;
using System.Linq;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Providers
{
    public class DirectoryConfig : IModuleConfig
    {
        public List<string> Paths { get; set; }
        public List<string> Excludes { get; set; }
        public List<RewriteRule> Rewrites { get; set; }
        public string Pattern { get; set; }
        public bool TopOnly { get; set; }

        public void Validate()
        {
            if (Paths == null || !Paths.Any())
                throw new InvalidConfigException<DirectoryConfig>("Paths is required");

            if (Rewrites != null)
            {
                var duplicates = Rewrites.GroupBy(i => i.From).Where(i => i.Count() > 1).ToList();
                if (duplicates.Any())
                {
                    var message = "Duplicated rewrite rules: " + string.Join(", ", duplicates.Select(i => i.Key));
                    throw new InvalidConfigException<DirectoryConfig>(message);
                }
            }
        }
    }
}