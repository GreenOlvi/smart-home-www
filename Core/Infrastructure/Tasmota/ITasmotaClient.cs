﻿using System.Text.Json;
using CSharpFunctionalExtensions;

namespace SmartHomeWWW.Core.Infrastructure.Tasmota;

public interface ITasmotaClient
{
    Task<Maybe<JsonDocument>> ExecuteCommandAsync(string command, string value);
    Task<Maybe<JsonDocument>> GetValueAsync(string command);
}
