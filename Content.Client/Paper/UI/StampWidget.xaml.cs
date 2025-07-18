// SPDX-FileCopyrightText: 2023 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2023 eoineoineoin <eoin.mcloughlin+gh@gmail.com>
// SPDX-FileCopyrightText: 2023 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;
using Content.Shared.Paper;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.Paper.UI;

[GenerateTypedNameReferences]
public sealed partial class StampWidget : PanelContainer
{
    private StyleBoxTexture _borderTexture;
    private ShaderInstance? _stampShader;

    public float Orientation
    {
        get => StampedByLabel.Orientation;
        set => StampedByLabel.Orientation = value;
    }

    public StampDisplayInfo StampInfo {
        set {
            StampedByLabel.Text = Loc.GetString(value.StampedName);
            StampedByLabel.FontColorOverride = value.StampedColor;
            ModulateSelfOverride = value.StampedColor;
        }
    }

    public StampWidget()
    {
        RobustXamlLoader.Load(this);
        var resCache = IoCManager.Resolve<IResourceCache>();
        var borderImage = resCache.GetResource<TextureResource>(
                "/Textures/Interface/Paper/paper_stamp_border.svg.96dpi.png");
        _borderTexture = new StyleBoxTexture {
            Texture = borderImage,
        };
        _borderTexture.SetPatchMargin(StyleBoxTexture.Margin.All, 7.0f);
        PanelOverride = _borderTexture;

        var prototypes = IoCManager.Resolve<IPrototypeManager>();
        _stampShader = prototypes.Index<ShaderPrototype>("PaperStamp").InstanceUnique();
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        _stampShader?.SetParameter("objCoord", GlobalPosition * UIScale * new Vector2(1, -1));
        handle.UseShader(_stampShader);
        handle.SetTransform(GlobalPosition * UIScale, Orientation, Vector2.One);
        base.Draw(handle);

        // Restore a sane transform+shader
        handle.SetTransform(Matrix3x2.Identity);
        handle.UseShader(null);
    }
}
