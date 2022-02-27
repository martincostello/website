// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.EndToEnd;

/// <summary>
/// The base class for end-to-end tests.
/// </summary>
[Collection(WebsiteCollection.Name)]
[Trait("Category", "EndToEnd")]
public abstract class EndToEndTest : UITest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EndToEndTest"/> class.
    /// </summary>
    /// <param name="fixture">The fixture to use.</param>
    /// <param name="outputHelper">The test output helper to use.</param>
    protected EndToEndTest(WebsiteFixture fixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Fixture = fixture;
    }

    /// <summary>
    /// Gets the <see cref="WebsiteFixture"/> to use.
    /// </summary>
    protected WebsiteFixture Fixture { get; }

    /// <inheritdoc />
    protected override Uri ServerAddress => Fixture.ServerAddress!;
}
