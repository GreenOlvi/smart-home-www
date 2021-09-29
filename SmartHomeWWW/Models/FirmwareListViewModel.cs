using System.Collections.ObjectModel;

namespace SmartHomeWWW.Models
{
    public class FirmwareListViewModel
    {
        public ReadOnlyCollection<FirmwareViewModel> Firmwares { get; init; }
    }
}
