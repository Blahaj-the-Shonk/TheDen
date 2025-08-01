// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Content.Client.ContextMenu.UI;
using Content.Client.Examine;
using Content.Client.Guidebook.Richtext;
using Content.Client.Verbs.UI;
using Content.Shared.Input;
using Content.Shared.Tag;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Client.Guidebook.Controls;

/// <summary>
///     Control for embedding an entity into a guidebook/document. This is effectively a sprite-view that supports
///     examination, interactions, and captions.
/// </summary>
[GenerateTypedNameReferences]
public sealed partial class GuideEntityEmbed : BoxContainer, IDocumentTag
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _systemManager = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    private readonly TagSystem _tagSystem;
    private readonly ExamineSystem _examineSystem;
    private readonly GuidebookSystem _guidebookSystem;

    public bool Interactive;

    public Entity<SpriteComponent>? Sprite => View.Entity == null || View.Sprite == null
        ? null
        : (View.Entity.Value, View.Sprite);

    public Vector2 Scale
    {
        get => View.Scale;
        set => View.Scale = value;
    }

    public GuideEntityEmbed()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        _tagSystem = _systemManager.GetEntitySystem<TagSystem>();
        _examineSystem = _systemManager.GetEntitySystem<ExamineSystem>();
        _guidebookSystem = _systemManager.GetEntitySystem<GuidebookSystem>();
        MouseFilter = MouseFilterMode.Stop;
    }

    public GuideEntityEmbed(string proto, bool caption, bool interactive) : this()
    {
        Interactive = interactive;

        var ent = _entityManager.SpawnEntity(proto, MapCoordinates.Nullspace);
        View.SetEntity(ent);

        if (caption)
            Caption.Text = _entityManager.GetComponent<MetaDataComponent>(ent).EntityName;
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);
        // get an entity associated with this element
        var entity = Sprite?.Owner;

        // Deleted() automatically checks for null & existence.
        if (_entityManager.Deleted(entity))
            return;

        // do examination?
        if (args.Function == ContentKeyFunctions.ExamineEntity)
        {
            _examineSystem.DoExamine(entity.Value,
                userOverride: _guidebookSystem.GetGuidebookUser());
            args.Handle();
            return;
        }

        if (!Interactive)
            return;

        // open verb menu?
        if (args.Function == EngineKeyFunctions.UseSecondary)
        {
            _ui.GetUIController<VerbMenuUIController>().OpenVerbMenu(entity.Value);
            args.Handle();
            return;
        }

        // from here out we're faking interactions! sue me. --moony

        if (args.Function == ContentKeyFunctions.ActivateItemInWorld)
        {
            _guidebookSystem.FakeClientActivateInWorld(entity.Value);
            _ui.GetUIController<ContextMenuUIController>().Close();
            args.Handle();
            return;
        }

        if (args.Function == ContentKeyFunctions.AltActivateItemInWorld)
        {
            _guidebookSystem.FakeClientAltActivateInWorld(entity.Value);
            _ui.GetUIController<ContextMenuUIController>().Close();
            args.Handle();
            return;
        }

        if (args.Function == ContentKeyFunctions.AltActivateItemInWorld)
        {
            _guidebookSystem.FakeClientUse(entity.Value);
            _ui.GetUIController<ContextMenuUIController>().Close();
            args.Handle();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (Sprite is not null)
            _entityManager.DeleteEntity(Sprite);
    }

    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        if (!args.TryGetValue("Entity", out var proto))
        {
            Logger.Error("Entity embed tag is missing entity prototype argument");
            control = null;
            return false;
        }

        var ent = _entityManager.SpawnEntity(proto, MapCoordinates.Nullspace);

        _tagSystem.AddTag(ent, GuidebookSystem.GuideEmbedTag);
        View.SetEntity(ent);

        if (!args.TryGetValue("Caption", out var caption))
            caption = _entityManager.GetComponent<MetaDataComponent>(ent).EntityName;

        if (!string.IsNullOrEmpty(caption))
            Caption.Text = caption;
        // else:
        //   caption text already defaults to null

        if (args.TryGetValue("Scale", out var scaleStr))
        {
            var scale = float.Parse(scaleStr, CultureInfo.InvariantCulture);
            Scale = new Vector2(scale, scale);
        }
        else
        {
            Scale = new Vector2(2, 2);
        }

        if (args.TryGetValue("Interactive", out var interactive))
            Interactive = bool.Parse(interactive);

        if (args.TryGetValue("Rotation", out var rotation))
        {
            View.OverrideDirection = Angle.FromDegrees(double.Parse(rotation)).GetDir();
        }

        if (args.TryGetValue("Margin", out var margin))
        {
            Margin = ParseThickness(margin);
        }
        else
        {
            Margin = new Thickness(4, 8);
        }

        // By default, we will map-initialize guidebook entities.
        if (!args.TryGetValue("Init", out var mapInit) || !bool.Parse(mapInit))
            _entityManager.RunMapInit(ent, _entityManager.GetComponent<MetaDataComponent>(ent));

        control = this;
        return true;
    }

    private static Thickness ParseThickness(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return default;

        var split = value.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(x => Parse.Float(x)).ToArray();
        if (split.Length == 1)
            return new Thickness(split[0]);
        if (split.Length == 2)
            return new Thickness(split[0], split[1]);
        if (split.Length == 4)
            return new Thickness(split[0], split[1], split[2], split[3]);

        throw new Exception("Invalid Thickness format!");
    }
}
