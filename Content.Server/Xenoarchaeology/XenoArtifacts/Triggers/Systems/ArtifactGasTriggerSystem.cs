// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Alexander Evgrashin <evgrashin.adl@gmail.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Atmos.EntitySystems;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;

public sealed class ArtifactGasTriggerSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly ArtifactSystem _artifactSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ArtifactGasTriggerComponent, ArtifactNodeEnteredEvent>(OnRandomizeTrigger);
    }

    private void OnRandomizeTrigger(EntityUid uid, ArtifactGasTriggerComponent component, ArtifactNodeEnteredEvent args)
    {
        if (component.ActivationGas != null)
            return;

        var gas = component.PossibleGases[args.RandomSeed % component.PossibleGases.Count];
        component.ActivationGas = gas;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        List<Entity<ArtifactComponent>> toUpdate = new();
        var query = EntityQueryEnumerator<ArtifactGasTriggerComponent, ArtifactComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var trigger, out var artifact, out var transform))
        {
            if (trigger.ActivationGas == null)
                continue;

            var environment = _atmosphereSystem.GetTileMixture((uid, transform));
            if (environment == null)
                continue;

            // check if outside there is enough moles to activate artifact
            var moles = environment.GetMoles(trigger.ActivationGas.Value);
            if (moles < trigger.ActivationMoles)
                continue;

            toUpdate.Add((uid, artifact));
        }

        foreach (var a in toUpdate)
        {
            _artifactSystem.TryActivateArtifact(a, null, a);
        }
    }
}
