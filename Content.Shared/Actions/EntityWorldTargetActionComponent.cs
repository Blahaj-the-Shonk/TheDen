// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Actions;

/// <summary>
/// Used on action entities to define an action that triggers when targeting an entity or entity coordinates.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EntityWorldTargetActionComponent : BaseTargetActionComponent
{
    public override BaseActionEvent? BaseEvent => Event;

    /// <summary>
    ///     The local-event to raise when this action is performed.
    /// </summary>
    [DataField]
    [NonSerialized]
    public EntityWorldTargetActionEvent? Event;

    /// <summary>
    /// Determines which entities are valid targets for this action.
    /// </summary>
    /// <remarks>No whitelist check when null.</remarks>
    [DataField] public EntityWhitelist? Whitelist;

    /// <summary>
    /// Whether this action considers the user as a valid target entity when using this action.
    /// </summary>
    [DataField] public bool CanTargetSelf = true;
}

[Serializable, NetSerializable]
public sealed class EntityWorldTargetActionComponentState(
    EntityWorldTargetActionComponent component,
    IEntityManager entManager)
    : BaseActionComponentState(component, entManager)
{
    public EntityWhitelist? Whitelist = component.Whitelist;
    public bool CanTargetSelf = component.CanTargetSelf;
}
