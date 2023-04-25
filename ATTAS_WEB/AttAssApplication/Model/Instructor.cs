using System;
using System.Collections.Generic;

namespace AttAssApplication.Model;

public partial class Instructor
{
    public int InstructorId { get; set; }

    public int SessionId { get; set; }

    public string BusinessId { get; set; }

    public int Order { get; set; }

    public virtual ICollection<Result> Results { get; } = new List<Result>();

    public virtual Session Session { get; set; }
}
