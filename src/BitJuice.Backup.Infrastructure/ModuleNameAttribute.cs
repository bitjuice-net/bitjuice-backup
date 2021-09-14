using System;

namespace BitJuice.Backup.Infrastructure
{
    public class ModuleNameAttribute : Attribute
    {
        public string Name { get; }

        public ModuleNameAttribute(string name)
        {
            if (name == null)
                throw new ArgumentNullException();
            if(string.IsNullOrWhiteSpace(name))
                throw new Exception("Module name cannot be empty.");
            Name = name;
        }
    }
}
