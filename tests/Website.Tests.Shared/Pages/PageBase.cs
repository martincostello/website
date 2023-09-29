// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Pages;

public abstract class PageBase(ApplicationNavigator navigator)
{
    protected ApplicationNavigator Navigator { get; } = navigator;

    protected abstract string RelativeUri { get; }

    internal async Task NavigateToSelfAsync()
        => await Navigator.NavigateToAsync(RelativeUri);
}
