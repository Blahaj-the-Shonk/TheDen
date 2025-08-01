// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.UserInterface.Controls;
using Robust.Shared.IoC;
using Robust.Shared.Localization;

namespace Content.Client.Voting.UI
{
    /// <summary>
    ///     LITERALLY just a button that opens the vote call menu.
    ///     Automatically disables itself if the client cannot call votes.
    /// </summary>
    public sealed class VoteCallMenuButton : Button
    {
        [Dependency] private readonly IVoteManager _voteManager = default!;

        public VoteCallMenuButton()
        {
            IoCManager.InjectDependencies(this);

            Text = Loc.GetString("ui-vote-menu-button");
            OnPressed += OnOnPressed;
        }

        private void OnOnPressed(ButtonEventArgs obj)
        {
            var menu = new VoteCallMenu();
            menu.OpenCentered();
        }

        protected override void EnteredTree()
        {
            base.EnteredTree();

            UpdateCanCall(_voteManager.CanCallVote);
            _voteManager.CanCallVoteChanged += UpdateCanCall;
        }

        protected override void ExitedTree()
        {
            base.ExitedTree();

            _voteManager.CanCallVoteChanged += UpdateCanCall;
        }

        private void UpdateCanCall(bool canCall)
        {
            Disabled = !canCall;
        }
    }
}
