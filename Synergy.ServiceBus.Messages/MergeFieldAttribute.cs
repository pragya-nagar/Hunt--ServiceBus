using System;

namespace Synergy.ServiceBus.Messages
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MergeFieldAttribute : Attribute
    {
        public MergeFieldAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public string FormatString { get; set; }
    }
}
