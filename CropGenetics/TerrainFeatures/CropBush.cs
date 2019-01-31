using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using _SyrupFramework;
using StardewValley;

namespace Perennials
{
    public class CropBush : SoilCrop, IModdedItem
    {

        public static Dictionary<string, Texture2D> bushSprites;

        private int seasonalSpriteRow;

        private Texture2D bushSpriteSheet;

        public CropBush() { }

        public CropBush(string cropName, int heightOffset = 0) : base(cropName, heightOffset)
        {
            Logger.Log("CropBush was created!");
            impassable = true;
            shakable = true;
            bushSpriteSheet = bushSprites[cropName];
        }

        public override bool grow(bool hydrated, bool flooded, int xTile, int yTile, GameLocation environment, string spoofSeason = null)
        {
            string season;
            if (spoofSeason is null)
                season = Game1.currentSeason;
            else
                season = spoofSeason;

            bool grew = false;
            bool growingSeason = isGrowingSeason(spoofSeason, environment);
            string report = crop + " growth report: ";

            if (flooded)
            {
                report += "flooded, so is now dead.";
                dead = true;
            }
            if (growingSeason)
            {
                report += season + " is within growing season, ";
                if (!mature)
                {
                    if (!dormant)
                    {
                        report += "was not mature, and was not dormant, ";
                        //Reset regrowMaturity, just in case.
                        regrowMaturity = 0;
                        //Progress the seed maturity by one.  When a bush reaches maturity, it immediately begins its regrowth cycle instead.
                        seedMaturity++;
                        report += "is " + seedMaturity + " days into seed growth out of " + seedDaysToMature + " needed to mature";
                        if (seedMaturity >= seedDaysToMature)
                        {
                            report += ", meaning it is now in its adult cycle. ";
                            mature = false;
                            dormant = true;
                            seedMaturity = 0;
                            regrowMaturity = 0;
                        }
                        else
                        {
                            report += ".";
                        }
                        grew = true;
                    }
                    //The bush is progressing towards fruiting.  The first phase of this growth cycle is its flowering phase.
                    //This uses the perennial "dormancy" flag, but is actually an adult yearly stage.
                    else if (dormant)
                    {
                        report += "was in its adult cycle, ";
                        regrowMaturity++;
                        report += "is " + regrowMaturity + " days into adult growth out of " + regrowDaysToMature + " needed to fruit";
                        if (regrowMaturity >= regrowDaysToMature)
                        {
                            report += ", making it ready to fruit. ";
                            mature = true;
                            dormant = false;
                        }
                        else
                        {
                            report += ".";
                        }
                        grew = true;
                    }
                }
                //New if statement, since it may have matured just now.
                if (mature)
                {
                    growFruit(season);
                    grew = true;
                }
            }
            else
            {
                report += season + " is out of its growing season, ";
                if (age != 0)
                {
                    report += "is partially grown, ";
                    //If it has fully grown, and was building towards fruiting, that is now reset.
                    if (dormant || mature)
                    {
                        report += "is in its adult growth, which is now reset. ";
                        age = 0;
                        regrowMaturity = 0;
                        dormant = true;
                        mature = false;
                        years++;
                        report += "It is now " + years + " years old.";
                    }
                    else if (!dormant && !mature)
                    {
                        report += "is still growing to maturity, and keeps its progress. ";
                    }
                }
            }
            Logger.Log(report);
            return grew;
        }

        public override void updateSpriteIndex(string spoofSeason = null)
        {
            if (!mature && !dormant)
            {
                //Still growing up, so we'll use the growing sprites.
                currentSprite = getCurrentPhase() - 1;
            }
            else
            {
                //If it's not still growing up, we are just going to use the adult sprite.
                currentSprite = 4;
            }
            string season;
            if (spoofSeason is null)
                season = Game1.currentSeason;
            else
                season = spoofSeason;
            switch (season)
            {
                case "spring":
                    seasonalSpriteRow = 0;
                    break;
                case "summer":
                    seasonalSpriteRow = 1;
                    break;
                case "fall":
                    seasonalSpriteRow = 2;
                    break;
                default:
                    seasonalSpriteRow = 3;
                    break;
            }
        }

