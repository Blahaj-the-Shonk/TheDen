// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;
using Content.Client.Computer;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.Gateway;
using Content.Shared.Shuttles.BUIStates;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Timing;

namespace Content.Client.Gateway.UI;

[GenerateTypedNameReferences]
public sealed partial class GatewayWindow : FancyWindow,
    IComputerWindow<EmergencyConsoleBoundUserInterfaceState>
{
    private readonly IGameTiming _timing;

    public event Action<NetEntity>? OpenPortal;
    private List<GatewayDestinationData> _destinations = new();

    public NetEntity Owner;

    private NetEntity? _current;
    private TimeSpan _nextReady;
    private TimeSpan _cooldown;

    private TimeSpan _unlockTime;
    private TimeSpan _nextUnlock;

    /// <summary>
    /// Re-apply the state if the timer has elapsed.
    /// </summary>
    private GatewayBoundUserInterfaceState? _lastState;

    /// <summary>
    /// Are we currently waiting on an unlock timer.
    /// </summary>
    private bool _isUnlockPending = true;

    /// <summary>
    /// Are we currently waiting on a cooldown timer.
    /// </summary>
    private bool _isCooldownPending = true;

    public GatewayWindow()
    {
        RobustXamlLoader.Load(this);
        var dependencies = IoCManager.Instance!;
        _timing = dependencies.Resolve<IGameTiming>();

        NextUnlockBar.ForegroundStyleBoxOverride = new StyleBoxFlat(Color.FromHex("#C74EBD"));
    }

    public void SetEntity(NetEntity entity)
    {

    }

    public void UpdateState(GatewayBoundUserInterfaceState state)
    {
        _destinations = state.Destinations;
        _current = state.Current;
        _nextReady = state.NextReady;
        _cooldown = state.Cooldown;
        _unlockTime = state.UnlockTime;
        _nextUnlock = state.NextUnlock;

        _isUnlockPending = _nextUnlock >= _timing.CurTime;
        _isCooldownPending = _nextReady >= _timing.CurTime;

        Container.DisposeAllChildren();

        if (_destinations.Count == 0)
        {
            Container.AddChild(new BoxContainer()
            {
                HorizontalExpand = true,
                VerticalExpand = true,
                Children =
                {
                    new Label()
                    {
                        Text = Loc.GetString("gateway-window-no-destinations"),
                        HorizontalAlignment = HAlignment.Center
                    }
                }
            });
            return;
        }

        var now = _timing.CurTime;

        foreach (var dest in _destinations)
        {
            var ent = dest.Entity;
            var name = dest.Name;
            var locked = dest.Locked && _nextUnlock > _timing.CurTime;

            var box = new BoxContainer()
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                Margin = new Thickness(5f, 5f),
            };

            // HOW DO I ALIGN THESE GOODER
            var nameLabel = new RichTextLabel()
            {
                VerticalAlignment = VAlignment.Center,
                SetWidth = 156f,
            };

            nameLabel.SetMessage(name);
            box.AddChild(nameLabel);
            // Buffer
            box.AddChild(new Control()
            {
                HorizontalExpand = true,
            });

            bool Pressable() => ent == _current || ent == Owner;

            var buttonStripe = new StripeBack()
            {
                Visible = locked,
                HorizontalExpand = true,
                VerticalExpand = true,
                Margin = new Thickness(10f, 0f, 0f, 0f),
                Children =
                {
                    new Label()
                    {
                        Text = Loc.GetString("gateway-window-locked"),
                        HorizontalAlignment = HAlignment.Center,
                        VerticalAlignment = VAlignment.Center,
                    }
                }
            };

            var openButton = new Button()
            {
                Text = Loc.GetString("gateway-window-open-portal"),
                Pressed = Pressable(),
                ToggleMode = true,
                Disabled = now < _nextReady || Pressable(),
                HorizontalAlignment = HAlignment.Right,
                Margin = new Thickness(10f, 0f, 0f, 0f),
                Visible = !locked,
                SetHeight = 32f,
            };

            openButton.OnPressed += args =>
            {
                OpenPortal?.Invoke(ent);
            };

            if (Pressable())
            {
                openButton.AddStyleClass(StyleBase.ButtonDanger);
            }

            var buttonContainer = new BoxContainer()
            {
                Children =
                {
                    buttonStripe,
                    openButton,
                },
                SetSize = new Vector2(128f, 40f),
            };

            box.AddChild(buttonContainer);

            Container.AddChild(new PanelContainer()
            {
                PanelOverride = new StyleBoxFlat(new Color(30, 30, 34)),
                Margin = new Thickness(10f, 5f),
                Children =
                {
                    box
                }
            });
        }

        _lastState = state;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        var now = _timing.CurTime;
        var dirtyState = false;

        // if its not going to close then show it as empty
        if (_nextUnlock == TimeSpan.Zero)
        {
            NextUnlockBar.Value = 1f;
            NextUnlockText.Text = "00:00";
        }
        else
        {
            var remaining = _nextUnlock - now;
            if (remaining < TimeSpan.Zero)
            {
                if (_isUnlockPending)
                {
                    dirtyState = true;
                    _isUnlockPending = false;
                }

                NextUnlockBar.Value = 1f;
                NextUnlockText.Text = "00:00";
            }
            else
            {
                NextUnlockBar.Value = 1f - (float) (remaining.TotalSeconds / _unlockTime.TotalSeconds);
                NextUnlockText.Text = $"{remaining.Minutes:00}:{remaining.Seconds:00}";
            }
        }

        // if its not going to close then show it as empty
        if (_current == null || _cooldown == TimeSpan.Zero)
        {
            NextReadyBar.Value = 1f;
            NextCloseText.Text = "00:00";
        }
        else
        {
            var remaining = _nextReady - now;
            if (remaining < TimeSpan.Zero)
            {
                if (_isCooldownPending)
                {
                    dirtyState = true;
                    _isCooldownPending = false;
                }

                NextReadyBar.Value = 1f;
                NextCloseText.Text = "00:00";
            }
            else
            {
                NextReadyBar.Value = 1f - (float) (remaining.TotalSeconds / _cooldown.TotalSeconds);
                NextCloseText.Text = $"{remaining.Minutes:00}:{remaining.Seconds:00}";
            }
        }

        if (dirtyState && _lastState != null)
        {
            // Refresh UI buttons.
            UpdateState(_lastState);
        }
    }
}
