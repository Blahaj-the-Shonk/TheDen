// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2024 sleepyyapril <***>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.BodyEffects.Subsystems;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GenerateChildPartComponent : Component
{

    [DataField(required: true)]
    public EntProtoId Id = "";

    [DataField, AutoNetworkedField]
    public EntityUid? ChildPart;

    [DataField]
    public bool Active = false;
}