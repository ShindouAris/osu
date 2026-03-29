// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModLowAr : Mod, IApplicableToDifficulty
    {
        public override string Name => "Low Approach Rate";
        public override string Acronym => "LR";
        public override IconUsage? Icon => OsuIcon.ModDifficultyAdjust;
        public override ModType Type => ModType.DifficultyIncrease;
        public override double ScoreMultiplier => 1.2;
        public override LocalisableString Description => "Reading circle now got a little bit harder...";
        public override Type[] IncompatibleMods => new[] { typeof(ModEasy), typeof(ModDifficultyAdjust), typeof(ModHardRock) };
        public override bool Ranked => true;
        public override bool ValidForFreestyleAsRequiredMod => true;

        [SettingSource("Approach Rate", "The approach rate to set when this mod is active.", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> ApproachRate { get; } = new BindableDouble(1.0)
        {
            MinValue = 0,
            MaxValue = 3,
            Precision = 0.1,
        };

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.ApproachRate = (float)ApproachRate.Value;
        }
    }
}
