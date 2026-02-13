using System;
using System.Collections.Generic;

namespace ProductMonitoring.API.Models;

public partial class SolutionHistory
{
    public long Id { get; set; }

    public string BitAddress { get; set; }

    public int? CategoryId { get; set; }

    public bool IsExistingSolution { get; set; }
    public string? Description { get; set; }

    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }

    public bool IsOpen { get; set; } = true;

    public string? File     { get; set; }
}
