// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.GameTicking.Rules.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Storage;

namespace Content.Server.GameTicking.Rules;

public sealed class SubGamemodesSystem : GameRuleSystem<SubGamemodesComponent>
{
    protected override void Added(EntityUid uid, SubGamemodesComponent comp, GameRuleComponent rule, GameRuleAddedEvent args)
    {
        var picked = EntitySpawnCollection.GetSpawns(comp.Rules, RobustRandom);
        foreach (var id in picked)
        {
            Log.Info($"Starting gamerule {id} as a subgamemode of {ToPrettyString(uid):rule}");
            GameTicker.AddGameRule(id);
        }
    }
}
