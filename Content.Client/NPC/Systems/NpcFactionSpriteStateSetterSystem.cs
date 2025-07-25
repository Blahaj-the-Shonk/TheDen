// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 stellar-novas <stellar_novas@riseup.net>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT


using Content.Shared.NPC.Components;
using Content.Shared.NPC.Events;
using Robust.Client.GameObjects;
using Robust.Shared.Reflection;

namespace Content.Client.NPC.Systems;
public sealed partial class NpcFactionSpriteStateSetterSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NpcFactionMemberComponent, NpcFactionAddedEvent>(OnFactionAdded);
    }

    private void OnFactionAdded(Entity<NpcFactionMemberComponent > entity, ref NpcFactionAddedEvent args)
    {
        if (!_entityManager.HasComponent(entity, typeof(NpcFactionSpriteStateSetterComponent)))
            return;

        SpriteComponent spriteComponent = _entityManager.GetComponent<SpriteComponent>(entity);

        var rsi = spriteComponent.LayerGetActualRSI(0);

        if(rsi != null && rsi.TryGetState(args.FactionID, out _))
            spriteComponent.LayerSetState(0, new Robust.Client.Graphics.RSI.StateId(args.FactionID));
    }
}
