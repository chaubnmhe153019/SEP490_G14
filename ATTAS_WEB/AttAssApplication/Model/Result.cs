using System;
using System.Collections.Generic;

namespace AttAssApplication.Model;

public partial class Result
{
    public int ResultId { get; set; }

    public int SolutionId { get; set; }

    public int TaskId { get; set; }

    public int InstructorId { get; set; }

    public int TimeId { get; set; }

    public virtual Instructor Instructor { get; set; }

    public virtual Solution Solution { get; set; }

    public virtual Task Task { get; set; }

    public virtual Time Time { get; set; }
}
