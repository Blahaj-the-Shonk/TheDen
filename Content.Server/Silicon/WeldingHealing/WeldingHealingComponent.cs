// SPDX-FileCopyrightText: 2024 Timemaster99 <57200767+Timemaster99@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;
using Content.Shared.Tools;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Silicon.WeldingHealing;

[RegisterComponent]
public sealed partial class WeldingHealingComponent : Component
{
    /// <summary>
    ///     All the damage to change information is stored in this <see cref="DamageSpecifier"/>.
    /// </summary>
    /// <remarks>
    ///     If this data-field is specified, it will change damage by this amount instead of setting all damage to 0.
    ///     in order to heal/repair the damage values have to be negative.
    /// </remarks>

    [DataField(required: true)]
    public DamageSpecifier Damage;

    [DataField(customTypeSerializer:typeof(PrototypeIdSerializer<ToolQualityPrototype>))]
    public string QualityNeeded = "Welding";

    /// <summary>
    ///     The fuel amount needed to repair physical related damage
    /// </summary>
    [DataField]
    public int FuelCost = 5;

    [DataField]
    public int DoAfterDelay = 3;

    /// <summary>
    ///     A multiplier that will be applied to the above if an entity is repairing themselves.
    /// </summary>
    [DataField]
    public float SelfHealPenalty = 3f;

    /// <summary>
    ///     Whether or not an entity is allowed to repair itself.
    /// </summary>
    [DataField]
    public bool AllowSelfHeal = true;

    [DataField(required: true)]
    public List<string> DamageContainers;
}
