// SPDX-FileCopyrightText: 2023 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Instruments.UI;

[Serializable, NetSerializable]
public sealed class InstrumentBandRequestBuiMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class InstrumentBandResponseBuiMessage : BoundUserInterfaceMessage
{
    public (NetEntity, string)[] Nearby { get; set; }

    public InstrumentBandResponseBuiMessage((NetEntity, string)[] nearby)
    {
        Nearby = nearby;
    }
}
