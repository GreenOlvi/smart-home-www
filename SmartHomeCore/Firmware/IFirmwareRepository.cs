using System;
using System.Collections.Generic;
using System.IO;
using SmartHomeCore.Domain;

namespace SmartHomeCore.Firmwares
{
    public interface IFirmwareRepository
    {
        IEnumerable<Firmware> GetAllFirmwares();
        bool TryGetCurrentVersion(out Version version);
        Version GetCurrentVersion();
        Stream GetCurrentFirmware();
    }
}
