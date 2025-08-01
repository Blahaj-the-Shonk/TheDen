// SPDX-FileCopyrightText: 2024 778b <33431126+778b@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Hands.Components;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Returns true if the active hand entity has the specified components.
/// </summary>
public sealed partial class HungryPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField(required: true)]
    public HungerThreshold MinHungerState = HungerThreshold.Starving;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        if (!blackboard.TryGetValue<EntityUid>(NPCBlackboard.Owner, out var owner, _entManager))
        {
            return false;
        }

        return _entManager.TryGetComponent<HungerComponent>(owner, out var hunger) ? hunger.CurrentThreshold <= MinHungerState : false;
    }
}
