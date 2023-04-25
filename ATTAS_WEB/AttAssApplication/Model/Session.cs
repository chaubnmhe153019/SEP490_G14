using System;
using System.Collections.Generic;

namespace AttAssApplication.Model;

public partial class Session
{
    public int SessionId { get; set; }

    public string SessionHash { get; set; }

    public int StatusId { get; set; }

    public int SolutionCount { get; set; }
    public virtual ICollection<Instructor> Instructors { get; } = new List<Instructor>();

    public virtual ICollection<Solution> Solutions { get; } = new List<Solution>();

    public virtual Status Status { get; set; }

    public virtual ICollection<Task> Tasks { get; } = new List<Task>();

    public virtual ICollection<Time> Times { get; } = new List<Time>();
}
