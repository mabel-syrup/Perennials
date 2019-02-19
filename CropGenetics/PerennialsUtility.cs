using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using Microsoft.Xna.Framework;

namespace Perennials
{
    public static class PerennialsUtility
    {
        //Almost purely copied code from StadrewValley's Game1.cs
        public static bool pressActionButton()
        {
            if (!Game1.player.UsingTool && !Game1.pickingTool && !Game1.menuUp && ((!Game1.eventUp || Game1.currentLocation.currentEvent.playerControlSequence) && (!Game1.nameSelectUp && Game1.numberOfSelectedItems == -1)) && !Game1.fadeToBlack)
            {
                Vector2 vector2 = new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y)) / 64f;
                if ((double)Game1.mouseCursorTransparency == 0.0 || !Game1.wasMouseVisibleThisFrame || !Game1.lastCursorMotionWasMouse && (Game1.player.ActiveObject == null || !Game1.player.ActiveObject.isPlaceable() && Game1.player.ActiveObject.Category != -74))
                {
                    vector2 = Game1.player.GetGrabTile();
                    if (vector2.Equals(Game1.player.getTileLocation()))
                        vector2 = Utility.getTranslatedVector2(vector2, Game1.player.FacingDirection, 1f);
                }
                if (!Utility.tileWithinRadiusOfPlayer((int)vector2.X, (int)vector2.Y, 1, Game1.player))
                {
                    vector2 = Game1.player.GetGrabTile();
                    if (vector2.Equals(Game1.player.getTileLocation()) && Game1.isAnyGamePadButtonBeingPressed())
                        vector2 = Utility.getTranslatedVector2(vector2, Game1.player.FacingDirection, 1f);
                }
                if (!Game1.eventUp || Game1.isFestival())
                {
                    if (Game1.tryToCheckAt(vector2, Game1.player))
                        return false;
                    if (Game1.player.isRidingHorse())
                    {
                        Game1.player.mount.checkAction(Game1.player, Game1.player.currentLocation);
                        return false;
                    }
                    if (!Game1.player.canMove)
                        return false;
                    bool flag = false;
                    if (Game1.player.ActiveObject != null && !(Game1.player.ActiveObject is StardewValley.Objects.Furniture))
                    {
                        int stack = Game1.player.ActiveObject.Stack;
                        Utility.tryToPlaceItem(Game1.currentLocation, (Item)Game1.player.ActiveObject, (int)vector2.X * 64 + 32, (int)vector2.Y * 64 + 32);
                        if (Game1.player.ActiveObject == null || Game1.player.ActiveObject.Stack < stack || Game1.player.ActiveObject.isPlaceable())
                            flag = true;
                    }
                }
            }
            return false;
        }
    }
}
