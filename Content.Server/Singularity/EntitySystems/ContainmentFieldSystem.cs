// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <keronshb@live.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Popups;
using Content.Server.Shuttles.Components;
using Content.Server.Singularity.Events;
using Content.Shared.Popups;
using Content.Shared.Singularity.Components;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;

namespace Content.Server.Singularity.EntitySystems;

public sealed class ContainmentFieldSystem : EntitySystem
{
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContainmentFieldComponent, StartCollideEvent>(HandleFieldCollide);
        SubscribeLocalEvent<ContainmentFieldComponent, EventHorizonAttemptConsumeEntityEvent>(HandleEventHorizon);
    }

    private void HandleFieldCollide(EntityUid uid, ContainmentFieldComponent component, ref StartCollideEvent args)
    {
        var otherBody = args.OtherEntity;

        if (HasComp<SpaceGarbageComponent>(otherBody))
        {
            _popupSystem.PopupEntity(Loc.GetString("comp-field-vaporized", ("entity", otherBody)), uid, PopupType.LargeCaution);
            QueueDel(otherBody);
        }

        if (TryComp<PhysicsComponent>(otherBody, out var physics) && physics.Mass <= component.MaxMass && physics.Hard)
        {
            var fieldDir = Transform(uid).WorldPosition;
            var playerDir = Transform(otherBody).WorldPosition;

            _throwing.TryThrow(otherBody, playerDir-fieldDir, baseThrowSpeed: component.ThrowForce);
        }
    }

    private void HandleEventHorizon(EntityUid uid, ContainmentFieldComponent component, ref EventHorizonAttemptConsumeEntityEvent args)
    {
        if(!args.Cancelled && !args.EventHorizon.CanBreachContainment)
            args.Cancelled = true;
    }
}
