using SmartHomeWWW.Core.Firmwares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeWWW.Core.ViewModel;

public record struct FirmwareViewModel
{
    public FirmwareVersion Version { get; init; }
    public long Size { get; init; }
    public UpdateChannel Channel { get; init; }

    public static FirmwareViewModel From(IFirmware firmware) => new()
    {
        Version = firmware.Version,
        Size = firmware.Size,
        Channel = firmware.Channel,
    };
}
