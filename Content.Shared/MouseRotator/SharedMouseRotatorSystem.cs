// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Interaction;

namespace Content.Shared.MouseRotator;

/// <summary>
/// This handles rotating an entity based on mouse location
/// </summary>
/// <see cref="MouseRotatorComponent"/>
public abstract class SharedMouseRotatorSystem : EntitySystem
{
    [Dependency] private readonly RotateToFaceSystem _rotate = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<RequestMouseRotatorRotationEvent>(OnRequestRotation);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // TODO maybe `ActiveMouseRotatorComponent` to avoid querying over more entities than we need?
        // (if this is added to players)
        // (but arch makes these fast anyway, so)
        var query = EntityQueryEnumerator<MouseRotatorComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var rotator, out var xform))
        {
            if (rotator.GoalRotation == null)
                continue;

            if (_rotate.TryRotateTo(
                    uid,
                    rotator.GoalRotation.Value,
                    frameTime,
                    rotator.AngleTolerance,
                    MathHelper.DegreesToRadians(rotator.RotationSpeed),
                    xform))
            {
                // Stop rotating if we finished
                rotator.GoalRotation = null;
                Dirty(uid, rotator);
            }
        }
    }

    private void OnRequestRotation(RequestMouseRotatorRotationEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } ent
            || !TryComp<MouseRotatorComponent>(ent, out var rotator))
        {
            Log.Error($"User {args.SenderSession.Name} ({args.SenderSession.UserId}) tried setting local rotation directly without a valid mouse rotator component attached!");
            return;
        }

        rotator.GoalRotation = msg.Rotation;
        Dirty(ent, rotator);
    }
}
