﻿using Robust.Shared.Audio;

namespace Content.Server.WhiteDream.BloodCult.Runes.Summon;

[RegisterComponent]
public sealed partial class CultRuneSummonComponent : Component
{
    [DataField]
    public SoundPathSpecifier TeleportSound = new("/Audio/_White/BloodCult/veilin.ogg");
}
