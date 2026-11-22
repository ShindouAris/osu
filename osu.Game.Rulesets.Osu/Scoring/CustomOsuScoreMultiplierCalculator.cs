// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    /// <summary>
    /// Fork-specific score multipliers, layered on top of upstream's <see cref="OsuScoreMultiplierCalculatorV2"/>.
    /// Keeping all deviations from upstream in this one file avoids merge conflicts when syncing.
    /// </summary>
    public class CustomOsuScoreMultiplierCalculator : OsuScoreMultiplierCalculatorV2
    {
        public CustomOsuScoreMultiplierCalculator(ScoreMultiplierContext context)
            : base(context)
        {
            Single<OsuModRelax>(hasMultiplier: 1);
            Single<OsuModAutopilot>(hasMultiplier: 1);
            Single<OsuModTraceable>(hasMultiplier: 1.1);
            Single<OsuModLowAr>(hasMultiplier: 1.2);
        }
    }
}
