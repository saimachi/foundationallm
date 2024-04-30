﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using FoundationaLLM.Core.Examples.Utils;
using Xunit.Abstractions;
using Environment = FoundationaLLM.Core.Examples.Utils.Environment;
using FoundationaLLM.Core.Examples.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace FoundationaLLM.Core.Examples
{
    public abstract class BaseTest(ITestOutputHelper output, IServiceProvider serviceProvider)
    {
		protected ITestOutputHelper Output { get; } = output;
		protected IServiceProvider ServiceProvider { get; } = serviceProvider;
		protected ILoggerFactory LoggerFactory { get; } = new XunitLogger(output);

		/// <summary>
		/// Service locator to get services from the ServiceProvider.
		/// </summary>
		/// <typeparam name="T">The type of service to retrieve.</typeparam>
		/// <returns></returns>
		protected T GetService<T>() where T : notnull
		{
			return ServiceProvider.GetRequiredService<T>();
		}

		/// <summary>
		/// This method can be substituted by Console.WriteLine when used in Console apps.
		/// </summary>
		/// <param name="target">Target object to write</param>
		protected void WriteLine(object? target = null)
		{
			this.Output.WriteLine((string)(target ?? string.Empty));
		}

		/// <summary>
		/// Current interface ITestOutputHelper does not have a Write method. This extension method adds it to make it analogous to Console.Write when used in Console apps.
		/// </summary>
		/// <param name="target">Target object to write</param>
		protected void Write(object? target = null)
		{
			this.Output.WriteLine((string)(target ?? string.Empty));
		}
	}
}
