using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Verbs
{
    /// <summary>
    ///     Contains combined name and icon information for a verb category.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class VerbCategory
    {
        public readonly string Text;

        public readonly SpriteSpecifier? Icon;

        /// <summary>
        ///     Columns for the grid layout that shows the verbs in this category. If <see cref="IconsOnly"/> is false,
        ///     this should very likely be set to 1.
        /// </summary>
        public int Columns = 1;

        /// <summary>
        ///     If true, the members of this verb category will be shown in the context menu as a row of icons without
        ///     any text.
        /// </summary>
        /// <remarks>
        ///     For example, the 'Rotate' category simply shows two icons for rotating left and right.
        /// </remarks>
        public readonly bool IconsOnly;

        public VerbCategory(string text, bool iconsOnly = false)
        {
            Text = Loc.GetString(text);
            IconsOnly = iconsOnly;
        }

        public VerbCategory(string text, string icon, bool iconsOnly = false)
        {
            Text = Loc.GetString(text);
            Icon = new SpriteSpecifier.Texture(new ResPath(icon));
            IconsOnly = iconsOnly;
        }

        public VerbCategory(string text, SpriteSpecifier? sprite, bool iconsOnly = false)
        {
            Text = Loc.GetString(text);
            Icon = sprite;
            IconsOnly = iconsOnly;
        }

        public static readonly VerbCategory Admin =
            new("verb-categories-admin", "/Textures/Interface/character.svg.192dpi.png");

        public static readonly VerbCategory Antag =
            new("verb-categories-antag", "/Textures/Interface/VerbIcons/antag-e_sword-temp.192dpi.png", iconsOnly: true)
                { Columns = 5 };

        public static readonly VerbCategory Examine =
            new("verb-categories-examine", "/Textures/Interface/VerbIcons/examine.svg.192dpi.png");

        public static readonly VerbCategory Debug =
            new("verb-categories-debug", "/Textures/Interface/VerbIcons/debug.svg.192dpi.png");

        public static readonly VerbCategory Eject =
            new("verb-categories-eject", "/Textures/Interface/VerbIcons/eject.svg.192dpi.png");

        public static readonly VerbCategory Insert =
            new("verb-categories-insert", "/Textures/Interface/VerbIcons/insert.svg.192dpi.png");

        public static readonly VerbCategory Buckle =
            new("verb-categories-buckle", "/Textures/Interface/VerbIcons/buckle.svg.192dpi.png");

        public static readonly VerbCategory Unbuckle =
            new("verb-categories-unbuckle", "/Textures/Interface/VerbIcons/unbuckle.svg.192dpi.png");

        public static readonly VerbCategory Rotate =
            new("verb-categories-rotate", "/Textures/Interface/VerbIcons/refresh.svg.192dpi.png", iconsOnly: true)
                { Columns = 5 };

        public static readonly VerbCategory Smite =
            new("verb-categories-smite", "/Textures/Interface/VerbIcons/smite.svg.192dpi.png", iconsOnly: true)
                { Columns = 6 };

        public static readonly VerbCategory Tricks =
            new("verb-categories-tricks", "/Textures/Interface/AdminActions/tricks.png", iconsOnly: true)
                { Columns = 5 };

        public static readonly VerbCategory SetTransferAmount =
            new("verb-categories-transfer", "/Textures/Interface/VerbIcons/spill.svg.192dpi.png");

        public static readonly VerbCategory Split = new("verb-categories-split");

        public static readonly VerbCategory InstrumentStyle = new("verb-categories-instrument-style");

        public static readonly VerbCategory ChannelSelect = new("verb-categories-channel-select");

        public static readonly VerbCategory SetSensor = new("verb-categories-set-sensor");

        public static readonly VerbCategory Lever = new("verb-categories-lever");

        public static readonly VerbCategory SelectType = new("verb-categories-select-type");

        public static readonly VerbCategory SelectFaction = new("verb-categories-select-faction");

        public static readonly VerbCategory Rename = new("verb-categories-rename");

        public static readonly VerbCategory PowerLevel = new("verb-categories-power-level");

        public static readonly VerbCategory Interaction = new("verb-categories-interaction");

        public static readonly VerbCategory BloodSpells = new("verb-categories-blood-cult",
            new SpriteSpecifier.Rsi(new ResPath("/Textures/_White/BloodCult/actions.rsi"), "blood_spells"));
    }
}
