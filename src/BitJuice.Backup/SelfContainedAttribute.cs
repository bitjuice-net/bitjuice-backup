using System;

namespace BitJuice.Backup
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class SelfContainedAttribute : Attribute
    {
        public bool SelfContained { get; }

        public SelfContainedAttribute(string selfContained)
        {
            SelfContained = bool.Parse(selfContained);
        }
    }
}
