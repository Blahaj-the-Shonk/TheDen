// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <flyingkarii@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays.Switchable;
using Robust.Client.Graphics;
using Robust.Shared.Timing;

namespace Content.Client.Overlays.Switchable;

public sealed class ThermalVisionSystem : EquipmentHudSystem<ThermalVisionComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private ThermalVisionOverlay _thermalOverlay = default!;
    private BaseSwitchableOverlay<ThermalVisionComponent> _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThermalVisionComponent, SwitchableOverlayToggledEvent>(OnToggle);

        _thermalOverlay = new ThermalVisionOverlay();
        _overlay = new BaseSwitchableOverlay<ThermalVisionComponent>();
    }

    protected override void OnRefreshComponentHud(EntityUid uid,
        ThermalVisionComponent component,
        RefreshEquipmentHudEvent<ThermalVisionComponent> args)
    {
        if (component.IsEquipment)
            return;

        base.OnRefreshComponentHud(uid, component, args);
    }

    protected override void OnRefreshEquipmentHud(EntityUid uid,
        ThermalVisionComponent component,
        InventoryRelayedEvent<RefreshEquipmentHudEvent<ThermalVisionComponent>> args)
    {
        if (!component.IsEquipment)
            return;

        base.OnRefreshEquipmentHud(uid, component, args);
    }

    private void OnToggle(Entity<ThermalVisionComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        RefreshOverlay(args.User);
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<ThermalVisionComponent> args)
    {
        base.UpdateInternal(args);
        ThermalVisionComponent? tvComp = null;
        var lightRadius = 0f;
        foreach (var comp in args.Components)
        {
            if (!comp.IsActive && (comp.PulseTime <= 0f || comp.PulseAccumulator >= comp.PulseTime))
                continue;

            if (tvComp == null)
                tvComp = comp;
            else if (!tvComp.DrawOverlay && comp.DrawOverlay)
                tvComp = comp;
            else if (tvComp.DrawOverlay == comp.DrawOverlay && tvComp.PulseTime > 0f && comp.PulseTime <= 0f)
                tvComp = comp;

            lightRadius = MathF.Max(lightRadius, comp.LightRadius);
        }

        UpdateThermalOverlay(tvComp, lightRadius);
        UpdateOverlay(tvComp);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        _thermalOverlay.ResetLight(false);
        UpdateOverlay(null);
        UpdateThermalOverlay(null, 0f);
    }

    private void UpdateThermalOverlay(ThermalVisionComponent? comp, float lightRadius)
    {
        _thermalOverlay.LightRadius = lightRadius;
        _thermalOverlay.Comp = comp;

        switch (comp)
        {
            case not null when !_overlayMan.HasOverlay<ThermalVisionOverlay>():
                _overlayMan.AddOverlay(_thermalOverlay);
                break;
            case null:
                _overlayMan.RemoveOverlay(_thermalOverlay);
                _thermalOverlay.ResetLight();
                break;
        }
    }

    private void UpdateOverlay(ThermalVisionComponent? tvComp)
    {
        _overlay.Comp = tvComp;

        switch (tvComp)
        {
            case { DrawOverlay: true } when !_overlayMan.HasOverlay<BaseSwitchableOverlay<ThermalVisionComponent>>():
                _overlayMan.AddOverlay(_overlay);
                break;
            case null or { DrawOverlay: false }:
                _overlayMan.RemoveOverlay(_overlay);
                break;
        }

        // Night vision overlay is prioritized
        _overlay.IsActive = !_overlayMan.HasOverlay<BaseSwitchableOverlay<NightVisionComponent>>();
    }
}
