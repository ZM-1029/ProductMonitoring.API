using System;
using System.Collections.Generic;

namespace ProductMonitoring.API.Models;

public partial class BitAddressRemedy
{
    public long Id { get; set; }

    public long BitAddressId { get; set; }

    public string Remedy { get; set; } = null!;
}
