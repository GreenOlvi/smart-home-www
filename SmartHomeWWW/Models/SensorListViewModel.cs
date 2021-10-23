using System.Collections.Generic;

namespace SmartHomeWWW.Models
{
    public class SensorListViewModel
    {
        public IReadOnlyCollection<SensorViewModel> Sensors { get; init; }
    }
}
