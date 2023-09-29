// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Pages;

public sealed class ToolsPage(ApplicationNavigator navigator) : PageBase(navigator)
{
    protected override string RelativeUri => "/tools/";

    public GuidComponent GuidGenerator() => new(Navigator);

    public HashComponent HashGenerator() => new(Navigator);

    public MachineKeyComponent MachineKeyGenerator() => new(Navigator);
}
