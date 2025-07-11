// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Shipyard.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Shipyard;

[Serializable, NetSerializable]
public enum ShipyardConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ShipyardConsoleState : BoundUserInterfaceState
{
    public readonly int Balance;

    public ShipyardConsoleState(int balance)
    {
        Balance = balance;
    }
}

/// <summary>
/// Ask the server to purchase a vessel.
/// </summary>
[Serializable, NetSerializable]
public sealed class ShipyardConsolePurchaseMessage : BoundUserInterfaceMessage
{
    public readonly ProtoId<VesselPrototype> Vessel;

    public ShipyardConsolePurchaseMessage(string vessel)
    {
        Vessel = vessel;
    }
}
