// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using Xunit;
using Xunit.Abstractions;

namespace MartinCostello.Website.Integration.UI
{
    /// <summary>
    /// The base class for browser integration tests.
    /// </summary>
    [Collection(HttpServerCollection.Name)]
    public abstract class BrowserTest : UITest
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserTest"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
        protected BrowserTest(HttpServerFixture fixture, ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Fixture = fixture;
            Fixture.SetOutputHelper(outputHelper);
        }

        /// <summary>
        /// Gets the <see cref="HttpServerFixture"/> to use.
        /// </summary>
        protected HttpServerFixture Fixture { get; }

        /// <inheritdoc />
        protected override Uri ServerAddress => Fixture.ServerAddress;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Fixture?.ClearOutputHelper();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
