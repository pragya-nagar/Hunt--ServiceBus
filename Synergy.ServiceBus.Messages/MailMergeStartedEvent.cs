using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages
{
    public class MailMergeStartedEvent : Event
    {
        public string Source { get; set; }

        public string TemplateFilePath { get; set; }

        public string ResultPath { get; set; }

        public IEnumerable<MergeFields> MergeFields { get; set; }

        public Guid? DownloadCenterId { get; set; }
    }
}
