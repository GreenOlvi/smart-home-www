using System;
using System.Collections.Generic;
using System.IO;
using SmartHomeWWW.Core.Domain;

namespace SmartHomeWWW.Core.Firmwares
{
    public interface IFirmwareRepository
    {
        IEnumerable<Firmware> GetAllFirmwares();
        bool TryGetCurrentVersion(out Version version);
        Version GetCurrentVersion();
        Stream GetCurrentFirmware();
    }
}
