using System;
using System.Collections.Generic;

namespace Synergy.ServiceBus.Messages.Events
{
    public class TasksReportItem
    {
        public Guid TaskId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? CompletedDate { get; set; }

        public Guid? CompletedBy { get; set; }

        public string CompletedByName { get; set; }

        public string Name { get; set; }

        public Guid EventId { get; set; }

        public string EventNumber { get; set; }

        public DateTime? DueDate { get; set; }

        public string Notes { get; set; }

        public int NotificationTypes { get; set; }

        public TaskCategory TaskCategory { get; set; }

        public IEnumerable<(Guid, string)> Assignees { get; set; }
    }
}
