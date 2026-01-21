using System;
using System.Collections.Generic;

namespace ProductMonitoring.API.Models;

public partial class BitAddressCause
{
    public long Id { get; set; }

    public long BitAddressId { get; set; }

    public string Cause { get; set; } = null!;
}
