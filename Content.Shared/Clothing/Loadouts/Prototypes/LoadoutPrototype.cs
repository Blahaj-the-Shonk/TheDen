// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Falcon <falcon@zigtag.dev>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <flyingkarii@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Customization.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared.Clothing.Loadouts.Prototypes;


[Prototype]
public sealed partial class LoadoutPrototype : IPrototype
{
    /// Formatted like "Loadout[Department/ShortHeadName][CommonClothingSlot][SimplifiedClothingId]", example: "LoadoutScienceOuterLabcoatSeniorResearcher"
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public ProtoId<LoadoutCategoryPrototype> Category = "Uncategorized";

    [DataField(required: true)]
    public List<EntProtoId> Items = new();

    /// Components to give each item on spawn
    [DataField]
    public ComponentRegistry Components = new();

    [DataField]
    public int Cost = 1;

    /// Should this item override other items in the same slot
    [DataField]
    public bool Exclusive;

    [DataField]
    public bool CustomName = true;

    [DataField]
    public bool CustomDescription = true;

    [DataField]
    public bool CustomColorTint = false;

    [DataField]
    public bool CanBeHeirloom = false;

    [DataField]
    public List<CharacterRequirement> Requirements = new();

    [DataField]
    public string GuideEntry { get; } = "";

    [DataField(serverOnly: true)]
    public LoadoutFunction[] Functions { get; private set; } = Array.Empty<LoadoutFunction>();
}

/// This serves as a hook for loadout functions to modify one or more entities upon spawning in.
[ImplicitDataDefinitionForInheritors]
public abstract partial class LoadoutFunction
{
    public abstract void OnPlayerSpawn(
        EntityUid character,
        EntityUid loadoutEntity,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager);
}
