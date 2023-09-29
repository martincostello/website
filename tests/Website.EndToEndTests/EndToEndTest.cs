// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.EndToEnd;

/// <summary>
/// The base class for end-to-end tests.
/// </summary>
[Collection(WebsiteCollection.Name)]
[Trait("Category", "EndToEnd")]
public abstract class EndToEndTest(WebsiteFixture fixture, ITestOutputHelper outputHelper) : UITest(outputHelper)
{
    /// <summary>
    /// Gets the <see cref="WebsiteFixture"/> to use.
    /// </summary>
    protected WebsiteFixture Fixture { get; } = fixture;

    /// <inheritdoc />
    protected override Uri ServerAddress => Fixture.ServerAddress!;
}
