// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

/// <summary>
/// Requires that this part is attached to a body for the surgery to be done.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryHasBodyConditionComponent : Component;
