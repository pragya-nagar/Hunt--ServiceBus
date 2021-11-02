using System;

namespace Synergy.ServiceBus.Messages.Events
{
    public class TaskStatusData
    {
        public TaskStatus PreviousStatus { get; set; }

        public TaskStatus CurrentStatus { get; set; }

        public DateTime? DueDate { get; set; }

        public string UserName { get; set; }

        public string CreatedByName { get; set; }

        public string Email { get; set; }

        public string TaskName { get; set; }

        public Guid EventId { get; set; }

        public string EventNumber { get; set; }

        public bool IsAutoCompleteEvent { get; set; }

        public int LogicType { get; set; }

        public int InstanceNumber { get; set; }
    }
}