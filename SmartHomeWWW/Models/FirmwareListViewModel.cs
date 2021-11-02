using System;
using System.Collections.ObjectModel;

namespace SmartHomeWWW.Models
{
    public class FirmwareListViewModel
    {
        public Version CurrentVersion { get; init; }
        public ReadOnlyCollection<FirmwareViewModel> Firmwares { get; init; }
    }
}
