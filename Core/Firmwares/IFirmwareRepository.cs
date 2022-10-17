using System;
using System.Collections.Generic;
using System.IO;
using SmartHomeWWW.Core.Domain;

namespace SmartHomeWWW.Core.Firmwares;

public interface IFirmwareRepository
{
    IEnumerable<IFirmware> GetAllFirmwares();

    IEnumerable<IFirmware> GetFirmwares(UpdateChannel channel) => GetAllFirmwares().Where(f => f.Channel == channel);
    IEnumerable<IFirmware> GetFirmwares() => GetFirmwares(UpdateChannel.Stable);

    IFirmware? GetCurrentFirmware(UpdateChannel channel) =>
        GetFirmwares(channel).OrderByDescending(f => f.Version.Prefix).FirstOrDefault();
    IFirmware? GetCurrentFirmware() => GetCurrentFirmware(UpdateChannel.Stable);
}
