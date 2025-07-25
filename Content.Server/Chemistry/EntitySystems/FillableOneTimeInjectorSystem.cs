// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 RedFoxIV <38788538+RedFoxIV@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Abilities.Chitinid;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Forensics;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Stacks;
using Robust.Server.Player;

namespace Content.Server.Chemistry.EntitySystems;

public sealed class FillableOneTimeInjectorSystem : SharedFillableOneTimeInjectorSystem
{
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly ReactiveSystem _reactiveSystem = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private const ChatChannel BlockInjectionDenyChannel = ChatChannel.Emotes;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FillableOneTimeInjectorComponent, InjectorDoAfterEvent>(OnInjectDoAfter);
        SubscribeLocalEvent<FillableOneTimeInjectorComponent, AfterInteractEvent>(OnInjectorAfterInteract);
    }

    private void UseInjector(Entity<FillableOneTimeInjectorComponent> injector, EntityUid target, EntityUid user)
    {
        bool isDrawing = injector.Comp.ToggleState == FillableOneTimeInjectorToggleMode.Draw;

        // Is the one-time use fillable injector already spent?
        if (injector.Comp.ToggleState == FillableOneTimeInjectorToggleMode.Spent)
            return;

        // Handle injecting/drawing for solutions
        if (!isDrawing)
        {
            if (SolutionContainers.TryGetInjectableSolution(target, out var injectableSolution, out _))
            {
                TryInject(injector, target, injectableSolution.Value, user, false);
            }
            else if (SolutionContainers.TryGetRefillableSolution(target, out var refillableSolution, out _))
            {
                TryInject(injector, target, refillableSolution.Value, user, true);
            }
            else if (TryComp<BloodstreamComponent>(target, out var bloodstream))
            {
                TryInjectIntoBloodstream(injector, (target, bloodstream), user);
            }
            else
            {
                Popup.PopupEntity(Loc.GetString("injector-component-cannot-transfer-message",
                    ("target", Identity.Entity(target, EntityManager))), injector, user);
            }
        }
        else if (isDrawing)
        {
            // Draw from a bloodstream, if the target has that
            if (TryComp<BloodstreamComponent>(target, out var stream) &&
                SolutionContainers.ResolveSolution(target, stream.BloodSolutionName, ref stream.BloodSolution))
            {
                TryDraw(injector, (target, stream), stream.BloodSolution.Value, user);
                return;
            }

            // Draw from an object (food, beaker, etc)
            if (SolutionContainers.TryGetDrawableSolution(target, out var drawableSolution, out _))
            {
                TryDraw(injector, target, drawableSolution.Value, user);
            }
            else
            {
                Popup.PopupEntity(Loc.GetString("injector-component-cannot-draw-message",
                    ("target", Identity.Entity(target, EntityManager))), injector.Owner, user);
            }
        }
    }

    private void OnInjectDoAfter(Entity<FillableOneTimeInjectorComponent> entity, ref InjectorDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target == null || entity.Comp.ToggleState == FillableOneTimeInjectorToggleMode.Spent)
            return;

        UseInjector(entity, args.Args.Target.Value, args.Args.User);
        args.Handled = true;
    }

    private void OnInjectorAfterInteract(Entity<FillableOneTimeInjectorComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || entity.Comp.ToggleState == FillableOneTimeInjectorToggleMode.Spent)
            return;

        //Make sure we have the attacking entity
        if (args.Target is not { Valid: true } target || !HasComp<SolutionContainerManagerComponent>(entity))
            return;

        // Is the target a mob? If yes, use a do-after to give them time to respond.
        if (HasComp<MobStateComponent>(target) || HasComp<BloodstreamComponent>(target))
        {
            InjectDoAfter(entity, target, args.User);
            args.Handled = true;
            return;
        }

        UseInjector(entity, target, args.User);
        args.Handled = true;
    }

    /// <summary>
    /// Send informative pop-up messages and wait for a do-after to complete.
    /// </summary>
    private void InjectDoAfter(Entity<FillableOneTimeInjectorComponent> injector, EntityUid target, EntityUid user)
    {
        if (injector.Comp.ToggleState == FillableOneTimeInjectorToggleMode.Spent)
            return;

        if (TryComp<BlockInjectionComponent>(target, out var blockComponent)) // DeltaV
        {
            var msg = Loc.GetString($"injector-component-deny-{blockComponent.BlockReason}");
            Popup.PopupEntity(msg, target, user);

            if (!_playerManager.TryGetSessionByEntity(target, out var session))
                return;

            _chat.ChatMessageToOne(
                BlockInjectionDenyChannel,
                msg,
                msg,
                EntityUid.Invalid,
                false,
                session.Channel);
            return;
        }

        bool isDrawing = injector.Comp.ToggleState == FillableOneTimeInjectorToggleMode.Draw;

        // Create a pop-up for the user
        if (isDrawing)
        {
            Popup.PopupEntity(Loc.GetString("injector-component-drawing-user"), target, user);
        }
        else
        {
            Popup.PopupEntity(Loc.GetString("injector-component-injecting-user"), target, user);
        }

        if (!SolutionContainers.TryGetSolution(injector.Owner, injector.Comp.SolutionName, out _, out var solution))
            return;

        var actualDelay = MathHelper.Max(injector.Comp.Delay, TimeSpan.FromSeconds(1));
        float amountToInject;
        if (isDrawing)
        {
            // additional delay is based on actual volume left to draw in syringe when smaller than transfer amount
            amountToInject = Math.Min(injector.Comp.TransferAmount.Float(), (solution.MaxVolume - solution.Volume).Float());
        }
        else
        {
            // additional delay is based on actual volume left to inject in syringe when smaller than transfer amount
            amountToInject = Math.Min(injector.Comp.TransferAmount.Float(), solution.Volume.Float());
        }

        // Injections take 0.5 seconds longer per 5u of possible space/content
        actualDelay += TimeSpan.FromSeconds(amountToInject / 10);

        var isTarget = user != target;

        if (isTarget)
        {
            // Create a pop-up for the target
            var userName = Identity.Entity(user, EntityManager);
            if (isDrawing)
            {
                Popup.PopupEntity(Loc.GetString("injector-component-drawing-target",
    ("user", userName)), user, target);
            }
            else
            {
                Popup.PopupEntity(Loc.GetString("injector-component-injecting-target",
    ("user", userName)), user, target);
            }

            // Check if the target is incapacitated or in combat mode and modify time accordingly.
            if (MobState.IsIncapacitated(target))
            {
                actualDelay /= 2.5f;
            }
            else if (Combat.IsInCombatMode(target))
            {
                // Slightly increase the delay when the target is in combat mode. Helps prevents cheese injections in
                // combat with fast syringes & lag.
                actualDelay += TimeSpan.FromSeconds(1);
            }

            // Add an admin log, using the "force feed" log type. It's not quite feeding, but the effect is the same.
            if (!isDrawing)
            {
                AdminLogger.Add(LogType.ForceFeed,
                    $"{EntityManager.ToPrettyString(user):user} is attempting to inject {EntityManager.ToPrettyString(target):target} with a solution {SharedSolutionContainerSystem.ToPrettyString(solution):solution}");
            }
            else
            {
                AdminLogger.Add(LogType.ForceFeed,
                    $"{EntityManager.ToPrettyString(user):user} is attempting to draw {injector.Comp.TransferAmount.ToString()} units from {EntityManager.ToPrettyString(target):target}");
            }
        }
        else
        {
            // Self-injections take half as long.
            actualDelay /= 2;

            if (!isDrawing)
            {
                AdminLogger.Add(LogType.Ingestion,
                    $"{EntityManager.ToPrettyString(user):user} is attempting to inject themselves with a solution {SharedSolutionContainerSystem.ToPrettyString(solution):solution}.");
            }
            else
            {
                AdminLogger.Add(LogType.ForceFeed,
                    $"{EntityManager.ToPrettyString(user):user} is attempting to draw {injector.Comp.TransferAmount.ToString()} units from themselves.");
            }
        }

        DoAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, actualDelay, new InjectorDoAfterEvent(), injector.Owner, target: target, used: injector.Owner)
        {
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
            BreakOnDamage = true,
            NeedHand = true,
            BreakOnHandChange = true,
            MovementThreshold = 0.1f,
        });
    }

    private void TryInjectIntoBloodstream(Entity<FillableOneTimeInjectorComponent> injector, Entity<BloodstreamComponent> target,
        EntityUid user)
    {
        // Get transfer amount. May be smaller than _transferAmount if not enough room
        if (!SolutionContainers.ResolveSolution(target.Owner, target.Comp.ChemicalSolutionName,
                ref target.Comp.ChemicalSolution, out var chemSolution) || injector.Comp.ToggleState != FillableOneTimeInjectorToggleMode.Inject)
        {
            Popup.PopupEntity(
                Loc.GetString("injector-component-cannot-inject-message",
                    ("target", Identity.Entity(target, EntityManager))), injector.Owner, user);
            return;
        }

        if (!SolutionContainers.TryGetSolution(injector.Owner, injector.Comp.SolutionName, out var soln,
                out var solution) || solution.Volume == 0)
            return;

        var realTransferAmount = FixedPoint2.Min(injector.Comp.TransferAmount, chemSolution.AvailableVolume);
        if (realTransferAmount <= 0)
        {
            Popup.PopupEntity(
                Loc.GetString("injector-component-cannot-inject-message",
                    ("target", Identity.Entity(target, EntityManager))), injector.Owner, user);
            return;
        }

        // Move units from attackSolution to targetSolution
        var removedSolution = SolutionContainers.SplitSolution(target.Comp.ChemicalSolution.Value, realTransferAmount);

        _blood.TryAddToChemicals(target, removedSolution, target.Comp);

        _reactiveSystem.DoEntityReaction(target, removedSolution, ReactionMethod.Injection);

        Popup.PopupEntity(Loc.GetString("injector-component-inject-success-message",
            ("amount", removedSolution.Volume),
            ("target", Identity.Entity(target, EntityManager))), injector.Owner, user);

        Dirty(injector);
        AfterInject(injector, target);
    }

    private void TryInject(Entity<FillableOneTimeInjectorComponent> injector, EntityUid targetEntity,
        Entity<SolutionComponent> targetSolution, EntityUid user, bool asRefill)
    {
        if (HasComp<BlockInjectionComponent>(targetEntity))  // DeltaV
            return;

        if (injector.Comp.ToggleState == FillableOneTimeInjectorToggleMode.Spent)
            return;

        if (!SolutionContainers.TryGetSolution(injector.Owner, injector.Comp.SolutionName, out var soln,
                out var solution) || solution.Volume == 0)
            return;

        // Get transfer amount. May be smaller than _transferAmount if not enough room
        var realTransferAmount =
            FixedPoint2.Min(injector.Comp.TransferAmount, targetSolution.Comp.Solution.AvailableVolume);

        if (realTransferAmount <= 0)
        {
            Popup.PopupEntity(
                Loc.GetString("injector-component-target-already-full-message",
                    ("target", Identity.Entity(targetEntity, EntityManager))),
                injector.Owner, user);
            return;
        }

        // Move units from attackSolution to targetSolution
        Solution removedSolution;
        if (TryComp<StackComponent>(targetEntity, out var stack))
            removedSolution = SolutionContainers.SplitStackSolution(soln.Value, realTransferAmount, stack.Count);
        else
            removedSolution = SolutionContainers.SplitSolution(soln.Value, realTransferAmount);

        _reactiveSystem.DoEntityReaction(targetEntity, removedSolution, ReactionMethod.Injection);

        if (!asRefill)
            SolutionContainers.Inject(targetEntity, targetSolution, removedSolution);
        else
            SolutionContainers.Refill(targetEntity, targetSolution, removedSolution);

        Popup.PopupEntity(Loc.GetString("injector-component-transfer-success-message",
            ("amount", removedSolution.Volume),
            ("target", Identity.Entity(targetEntity, EntityManager))), injector.Owner, user);

        Dirty(injector);
        AfterInject(injector, targetEntity);
    }

    private void AfterInject(Entity<FillableOneTimeInjectorComponent> injector, EntityUid target)
    {
        // Automatically set syringe to draw after draining it.
        if (SolutionContainers.TryGetSolution(injector.Owner, injector.Comp.SolutionName, out var user,
                out var solution) && solution.Volume == 0)
        {
            SetMode(injector, FillableOneTimeInjectorToggleMode.Spent);
            Popup.PopupClient(Loc.GetString("injector-spent-text"), injector, user);
        }
        // Leave some DNA from the injectee on it
        var ev = new TransferDnaEvent { Donor = target, Recipient = injector };
        RaiseLocalEvent(target, ref ev);
    }

    private void AfterDraw(Entity<FillableOneTimeInjectorComponent> injector, EntityUid target)
    {
        // Automatically set syringe to inject after filling it completely.
        if (SolutionContainers.TryGetSolution(injector.Owner, injector.Comp.SolutionName, out var user,
                out var solution) && solution.AvailableVolume == 0)
        {
            SetMode(injector, FillableOneTimeInjectorToggleMode.Inject);
            Popup.PopupClient(Loc.GetString("injector-component-injecting-locked-text"), injector, user);
        }

        // Leave some DNA from the drawee on it
        var ev = new TransferDnaEvent { Donor = target, Recipient = injector };
        RaiseLocalEvent(target, ref ev);
    }

    private void TryDraw(Entity<FillableOneTimeInjectorComponent> injector, Entity<BloodstreamComponent?> target,
        Entity<SolutionComponent> targetSolution, EntityUid user)
    {
        if (!SolutionContainers.TryGetSolution(injector.Owner, injector.Comp.SolutionName, out var soln,
                out var solution) || solution.AvailableVolume == 0)
        {
            return;
        }

        // Get transfer amount. May be smaller than _transferAmount if not enough room, also make sure there's room in the injector
        var realTransferAmount = FixedPoint2.Min(injector.Comp.TransferAmount, targetSolution.Comp.Solution.Volume,
            solution.AvailableVolume);

        if (realTransferAmount <= 0)
        {
            Popup.PopupEntity(
                Loc.GetString("injector-component-target-is-empty-message",
                    ("target", Identity.Entity(target, EntityManager))),
                injector.Owner, user);
            return;
        }

        // We have some snowflaked behavior for streams.
        if (target.Comp != null)
        {
            DrawFromBlood(injector, (target.Owner, target.Comp), soln.Value, realTransferAmount, user);
            return;
        }

        // Move units from attackSolution to targetSolution
        var removedSolution = SolutionContainers.Draw(target.Owner, targetSolution, realTransferAmount);

        if (!SolutionContainers.TryAddSolution(soln.Value, removedSolution))
        {
            return;
        }

        Popup.PopupEntity(Loc.GetString("injector-component-draw-success-message",
            ("amount", removedSolution.Volume),
            ("target", Identity.Entity(target, EntityManager))), injector.Owner, user);

        Dirty(injector);
        AfterDraw(injector, target);
    }

    private void DrawFromBlood(Entity<FillableOneTimeInjectorComponent> injector, Entity<BloodstreamComponent> target,
        Entity<SolutionComponent> injectorSolution, FixedPoint2 transferAmount, EntityUid user)
    {
        var drawAmount = (float) transferAmount;

        if (SolutionContainers.ResolveSolution(target.Owner, target.Comp.ChemicalSolutionName,
                ref target.Comp.ChemicalSolution))
        {
            var chemTemp = SolutionContainers.SplitSolution(target.Comp.ChemicalSolution.Value, drawAmount * 0.15f);
            SolutionContainers.TryAddSolution(injectorSolution, chemTemp);
            drawAmount -= (float) chemTemp.Volume;
        }

        if (SolutionContainers.ResolveSolution(target.Owner, target.Comp.BloodSolutionName,
                ref target.Comp.BloodSolution))
        {
            var bloodTemp = SolutionContainers.SplitSolution(target.Comp.BloodSolution.Value, drawAmount);
            SolutionContainers.TryAddSolution(injectorSolution, bloodTemp);
        }

        Popup.PopupEntity(Loc.GetString("injector-component-draw-success-message",
            ("amount", transferAmount),
            ("target", Identity.Entity(target, EntityManager))), injector.Owner, user);

        Dirty(injector);
        AfterDraw(injector, target);
    }
}
