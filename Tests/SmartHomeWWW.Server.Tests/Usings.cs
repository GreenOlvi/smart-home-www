﻿global using System;
global using System.Collections.Generic;
global using System.IO.Abstractions.TestingHelpers;
global using System.Linq;
global using System.Text;
global using System.Text.Json;
global using System.Threading.Tasks;
global using NUnit.Framework;
global using FluentAssertions;
global using Moq;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;
global using static SmartHomeWWW.Server.Tests.SmartHomeDbTestContextFactory;
