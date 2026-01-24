using System;
using System.Collections.Generic;

namespace ProductMonitoring.API.Models;

public partial class SolutionHistory
{
    public long Id { get; set; }

    public long BitAddressId { get; set; }

    public bool IsExistingSolution { get; set; }
    public string? Description { get; set; }

    public DateTime? CreatedOn { get; set; }
}
