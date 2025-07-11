// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BlitzTheSquishy <73762869+BlitzTheSquishy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.VendingMachines;

/// <summary>
/// Similar to <c>VendingMachineInventoryPrototype</c> but for <see cref="ShopVendorComponent"/>.
/// </summary>
[Prototype]
public sealed class ShopInventoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The item listings for sale.
    /// </summary>
    [DataField(required: true)]
    public List<ShopListing> Listings = new();
}

[DataRecord, Serializable]
public record struct ShopListing(EntProtoId Id, uint Cost);
