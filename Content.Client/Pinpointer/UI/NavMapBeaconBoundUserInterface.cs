// SPDX-FileCopyrightText: 2023 Nemanja <98561806+emogarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 mr-bo-jangles <mr-bo-jangles@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Pinpointer;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Pinpointer.UI;

[UsedImplicitly]
public sealed class NavMapBeaconBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private NavMapBeaconWindow? _window;

    public NavMapBeaconBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<NavMapBeaconWindow>();

        if (EntMan.TryGetComponent(Owner, out NavMapBeaconComponent? beacon))
        {
            _window.SetEntity(Owner, beacon);
        }

        _window.OnApplyButtonPressed += (label, enabled, color) =>
        {
            SendMessage(new NavMapBeaconConfigureBuiMessage(label, enabled, color));
        };
    }
}
