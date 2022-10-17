using SmartHomeWWW.Core.Firmwares;

namespace SmartHomeWWW.Server.Firmwares;

public class FileFirmware : IFirmware
{
    private readonly Func<Stream> _dataStreamFactory;

    private FileFirmware()
    {
        _dataStreamFactory = () => throw new InvalidOperationException("Not actually a firmware");
    }

    public FileFirmware(string filename, FirmwareVersion version, long size, UpdateChannel channel, Func<Stream> dataStreamFactory)
    {
        Filename = filename;
        Version = version;
        Size = size;
        Channel = channel;
        _dataStreamFactory = dataStreamFactory;
    }

    public string Filename { get; } = string.Empty;
    public FirmwareVersion Version { get; } = new FirmwareVersion();
    public long Size { get; init; }
    public UpdateChannel Channel { get; init; } = UpdateChannel.Stable;
    public Stream Data => _dataStreamFactory.Invoke();

    public static readonly FileFirmware NullFirmware = new();
}
