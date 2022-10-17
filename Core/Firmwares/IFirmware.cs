using System;

namespace SmartHomeWWW.Core.Firmwares;

public interface IFirmware
{
    public FirmwareVersion Version { get; }
    public long Size { get; }
    public UpdateChannel Channel { get; }
    Stream GetData();
}
