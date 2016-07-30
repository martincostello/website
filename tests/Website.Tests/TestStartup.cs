// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using MyTested.AspNetCore.Mvc;
    using NodaTime;
    using NodaTime.Testing;

    /// <summary>
    /// A class representing the <see cref="Startup"/> class to use for tests.
    /// </summary>
    public class TestStartup : Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestStartup"/> class.
        /// </summary>
        /// <param name="env">The <see cref="IHostingEnvironment"/> to use.</param>
        public TestStartup(IHostingEnvironment env)
            : base(env)
        {
        }

        /// <summary>
        /// Adds a fake <see cref="IClock"/> implementation to the specified service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the clock to.</param>
        /// <param name="instant">The optional instant to use for the clock.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> passed to <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection AddFakeClock(IServiceCollection services, Instant? instant = null)
        {
            var initial = instant ?? SystemClock.Instance.GetCurrentInstant();
            return services.ReplaceSingleton<IClock>((_) => new FakeClock(initial));
        }
    }
}
