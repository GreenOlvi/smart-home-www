using System;
using System.Collections.Generic;
using System.IO;

namespace SmartHomeWWW.Logic.Firmware
{
    public interface IFirmwareRepository
    {
        IEnumerable<Version> GetAllVersions();
        Version GetCurrentVersion();
        Stream GetCurrentFirmware();
    }
}
