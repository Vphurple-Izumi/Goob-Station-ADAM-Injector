// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.AdamInjector;
using Content.Server.Administration.Systems;
using Content.Server.Popups;
using Content.Shared.Chemistry;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.AdamInjector;

public sealed class AdamInjectorSystem : EntitySystem
{
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AdamInjectorComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<AdamInjectorComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(EntityUid uid, AdamInjectorComponent component, ComponentStartup args)
    {
        // Initialize appearance to show full state
        _appearance.SetData(uid, SolutionContainerVisuals.FillFraction, 1.0f);
    }

    private void OnAfterInteract(EntityUid uid, AdamInjectorComponent component, AfterInteractEvent args)
    {
        if (args.Handled || args.Target == null)
            return;

        // Check if injector is empty
        if (component.IsEmpty)
        {
            _popup.PopupEntity(Loc.GetString("adam-injector-empty"), uid, args.User);
            return;
        }

        // Check if target is dead
        if (!TryComp<MobStateComponent>(args.Target.Value, out var mobState) ||
            !_mobState.IsDead(args.Target.Value, mobState))
        {
            _popup.PopupEntity(Loc.GetString("adam-injector-target-not-dead"), uid, args.User);
            return;
        }

        // Check if target is a player (prevent usage on players)
        if (HasComp<ActorComponent>(args.Target.Value))
        {
            _popup.PopupEntity(Loc.GetString("adam-injector-target-player"), uid, args.User);
            return;
        }

        // Perform rejuvenation
        _rejuvenate.PerformRejuvenate(args.Target.Value);

        // Mark injector as empty
        component.IsEmpty = true;
        Dirty(uid, component);

        // Update appearance to show empty state
        _appearance.SetData(uid, SolutionContainerVisuals.FillFraction, 0.0f);

        // Play sound effect
        _audio.PlayPvs("/Audio/Items/hypospray.ogg", uid);

        // Show success message
        _popup.PopupEntity(Loc.GetString("adam-injector-target-revived", ("target", args.Target.Value)), uid, args.User);

        args.Handled = true;
    }
}
