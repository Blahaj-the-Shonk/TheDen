// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;
using Content.Shared._Shitmed.Targeting; // Shitmed

namespace Content.Server.Damage.Components;

[RegisterComponent]
public sealed partial class DamageUserOnTriggerComponent : Component
{
    [DataField("ignoreResistances")] public bool IgnoreResistances;

    [DataField("damage", required: true)]
    public DamageSpecifier Damage = default!;

    /// <summary>
    /// Shitmed Change: Lets mousetraps, etc. target the feet.
    /// </summary>
    [DataField]
    public TargetBodyPart? TargetPart = TargetBodyPart.Feet;
}