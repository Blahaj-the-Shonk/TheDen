// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Daniel Castro Razo <eldanielcr@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 sleepyyapril <flyingkarii@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.ActionBlocker;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Inventory.VirtualItem;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Shared.Verbs
{
    public abstract class SharedVerbSystem : EntitySystem
    {
        [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
        [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
        [Dependency] protected readonly SharedContainerSystem ContainerSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeAllEvent<ExecuteVerbEvent>(HandleExecuteVerb);
        }

        private void HandleExecuteVerb(ExecuteVerbEvent args, EntitySessionEventArgs eventArgs)
        {
            var user = eventArgs.SenderSession.AttachedEntity;
            if (user == null)
                return;

            if (!TryGetEntity(args.Target, out var target))
                return;

            // It is possible that client-side prediction can cause this event to be raised after the target entity has
            // been deleted. So we need to check that the entity still exists.
            if (Deleted(user))
                return;

            // Get the list of verbs. This effectively also checks that the requested verb is in fact a valid verb that
            // the user can perform.
            var verbs = GetLocalVerbs(target.Value, user.Value, args.RequestedVerb.GetType());

            // Note that GetLocalVerbs might waste time checking & preparing unrelated verbs even though we know
            // precisely which one we want to run. However, MOST entities will only have 1 or 2 verbs of a given type.
            // The one exception here is the "other" verb type, which has 3-4 verbs + all the debug verbs.

            // Find the requested verb.
            if (verbs.TryGetValue(args.RequestedVerb, out var verb))
                ExecuteVerb(verb, user.Value, target.Value);
        }

        /// <summary>
        ///     Raises a number of events in order to get all verbs of the given type(s) defined in local systems. This
        ///     does not request verbs from the server.
        /// </summary>
        public SortedSet<Verb> GetLocalVerbs(EntityUid target, EntityUid user, Type type, bool force = false)
        {
            return GetLocalVerbs(target, user, new List<Type>() { type }, force);
        }

        /// <inheritdoc cref="GetLocalVerbs(Robust.Shared.GameObjects.EntityUid,Robust.Shared.GameObjects.EntityUid,System.Type,bool)"/>
        public SortedSet<Verb> GetLocalVerbs(EntityUid target, EntityUid user, List<Type> types, bool force = false)
        {
            return GetLocalVerbs(target, user, types, out _, force);
        }

        /// <summary>
        ///     Raises a number of events in order to get all verbs of the given type(s) defined in local systems. This
        ///     does not request verbs from the server.
        /// </summary>
        public SortedSet<Verb> GetLocalVerbs(EntityUid target, EntityUid user, List<Type> types,
            out List<VerbCategory> extraCategories, bool force = false)
        {
            SortedSet<Verb> verbs = new();
            extraCategories = new();

            // accessibility checks
            var canAccess = force || _interactionSystem.InRangeAndAccessible(user, target);

            // A large number of verbs need to check action blockers. Instead of repeatedly having each system individually
            // call ActionBlocker checks, just cache it for the verb request.
            var canInteract = force || _actionBlockerSystem.CanInteract(user, target);
            var canComplexInteract = force || _actionBlockerSystem.CanComplexInteract(user);

            _interactionSystem.TryGetUsedEntity(user, out var @using);
            TryComp<HandsComponent>(user, out var hands);

            // TODO: fix this garbage and use proper generics or reflection or something else, not this.
            if (types.Contains(typeof(InteractionVerb)))
            {
                var verbEvent = new GetVerbsEvent<InteractionVerb>(user, target, @using, hands, canInteract: canInteract, canComplexInteract: canComplexInteract, canAccess: canAccess, extraCategories);
                RaiseLocalEvent(target, verbEvent, true);
                verbs.UnionWith(verbEvent.Verbs);
            }

            if (types.Contains(typeof(UtilityVerb))
                && @using != null
                && @using != target)
            {
                var verbEvent = new GetVerbsEvent<UtilityVerb>(user, target, @using, hands, canInteract: canInteract, canComplexInteract: canComplexInteract, canAccess: canAccess, extraCategories);
                RaiseLocalEvent(@using.Value, verbEvent, true); // directed at used, not at target
                verbs.UnionWith(verbEvent.Verbs);
            }

            if (types.Contains(typeof(InnateVerb)))
            {
                var verbEvent = new GetVerbsEvent<InnateVerb>(user, target, @using, hands, canInteract: canInteract, canComplexInteract: canComplexInteract, canAccess: canAccess, extraCategories);
                RaiseLocalEvent(user, verbEvent, true);
                verbs.UnionWith(verbEvent.Verbs);
            }

            if (types.Contains(typeof(AlternativeVerb)))
            {
                var verbEvent = new GetVerbsEvent<AlternativeVerb>(user, target, @using, hands, canInteract: canInteract, canComplexInteract: canComplexInteract, canAccess: canAccess, extraCategories);
                RaiseLocalEvent(target, verbEvent, true);
                verbs.UnionWith(verbEvent.Verbs);
            }

            if (types.Contains(typeof(ActivationVerb)))
            {
                var verbEvent = new GetVerbsEvent<ActivationVerb>(user, target, @using, hands, canInteract: canInteract, canComplexInteract: canComplexInteract, canAccess: canAccess, extraCategories);
                RaiseLocalEvent(target, verbEvent, true);
                verbs.UnionWith(verbEvent.Verbs);
            }

            if (types.Contains(typeof(ExamineVerb)))
            {
                var verbEvent = new GetVerbsEvent<ExamineVerb>(user, target, @using, hands, canInteract: canInteract, canComplexInteract: canComplexInteract, canAccess: canAccess, extraCategories);
                RaiseLocalEvent(target, verbEvent, true);
                verbs.UnionWith(verbEvent.Verbs);
            }

            // generic verbs
            if (types.Contains(typeof(Verb)))
            {
                var verbEvent = new GetVerbsEvent<Verb>(user, target, @using, hands, canInteract: canInteract, canComplexInteract: canComplexInteract, canAccess: canAccess, extraCategories);
                RaiseLocalEvent(target, verbEvent, true);
                verbs.UnionWith(verbEvent.Verbs);
            }

            if (types.Contains(typeof(EquipmentVerb)))
            {
                var access = canAccess || _interactionSystem.CanAccessEquipment(user, target);
                var verbEvent = new GetVerbsEvent<EquipmentVerb>(user, target, @using, hands, canInteract: canInteract, canComplexInteract: canComplexInteract, canAccess: canAccess, extraCategories);
                RaiseLocalEvent(target, verbEvent);
                verbs.UnionWith(verbEvent.Verbs);
            }

            return verbs;
        }

        /// <summary>
        ///     Execute the provided verb.
        /// </summary>
        /// <remarks>
        ///     This will try to call the action delegates and raise the local events for the given verb.
        /// </remarks>
        public virtual void ExecuteVerb(Verb verb, EntityUid user, EntityUid target, bool forced = false)
        {
            // invoke any relevant actions
            verb.Act?.Invoke();

            // Maybe raise a local event
            if (verb.ExecutionEventArgs != null)
            {
                if (verb.EventTarget.IsValid())
                    RaiseLocalEvent(verb.EventTarget, verb.ExecutionEventArgs);
                else
                    RaiseLocalEvent(verb.ExecutionEventArgs);
            }

            if (Deleted(user) || Deleted(target))
                return;

            // Perform any contact interactions
            if (verb.DoContactInteraction ?? (verb.DefaultDoContactInteraction && _interactionSystem.InRangeUnobstructed(user, target)))
                _interactionSystem.DoContactInteraction(user, target);
        }
    }

    // Does nothing on server
    /// <summary>
    /// Raised directed when trying to get the entity menu visibility for entities.
    /// </summary>
    [ByRefEvent]
    public record struct MenuVisibilityEvent
    {
        public MapCoordinates TargetPos;
        public MenuVisibility Visibility;
    }

    // Does nothing on server
    [Flags]
    public enum MenuVisibility
    {
        // What entities can a user see on the entity menu?
        Default = 0,          // They can only see entities in FoV.
        NoFov = 1 << 0,         // They ignore FoV restrictions
        InContainer = 1 << 1,   // They can see through containers.
        Invisible = 1 << 2,   // They can see entities without sprites and the "HideContextMenu" tag is ignored.
        All = NoFov | InContainer | Invisible
    }
}
