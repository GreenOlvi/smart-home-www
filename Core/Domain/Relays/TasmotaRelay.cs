using CSharpFunctionalExtensions;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SmartHomeWWW.Core.Domain.Relays
{
    public class TasmotaRelay : IRelay
    {
        public TasmotaRelay(ITasmotaClient tasmota)
        {
            _tasmota = tasmota;
        }

        private readonly ITasmotaClient _tasmota;

        public async Task<Maybe<bool>> GetStateAsync()
        {
            var response = await _tasmota.GetValueAsync("Power");
            if (!response.HasValue)
            {
                return Maybe.None;
            }

            return StringComparer.InvariantCultureIgnoreCase.Compare(response.Value.ToString(), "on") == 0;
        }

        public Task<bool> SetStateAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ToggleAsync()
        {
            throw new NotImplementedException();
        }
    }
}
