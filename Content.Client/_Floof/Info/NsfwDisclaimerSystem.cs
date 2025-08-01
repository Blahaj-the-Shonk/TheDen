// SPDX-FileCopyrightText: 2025 Mnemotechnican
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FloofStation.Info;
using Robust.Shared.Network;


namespace Content.Client._Floof.Info;


public sealed class NsfwDisclaimerSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    private NsfwDisclaimerWindow? _window;

    public override void Initialize()
    {
        SubscribeNetworkEvent<ShowNsfwPopupDisclaimerMessage>(OnShowPopup);
        _net.Disconnect += (_, _) => _window?.Close();
    }

    private void OnShowPopup(ShowNsfwPopupDisclaimerMessage message)
    {
        _window?.Close();

        _window = new();
        _window.OpenCentered();
        _window.OnClose += () =>
        {
            _window.OnWindowClosed();
            _window = null;
        };
    }
}
