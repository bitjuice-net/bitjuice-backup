using System.Collections.Generic;
using System.Linq;

namespace BitJuice.Backup.Modules.Providers
{
    public class RewriteEngine
    {
        private readonly Dictionary<string, string> rules;
        private readonly List<string> keys;

        public RewriteEngine(IEnumerable<RewriteRule> rules)
        {
            this.rules = rules.OrderByDescending(i => i.From).ToDictionary(i => i.From, i => i.To);
            keys = this.rules.Keys.OrderByDescending(i => i).ToList();
        }

        public string Rewrite(string input)
        {
            foreach (var key in keys)
            {
                if (!input.StartsWith(key))
                    continue;

                var replacement = rules[key];
                return replacement + input.Substring(key.Length);
            }
            return input;
        }
    }
}
