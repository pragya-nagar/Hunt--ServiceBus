using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages
{
    public class MailMergeFinishedEvent : Event
    {
        public string Source { get; set; }

        public string TemplateFilePath { get; set; }

        public string ResultPath { get; set; }

        public Guid? DownloadCenterId { get; set; }

        public string DownloadCenterFilePath { get; set; }
    }
}