        public override Rectangle getSprite(int number = 0)
        {
            if(isSeed() && !dormant)
            {
                //Returns either the top or bottom seed sprite.
                return new Rectangle(80, (number % 2) * 32, 16, 32);
            }
            //Returns the current sprite index, offset by season.
            return new Rectangle(currentSprite * 16, seasonalSpriteRow * 32, 16, 32);
        }

        public override void draw(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
        {
            b.Draw(bushSpriteSheet,
                Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((double)tileLocation.X * (double)Game1.tileSize) + (Game1.tileSize / 2), (float)((double)tileLocation.Y * (double)Game1.tileSize) + (Game1.tileSize / 2) - (Game1.tileSize * heightOffset / 4))),
                getSprite((int)tileLocation.X * 7 + (int)tileLocation.Y * 11),
                toTint,
                rotation,
                new Vector2(8f, 24f),
                (float)Game1.pixelZoom,
                this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                (float)(((double)tileLocation.Y * 64.0 + 32.0 + (((double)tileLocation.Y * 11.0 + (double)tileLocation.X * 7.0) % 10.0 - 5.0)) / 10000.0 / (!isSeed() ? 1.0 : 2.0))
            //(float)(((double)tileLocation.Y + 0.670000016689301) * 64.0 / 10000.0 + (double)tileLocation.X * 9.99999974737875E-06)
            //(float)(((double)tileLocation.Y * (double)Game1.tileSize + (double)(Game1.tileSize / 2) + (((double)tileLocation.Y * 11.0 + (double)tileLocation.X * 7.0) % 10.0 - 5.0)) / 10000.0 / (this.currentPhase != 0 || this.impassable || (specialType.Equals("Bush") && hasMatured) ? 1.0 : 2.0))
            //(float)((double)tileLocation.Y * (double)Game1.tileSize + (double)(Game1.tileSize / 2))
            );
            if (hasFruit)
            {
                b.Draw(bushSpriteSheet,
                    Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((double)tileLocation.X * (double)Game1.tileSize) + (Game1.tileSize / 2), (float)((double)tileLocation.Y * (double)Game1.tileSize) + (Game1.tileSize / 2) - (Game1.tileSize * heightOffset / 4))),
                    new Rectangle(80, 96, 16, 32),
                    toTint,
                    rotation,
                    new Vector2(8f, 24f),
                    (float)Game1.pixelZoom,
                    this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    (float)(((double)tileLocation.Y * 64.0 + 32.0 + (((double)tileLocation.Y * 11.0 + (double)tileLocation.X * 7.0) % 10.0 - 5.0)) / 10000.0 / 0.95)
                 );
            }
            else if (!mature && dormant && age > 0 && age < regrowthStages[0])
            {
                b.Draw(bushSpriteSheet,
                    Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((double)tileLocation.X * (double)Game1.tileSize) + (Game1.tileSize / 2), (float)((double)tileLocation.Y * (double)Game1.tileSize) + (Game1.tileSize / 2) - (Game1.tileSize * heightOffset / 4))),
                    new Rectangle(80, 64, 16, 32),
                    toTint,
                    rotation,
                    new Vector2(8f, 24f),
                    (float)Game1.pixelZoom,
                    this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    (float)(((double)tileLocation.Y * 64.0 + 32.0 + (((double)tileLocation.Y * 11.0 + (double)tileLocation.X * 7.0) % 10.0 - 5.0)) / 10000.0 / 0.95)
                 );
            }
        }

        public override void drawInMenu(SpriteBatch b, Vector2 screenPosition, Color toTint, float rotation, float scale, float layerDepth)
        {
            b.Draw(cropSpriteSheet, screenPosition, new Rectangle?(this.getSprite(0)), toTint, rotation, new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize + Game1.tileSize / 2)), scale, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }
    }
}
