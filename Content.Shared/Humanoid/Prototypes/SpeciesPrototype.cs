// SPDX-FileCopyrightText: 2022 EmoGarbage404
// SPDX-FileCopyrightText: 2022 Flipp Syder
// SPDX-FileCopyrightText: 2022 Moony
// SPDX-FileCopyrightText: 2022 Nemanja
// SPDX-FileCopyrightText: 2022 Paul Ritter
// SPDX-FileCopyrightText: 2022 Rane
// SPDX-FileCopyrightText: 2022 T-Stalker
// SPDX-FileCopyrightText: 2022 ZeroDayDaemon
// SPDX-FileCopyrightText: 2022 mirrorcult
// SPDX-FileCopyrightText: 2022 wrexbe
// SPDX-FileCopyrightText: 2023 Adrian16199
// SPDX-FileCopyrightText: 2023 Colin-Tel
// SPDX-FileCopyrightText: 2023 DrSmugleaf
// SPDX-FileCopyrightText: 2023 Fluffiest Floofers
// SPDX-FileCopyrightText: 2023 PHCodes
// SPDX-FileCopyrightText: 2023 Visne
// SPDX-FileCopyrightText: 2024 Aiden
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT
// SPDX-FileCopyrightText: 2024 Errant
// SPDX-FileCopyrightText: 2024 FoxxoTrystan
// SPDX-FileCopyrightText: 2024 Memeji
// SPDX-FileCopyrightText: 2024 Timemaster99
// SPDX-FileCopyrightText: 2024 VMSolidus
// SPDX-FileCopyrightText: 2024 metalgearsloth
// SPDX-FileCopyrightText: 2025 Blitz
// SPDX-FileCopyrightText: 2025 Skubman
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Dataset;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Humanoid.Prototypes;

[Prototype("species")]
public sealed partial class SpeciesPrototype : IPrototype
{
    /// <summary>
    /// Prototype ID of the species.
    /// </summary>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// User visible name of the species.
    /// </summary>
    [DataField(required: true)]
    public string Name { get; private set; } = default!;

    /// <summary>
    ///     Descriptor. Unused...? This is intended
    ///     for an eventual integration into IdentitySystem
    ///     (i.e., young human person, young lizard person, etc.)
    /// </summary>
    [DataField]
    public string Descriptor { get; private set; } = "humanoid";

    /// <summary>
    /// Whether the species is available "at round start" (In the character editor)
    /// </summary>
    [DataField(required: true)]
    public bool RoundStart { get; private set; } = false;

    // The below two are to avoid fetching information about the species from the entity
    // prototype.

    // This one here is a utility field, and is meant to *avoid* having to duplicate
    // the massive SpriteComponent found in every species.
    // Species implementors can just override SpriteComponent if they want a custom
    // sprite layout, and leave this null. Keep in mind that this will disable
    // sprite accessories.

    [DataField("sprites")]
    public ProtoId<HumanoidSpeciesBaseSpritesPrototype> SpriteSet { get; private set; } = default!;

    /// <summary>
    ///     Default skin tone for this species. This applies for non-human skin tones.
    /// </summary>
    [DataField]
    public Color DefaultSkinTone { get; private set; } = Color.White;

    /// <summary>
    ///     Default human skin tone for this species. This applies for human skin tones.
    ///     See <see cref="SkinColor.HumanSkinTone"/> for the valid range of skin tones.
    /// </summary>
    [DataField]
    public int DefaultHumanSkinTone { get; private set; } = 20;

    /// <summary>
    ///     The limit of body markings that you can place on this species.
    /// </summary>
    [DataField("markingLimits")]
    public ProtoId<MarkingPointsPrototype> MarkingPoints { get; private set; } = default!;

    /// <summary>
    ///     Humanoid species variant used by this entity.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Prototype { get; private set; }

    /// <summary>
    /// Prototype used by the species for the dress-up doll in various menus.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId DollPrototype { get; private set; }

    /// <summary>
    /// Allow Custom Specie Name for this Specie.
    /// </summary>
    [DataField]
    public Boolean CustomName { get; private set; } = false;

    /// <summary>
    /// Method of skin coloration used by the species.
    /// </summary>
    [DataField(required: true)]
    public HumanoidSkinColor SkinColoration { get; private set; }

    [DataField]
    public ProtoId<LocalizedDatasetPrototype> MaleFirstNames { get; private set; } = "NamesFirstMale";

    [DataField]
    public ProtoId<LocalizedDatasetPrototype> FemaleFirstNames { get; private set; } = "NamesFirstFemale";

    [DataField]
    public ProtoId<LocalizedDatasetPrototype> LastNames { get; private set; } = "NamesLast";

    [DataField]
    public SpeciesNaming Naming { get; private set; } = SpeciesNaming.FirstLast;

    [DataField]
    public List<Sex> Sexes { get; private set; } = new() { Sex.Male, Sex.Female };

    /// <summary>
    ///     Characters younger than this are too young to be hired by Nanotrasen.
    /// </summary>
    [DataField]
    public int MinAge = 18;

    /// <summary>
    ///     Characters younger than this appear young.
    /// </summary>
    [DataField]
    public int YoungAge = 30;

    /// <summary>
    ///     Characters older than this appear old. Characters in between young and old age appear middle aged.
    /// </summary>
    [DataField]
    public int OldAge = 60;

    /// <summary>
    ///     Characters cannot be older than this. Only used for restrictions...
    ///     although imagine if ghosts could age people WYCI...
    /// </summary>
    [DataField]
    public int MaxAge = 120;

    /// <summary>
    ///     The minimum height and width ratio for this species
    /// </summary>
    [DataField]
    public float SizeRatio = 1.2f;

    /// <summary>
    ///     The minimum height for this species
    /// </summary>
    [DataField]
    public float MinHeight = 0.6f;

    /// <summary>
    ///     The default height for this species
    /// </summary>
    [DataField]
    public float DefaultHeight = 1f;

    /// <summary>
    ///     The maximum height for this species
    /// </summary>
    [DataField]
    public float MaxHeight = 1.7f;

    /// <summary>
    ///     The minimum width for this species
    /// </summary>
    [DataField]
    public float MinWidth = 0.6f;

    /// <summary>
    ///     The default width for this species
    /// </summary>
    [DataField]
    public float DefaultWidth = 1f;

    /// <summary>
    ///     The maximum width for this species
    /// </summary>
    [DataField]
    public float MaxWidth = 1.7f;

    /// <summary>
    ///     The average height in centimeters for this species, used to calculate player facing height values in UI elements
    /// </summary>
    [DataField]
    public float AverageHeight = 176.1f;

    /// <summary>
    ///     The average shoulder-to-shoulder width in cm for this species, used to calculate player facing width values in UI elements
    /// </summary>
    [DataField]
    public float AverageWidth = 40f;
}

public enum SpeciesNaming : byte
{
    First,
    FirstLast,
    FirstDashFirst,
    //Start of Nyano - Summary: for Oni naming
    LastNoFirst,
    //End of Nyano - Summary: for Oni naming
    TheFirstofLast,
    FirstDashLast,
    LastFirst, // DeltaV
    FirstRoman,
}
