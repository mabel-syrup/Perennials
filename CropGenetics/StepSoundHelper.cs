using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using Microsoft.Xna.Framework.Audio;

namespace Perennials
{
    public static class StepSoundHelper
    {
        public static Cue waterSound;
        public static Cue woodSound;

        public static void getSounds()
        {
            if(Game1.soundBank != null)
            {
                waterSound = Game1.soundBank.GetCue("waterSlosh");
                woodSound = Game1.soundBank.GetCue("woodyStep");
            }
        }
    }
}
