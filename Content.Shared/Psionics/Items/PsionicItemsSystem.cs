// SPDX-FileCopyrightText: 2023 PHCodes <47927305+PHCodes@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShatteredSwords <135023515+ShatteredSwords@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Inventory.Events;
using Content.Shared.Clothing.Components;
using Content.Shared.StatusEffect;

namespace Content.Shared.Abilities.Psionics
{
    public sealed class PsionicItemsSystem : EntitySystem
    {
        [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TinfoilHatComponent, GotEquippedEvent>(OnTinfoilEquipped);
            SubscribeLocalEvent<TinfoilHatComponent, GotUnequippedEvent>(OnTinfoilUnequipped);
            SubscribeLocalEvent<ClothingGrantPsionicPowerComponent, GotEquippedEvent>(OnGranterEquipped);
            SubscribeLocalEvent<ClothingGrantPsionicPowerComponent, GotUnequippedEvent>(OnGranterUnequipped);
        }
        private void OnTinfoilEquipped(EntityUid uid, TinfoilHatComponent component, GotEquippedEvent args)
        {
            // This only works on clothing
            if (!TryComp<ClothingComponent>(uid, out var clothing))
                return;
            // Is the clothing in its actual slot?
            if (!clothing.Slots.HasFlag(args.SlotFlags))
                return;

            var insul = EnsureComp<PsionicInsulationComponent>(args.Equipee);
            insul.Passthrough = component.Passthrough;
            component.IsActive = true;
        }

        private void OnTinfoilUnequipped(EntityUid uid, TinfoilHatComponent component, GotUnequippedEvent args)
        {
            if (!component.IsActive)
                return;

            if (!_statusEffects.HasStatusEffect(uid, "PsionicallyInsulated"))
                RemComp<PsionicInsulationComponent>(args.Equipee);

            component.IsActive = false;
        }

        private void OnGranterEquipped(EntityUid uid, ClothingGrantPsionicPowerComponent component, GotEquippedEvent args)
        {
            // This only works on clothing
            if (!TryComp<ClothingComponent>(uid, out var clothing))
                return;
            // Is the clothing in its actual slot?
            if (!clothing.Slots.HasFlag(args.SlotFlags))
                return;
            // does the user already has this power?
            var componentType = _componentFactory.GetRegistration(component.Power).Type;
            if (EntityManager.HasComponent(args.Equipee, componentType)) return;


            var newComponent = (Component) _componentFactory.GetComponent(componentType);
            newComponent.Owner = args.Equipee;

            EntityManager.AddComponent(args.Equipee, newComponent);

            component.IsActive = true;
        }

        private void OnGranterUnequipped(EntityUid uid, ClothingGrantPsionicPowerComponent component, GotUnequippedEvent args)
        {
            if (!component.IsActive)
                return;

            component.IsActive = false;
            var componentType = _componentFactory.GetRegistration(component.Power).Type;
            if (EntityManager.HasComponent(args.Equipee, componentType))
            {
                EntityManager.RemoveComponent(args.Equipee, componentType);
            }
        }
    }
}
