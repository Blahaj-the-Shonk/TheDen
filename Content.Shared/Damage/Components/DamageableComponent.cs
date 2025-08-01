// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández
// SPDX-FileCopyrightText: 2021 metalgearsloth
// SPDX-FileCopyrightText: 2022 Alex Evgrashin
// SPDX-FileCopyrightText: 2022 Leon Friedrich
// SPDX-FileCopyrightText: 2022 Moony
// SPDX-FileCopyrightText: 2022 Paul Ritter
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto
// SPDX-FileCopyrightText: 2022 mirrorcult
// SPDX-FileCopyrightText: 2022 wrexbe
// SPDX-FileCopyrightText: 2023 DrSmugleaf
// SPDX-FileCopyrightText: 2024 SimpleStation14
// SPDX-FileCopyrightText: 2024 VMSolidus
// SPDX-FileCopyrightText: 2024 slarticodefast
// SPDX-FileCopyrightText: 2025 RedFoxIV
// SPDX-FileCopyrightText: 2025 Sir Warock
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Damage
{
    /// <summary>
    ///     Component that allows entities to take damage.
    /// </summary>
    /// <remarks>
    ///     The supported damage types are specified using a <see cref="DamageContainerPrototype"/>s. DamageContainers
    ///     may also have resistances to certain damage types, defined via a <see cref="DamageModifierSetPrototype"/>.
    /// </remarks>
    [RegisterComponent]
    [NetworkedComponent()]
    [Access(typeof(DamageableSystem), Other = AccessPermissions.ReadExecute)]
    public sealed partial class DamageableComponent : Component
    {
        /// <summary>
        ///     This <see cref="DamageContainerPrototype"/> specifies what damage types are supported by this component.
        ///     If null, all damage types will be supported.
        /// </summary>
        [DataField("damageContainer", customTypeSerializer: typeof(PrototypeIdSerializer<DamageContainerPrototype>))]
        public string? DamageContainerID;

        /// <summary>
        ///     This <see cref="DamageModifierSetPrototype"/> will be applied to any damage that is dealt to this container,
        ///     unless the damage explicitly ignores resistances.
        /// </summary>
        /// <remarks>
        ///     Though DamageModifierSets can be deserialized directly, we only want to use the prototype version here
        ///     to reduce duplication.
        /// </remarks>
        [DataField("damageModifierSet", customTypeSerializer: typeof(PrototypeIdSerializer<DamageModifierSetPrototype>))]
        public string? DamageModifierSetId;

        /// <summary>
        ///     List of all Modifier Sets stored by this entity. The above single format is a deprecated function used only to support legacy yml.
        /// </summary>
        [DataField]
        public List<string> DamageModifierSets = new();

        /// <summary>
        ///     All the damage information is stored in this <see cref="DamageSpecifier"/>.
        /// </summary>
        /// <remarks>
        ///     If this data-field is specified, this allows damageable components to be initialized with non-zero damage.
        /// </remarks>
        [DataField("damage", readOnly: true)] //todo remove this readonly when implementing writing to damagespecifier
        public DamageSpecifier Damage = new();

        /// <summary>
        ///     Damage, indexed by <see cref="DamageGroupPrototype"/> ID keys.
        /// </summary>
        /// <remarks>
        ///     Groups which have no members that are supported by this component will not be present in this
        ///     dictionary.
        /// </remarks>
        [ViewVariables] public Dictionary<string, FixedPoint2> DamagePerGroup = new();

        /// <summary>
        ///     The sum of all damages in the DamageableComponent.
        /// </summary>
        [ViewVariables]
        public FixedPoint2 TotalDamage;

        [DataField("radiationDamageTypes", customTypeSerializer: typeof(PrototypeIdListSerializer<DamageTypePrototype>))]
        public List<string> RadiationDamageTypeIDs = new() { "Radiation" };

        [DataField]
        public Dictionary<MobState, ProtoId<HealthIconPrototype>> HealthIcons = new()
        {   // Den: Nuked Alive Icon so speech bubbles are visible
            { MobState.Critical, "HealthIconCritical" },
            { MobState.Dead, "HealthIconDead" },
        };

        [DataField]
        public ProtoId<HealthIconPrototype> RottingIcon = "HealthIconRotting";

        [DataField]
        public FixedPoint2? HealthBarThreshold;
    }

    [Serializable, NetSerializable]
    public sealed class DamageableComponentState : ComponentState
    {
        public readonly Dictionary<string, FixedPoint2> DamageDict;
        public readonly string? ModifierSetId;

        public DamageableComponentState(
            Dictionary<string, FixedPoint2> damageDict,
            string? modifierSetId)
        {
            DamageDict = damageDict;
            ModifierSetId = modifierSetId;
        }
    }
}
