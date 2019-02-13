using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using _SyrupFramework;

namespace Perennials
{
    public class CropSprawler : SoilCrop
    {

        public CropSprawler() { }
        public bool spreading;
        public int spreadProgress;
        public int currentFruitSprite;

        public static Dictionary<int, int> drawGuide;
        public static Dictionary<string, Texture2D> sprawlerSprites;
        private Texture2D sprawlerSpriteSheet;

        public List<Sprawl> sprawlTiles;

        private bool harvestFinished = false;

        public CropSprawler(string cropName, int heightOffset = 0) : base(cropName, heightOffset)
        {
            Logger.Log("CropSprawler was created!");
            shakable = false;
            spreading = false;
            spreadProgress = 0;
            sprawlTiles = new List<Sprawl>();
            sprawlerSpriteSheet = sprawlerSprites[cropName];
            currentFruitSprite = -1;
        }

        private Dictionary<Vector2,float> getWeightedGrowth(Vector2 tileLocation, GameLocation location = null)
        {
            Dictionary<Vector2, float> weights = new Dictionary<Vector2, float>();
            if (location is null)
                location = Game1.currentLocation;
            Vector2 index = new Vector2(tileLocation.X - 1, tileLocation.Y);
            weights[index] = getWeightedGrowthForTile(index, location);
            index.X += 2;
            weights[index] = getWeightedGrowthForTile(index, location);
            index.X--;
            index.Y--;
            weights[index] = getWeightedGrowthForTile(index, location);
            index.Y += 2;
            weights[index] = getWeightedGrowthForTile(index, location);
            return weights;
        }

        private float getWeightedGrowthForTile(Vector2 tileLocation, GameLocation location = null, bool ignoreSelf = false)
        {
            if (location is null)
                location = Game1.currentLocation;
            //This is where the factors are weighed on how eager the vine is to grow here.
            if (!location.isTileOccupiedForPlacement(tileLocation) || (ignoreSelf && location.objects.ContainsKey(tileLocation) && location.objects[tileLocation] is Sprawl))
            {
                float weightValue = 1f;
                if(location.terrainFeatures.ContainsKey(tileLocation))
                {
                    if (location.terrainFeatures[tileLocation] is CropSoil)
                    {
                        //The tile is tilled soil.
                        CropSoil soil = location.terrainFeatures[tileLocation] as CropSoil;
                        //Sprawl will not expand onto soil with a crop in it.
                        if (soil.crop != null && !soil.crop.dead)
                        {
                            //Sprawl is ok with taking a tile with a dead (NOT dormant) crop.
                            if (soil.crop.dead)
                            {
                                soil.crop = null;
                            }
                            else
                                return 0f;
                        }
                        //Sprawl sees raised and lowered terrain as less favorable.
                        if (soil.height == CropSoil.Raised)
                            weightValue *= 0.8f;
                        else if (soil.height == CropSoil.Lowered)
                        {
                            //Sprawl will not grow on flooded land.
                            if (soil.flooded)
                                return 0f;
                            weightValue *= 0.8f;
                        }
                        //Sprawl avoids weeds.
                        if (soil.weeds)
                            weightValue *= 0.6f;
                    }
                    else if (location.terrainFeatures[tileLocation] is Flooring)
                    {
                        weightValue *= 0.4f;
                    }
                }
                else
                {
                    if (location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") != null)
                        weightValue = 0.6f;
                    else
                        weightValue = 0.3f;
                }
                weightValue *= 1 - ((float)Game1.random.NextDouble() * 0.8f);
                //Sprawl can grow over the top of paths
                return weightValue;
            }
            return 0f;
        }

        private void growSprawl(Vector2 tilLocation, GameLocation location)
        {
            Dictionary<Vector2, float> weights = getWeightedGrowth(tilLocation, location);
            Logger.Log("Weighing from sprawl tiles. Evaluating " + sprawlTiles.Count + " sprawl tiles...");
            foreach (Sprawl sprawl in sprawlTiles)
            {
                Dictionary<Vector2, float> sprawlWeights = getWeightedGrowth(sprawl.TileLocation, location);
                foreach(Vector2 tile in sprawlWeights.Keys)
                {
                    if (weights.ContainsKey(tile))
                    {
                        Logger.Log("Already weighted (" + tile.X + ", " + tile.Y + ") as " + weights[tile]*100 + "%");
                        weights[tile] = Math.Max(weights[tile], sprawlWeights[tile]);
                    }
                    else
                        weights[tile] = sprawlWeights[tile];
                    Logger.Log("(" + tile.X + ", " + tile.Y + ") weighted at " + weights[tile]*100 + "%");
                }
            }
            Logger.Log("Done evaluating weights.  Selecting highest weight...");
            float highest = 0f;
            Vector2 highestTile = Vector2.Zero;
            foreach(Vector2 tile in weights.Keys)
            {
                if(weights[tile] > highest)
                {
                    highest = weights[tile];
                    highestTile = tile;
                }
            }
            if(highestTile == Vector2.Zero)
            {
                Logger.Log("No suitable tile exists to expand to.");
                return;
            }

            Sprawl newSprawl = new Sprawl(highestTile, this);
            sprawlTiles.Add(newSprawl);
            syncSprawlTiles(location);
        }

        private void syncSprawlTiles(GameLocation location)
        {
            foreach(Sprawl sprawl in sprawlTiles)
            {
                location.objects[sprawl.TileLocation] = sprawl;
            }
        }

        public override void growFruit(string season)
        {
            string fruitReport = "Fruit report for sprawler " + crop + ": ";
            if (hasFruit)
            {
                Logger.Log(fruitReport + "already selected fruit tiles.  Aborting...");
                return;
            }
            //Ranks tie a sprawl to a value key, to make sorting them easier.
            Dictionary<float, Sprawl> ranks = new Dictionary<float, Sprawl>();
            foreach(Sprawl sprawl in sprawlTiles)
            {
                float weight = getWeightedGrowthForTile(sprawl.tileLocation, Game1.currentLocation, true);
                ranks[weight] = sprawl;
                fruitReport += "\nSprawler @ (" + sprawl.tileLocation.X + ", " + sprawl.tileLocation.Y + ") is valued at " + weight + ".";
            }
            fruitReport += "\nThere are " + ranks.Count + " ranked sprawl tiles, ";
            List<float> orderedRanks = ranks.Keys.ToList<float>();
            orderedRanks.Sort();
            orderedRanks.Reverse();
            fruitReport += (orderedRanks.Count == ranks.Count ? "identical " : "DIFFERENT ") + "to the initial ranks.";
            for(int i = 0; i < Math.Min(5, orderedRanks.Count); i++)
            {
                Sprawl tile = ranks[orderedRanks[i]];
                fruitReport += "\nAdding fruit to #" + (i + 1) + " ranked tile, (" + tile.tileLocation.X + ", " + tile.tileLocation.Y + ")";
                tile.hasFruit = true;
                tile.setInteractive(true);
            }
            fruitReport += "\nFinished fruit processing.";
            hasFruit = true;
            Logger.Log(fruitReport);
        }

        public override void beforeDestroy()
        {
            destroySprawl(sprawlTiles);
        }

        public bool harvestFruit(Sprawl sprawl)
        {
            if (!mature)
                return false;
            StardewValley.Farmer who = Game1.player;
            int qualityLevel = calculateQuality(who);
            StardewValley.Object fruitItem = new Fruit(crop, 1, qualityLevel);

            if (!who.addItemToInventoryBool(fruitItem))
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                return false;
            }
            sprawl.hasFruit = false;
            Game1.playSound("harvest");
            Game1.player.animateOnce(279 + Game1.player.facingDirection);
            Game1.player.canMove = false;
            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(17, new Vector2(sprawl.tileLocation.X * (float)Game1.tileSize, sprawl.tileLocation.Y * (float)Game1.tileSize), Color.White, 7, Game1.random.NextDouble() < 0.5, 125f, 0, -1, -1f, -1, 0));
            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(14, new Vector2(sprawl.tileLocation.X * (float)Game1.tileSize, sprawl.tileLocation.Y * (float)Game1.tileSize), Color.White, 7, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, -1, 0));
            float experience = (float)(16.0 * Math.Log(0.018 * fruitItem.price + 1.0, Math.E));
            who.gainExperience(0, (int)Math.Round((double)0.5));

            //Check if there's any fruit left on the vine

            harvestFinished = true;
            foreach(Sprawl sprawlTile in sprawlTiles)
            {
                if (sprawlTile.hasFruit)
                    harvestFinished = false;
            }

            if (harvestFinished)
                destroySprawl(sprawlTiles);
            return true;
        }

        public override bool harvest(Vector2 tileLocation)
        {
            return harvestFinished;
        }

        public void destroySprawl(Sprawl sprawlToDestroy)
        {
            if (sprawlTiles.Contains(sprawlToDestroy))
                sprawlTiles.Remove(sprawlToDestroy);
            if (Game1.currentLocation.objects.ContainsKey(sprawlToDestroy.tileLocation))
                Game1.currentLocation.objects.Remove(sprawlToDestroy.tileLocation);
        }

        private void destroySprawl(List<Sprawl> sprawlToDestroy)
        {
            //The killed off sprawl need to be put in a new list, in case this is fed the list of sprawl tiles itself.
            List<Sprawl> sprawlToRemove = new List<Sprawl>();
            foreach(Sprawl sprawl in sprawlToDestroy)
            {
                if (sprawlTiles.Contains(sprawl))
                    sprawlToRemove.Add(sprawl);
                if (Game1.currentLocation.objects.ContainsKey(sprawl.tileLocation))
                    Game1.currentLocation.objects.Remove(sprawl.tileLocation);
            }
            foreach(Sprawl sprawl in sprawlToRemove)
            {
                sprawlTiles.Remove(sprawl);
            }
        }

        public override bool grow(bool hydrated, bool flooded, int xTile, int yTile, GameLocation environment, string spoofSeason = null)
        {
            bool grew = false;
            string report = "Sprawler " + crop + "'s growth report: ";
            if (flooded)
            {
                report += "flooded, and now dead.";
                dead = true;
            }
            if (isGrowingSeason(spoofSeason))
            {
                if (!mature && !spreading)
                {
                    //Crop is still growing the parent sprout.
                    report += "Was not mature, and was not spreading, ";
                    //Reset regrowMaturity, just in case.
                    regrowMaturity = 0;
                    //Progress the seed maturity by one, flagging this crop as mature if it's done.
                    seedMaturity++;
                    report += "is " + seedMaturity + " days into seed growth out of " + seedDaysToMature + " needed to mature";
                    if (seedMaturity >= seedDaysToMature)
                    {
                        report += ". It will now begin spreading. ";
                        spreading = true;
                    }
                    else
                    {
                        report += ". ";
                    }
                }
                if (spreading)
                {
                    //During the spreading phase, the crop ticks up two counters
                    //spreadProgress tracks the days until the next growth
                    //regrowMaturity tracks the days until maturity
                    mature = false;
                    report += "Is spreading, ";
                    spreadProgress++;
                    regrowMaturity++;
                    report += "is " + spreadProgress + " days into sprawl growth out of " + regrowthStages[0] + " needed to spread";
                    if(spreadProgress >= regrowthStages[0])
                    {
                        report += ", causing it to expand by one tile";
                        //Here's where the growing method would be called
                        growSprawl(new Vector2(xTile, yTile), environment);
                        spreadProgress = 0;
                    }
                    report += ". It is now " + regrowMaturity + " days into its second growth phase, out of " + regrowDaysToMature + " needed to mature";
                    if(regrowMaturity >= regrowthStages[2] + regrowthStages[1] + regrowthStages[0])
                    {
                        growFruit(spoofSeason);
                    }
                    if(regrowMaturity >= regrowDaysToMature)
                    {
                        report += ", making it mature";
                        mature = true;
                        spreading = false;
                    }
                    report += ". ";
                }
                if (mature)
                {
                    //Will make the fruit harvestable.
                    //growFruit(spoofSeason);
                }
                grew = true;
            }
            else
            {
                if (isSeed())
                {
                    report += "Currently a seed outside of its growing season, lying in wait.";
                    //Do nothing, the seed is dormant.
                }
                else if (age > 0 && perennial)
                {
                    report += "A perennial that is out of season, which will revert back to a seed.";
                    dormant = true;
                    mature = false;
                    regrowMaturity = 0;
                    seedMaturity = 0;
                    years++;
                    age = 0;
                    hydration = 0f;
                    weedless = 1f;
                    report += " It is now " + years + " years old.";
                }
            }
            Logger.Log(report);
            return grew;
            //return base.grow(hydrated, flooded, xTile, yTile, environment, spoofSeason);
        }

        public static void populateDrawGuide()
        {
            drawGuide = new Dictionary<int, int>();
            drawGuide.Add(0, 0);
            drawGuide.Add(10, 15);
            drawGuide.Add(100, 13);
            drawGuide.Add(1000, 12);
            drawGuide.Add(500, 4);
            drawGuide.Add(1010, 11);
            drawGuide.Add(1100, 9);
            drawGuide.Add(1500, 8);
            drawGuide.Add(600, 1);
            drawGuide.Add(510, 3);
            drawGuide.Add(110, 14);
            drawGuide.Add(1600, 5);
            drawGuide.Add(1610, 6);
            drawGuide.Add(1510, 7);
            drawGuide.Add(1110, 10);
            drawGuide.Add(610, 2);
        }

        private int calculateAdjacency(Vector2 sprawlTile, Vector2 tileLocation, GameLocation location)
        {
            int adjacency = 0;
            Vector2 key = sprawlTile;
            ++key.X;
            if (key == tileLocation)
                adjacency += 100;
            else
            {
                foreach (Sprawl sprawl in sprawlTiles)
                {
                    if (sprawl.tileLocation == key)
                        adjacency += 100;
                }
            }
            key.X -= 2f;
            if (key == tileLocation)
                adjacency += 10;
            else
            {
                foreach (Sprawl sprawl in sprawlTiles)
                {
                    if (sprawl.tileLocation == key)
                        adjacency += 10;
                }
            }
            ++key.X;
            ++key.Y;
            if (key == tileLocation)
                adjacency += 500;
            else
            {
                foreach (Sprawl sprawl in sprawlTiles)
                {
                    if (sprawl.tileLocation == key)
                        adjacency += 500;
                }
            }
            key.Y -= 2f;
            if (key == tileLocation)
                adjacency += 1000;
            else
            {
                foreach (Sprawl sprawl in sprawlTiles)
                {
                    if (sprawl.tileLocation == key)
                        adjacency += 1000;
                }
            }
            return adjacency;
        }

        public override void updateSpriteIndex(string spoofSeason = null)
        {
            currentSprite = getCurrentPhase();
            currentFruitSprite = getCurrentFruit();
        }

        public int getCurrentFruit()
        {
            int phase = -2;
            int growthSum = 0;
            foreach (int growthStage in regrowthStages)
            {
                growthSum += growthStage;
                if (regrowMaturity >= growthSum)
                    phase++;
                else
                    break;
            }
            if (mature)
                return regrowthStages.Count - 2;
            return phase;
        }

        public override Rectangle getSprite(int number = 0)
        {
            if (isSeed())
            {
                //Returns either the top or bottom seed sprite.
                return new Rectangle(0, (number % 2) * 16 + 32, 16, 16);
            }
            //Returns the current sprite.
            return new Rectangle((currentSprite - 2) * 16, 0, 16, 32);
        }

        private Rectangle getFruitSprite()
        {
            //Returns the current sprite.
            return new Rectangle((currentFruitSprite + 1) * 16, 0, 16, 32);
        }

        private Rectangle getLeavesSprite(int num)
        {
            return new Rectangle(0, 64 + (num % 2) * 16, 16, 16);
        }

        private Rectangle getSprawlSprite(int adjacency, bool flip = false)
        {
            //true if the tile is flipped, and it is either left-connected or right-connected, but not both.
            if (flip && (adjacency % 500 >= 100 ^ adjacency % 100 == 10))
            {
                adjacency = drawGuide[adjacency % 100 == 10 ? adjacency + 90 : adjacency - 90];
            }
            else
                adjacency = drawGuide[adjacency];
            return new Rectangle(adjacency % 4 * 16 + 16, adjacency / 4 * 16 + 32, 16, 16);
        }

        private bool vineMirror(int adjacency, int num)
        {
            bool right = adjacency % 500 >= 100;
            bool left = adjacency % 100 == 10;
            return ((left && right) || (!left && !right)) ? num % 2 == 0 : false;
        }

        private void drawLeaves(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation, bool flip, int adjacency)
        {
            //I don't like the way the leaves are rendered, and I'm going to change it entirely.
            return;
            //Draw leaves
            b.Draw(
                sprawlerSpriteSheet,
                Game1.GlobalToLocal(
                    Game1.viewport,
                    new Vector2((float)((double)tileLocation.X * (double)Game1.tileSize) + (float)(Game1.tileSize / 2),
                    (float)((double)tileLocation.Y * (double)Game1.tileSize) + (Game1.tileSize / 2) - (Game1.tileSize * heightOffset / 4))
                ),
                getLeavesSprite((int)tileLocation.X * 7 + (int)tileLocation.Y * 11),
                toTint,
                rotation,
                new Vector2(8f, 8f),
                (float)Game1.pixelZoom,
                flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                (float)((tileLocation.Y * 64 + 32) + (tileLocation.Y * 11.0 + tileLocation.X * 7.0) % 10.0 - 5.0) / 10000f
            //(float)((double)tileLocation.Y * (double)Game1.tileSize + (double)(Game1.tileSize / 2))
            );
        }

        public override void draw(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
        {
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((double)tileLocation.X * (double)Game1.tileSize) + (float)(Game1.tileSize / 2), (float)((double)tileLocation.Y * (double)Game1.tileSize) + (Game1.tileSize / 2) - (Game1.tileSize * heightOffset / 4)));
            float depth = (float)((tileLocation.Y * 64 + 4) + (tileLocation.Y * 11.0 + tileLocation.X * 7.0) % 10.0 - 5.0) / 10000f;
            if (!spreading && !mature)
            {
                b.Draw(sprawlerSpriteSheet,
                local,
                getSprite((int)tileLocation.X * 7 + (int)tileLocation.Y * 11),
                toTint,
                rotation,
                new Vector2(8f, isSeed() ? 8f : 24f),
                (float)Game1.pixelZoom,
                this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                depth
                //(float)((double)tileLocation.Y * (double)Game1.tileSize + (double)(Game1.tileSize / 2))
                );
            }
            else
            {
                bool cropFlip = (((int)tileLocation.X * 7 + (int)tileLocation.Y * 11) % 2) == 0;
                int cropAdj = calculateAdjacency(tileLocation, tileLocation, Game1.currentLocation);
                b.Draw(sprawlerSpriteSheet,
                local,
                getSprawlSprite(cropAdj, cropFlip),
                toTint,
                0f,
                new Vector2(8f, 8f),
                (float)Game1.pixelZoom,
                cropFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                depth
                //(float)((double)tileLocation.Y * (double)Game1.tileSize + (double)(Game1.tileSize / 2))
                );
                drawLeaves(b, tileLocation, toTint, rotation, flip, 0);
                foreach (Sprawl sprawl in sprawlTiles)
                {
                    int sprawlAdj = calculateAdjacency(sprawl.tileLocation, tileLocation, Game1.currentLocation);
                    bool sprawlFlip = ((((int)sprawl.tileLocation.X + drawGuide[sprawlAdj] % 7) * 7 + ((int)sprawl.tileLocation.Y + drawGuide[sprawlAdj] % 5) * 11) % 2) == 0;
                    b.Draw(sprawlerSpriteSheet,
                    Game1.GlobalToLocal(
                        Game1.viewport,
                        new Vector2((float)((double)sprawl.tileLocation.X * (double)Game1.tileSize) + (float)(Game1.tileSize / 2),
                        (float)((double)sprawl.tileLocation.Y * (double)Game1.tileSize) + (Game1.tileSize / 2) - (Game1.tileSize * heightOffset / 4))
                    ),
                    getSprawlSprite(sprawlAdj, sprawlFlip),
                    toTint,
                    0f,
                    new Vector2(8f, 8),
                    (float)Game1.pixelZoom,
                    sprawlFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    (float)((sprawl.tileLocation.Y * 64 + 4) + (sprawl.tileLocation.Y * 11.0 + sprawl.tileLocation.X * 7.0) % 10.0 - 5.0) / 10000f
                    //(float)((double)tileLocation.Y * (double)Game1.tileSize + (double)(Game1.tileSize / 2))
                    );
                    drawLeaves(b, sprawl.tileLocation, toTint, rotation, sprawlFlip, 0);
                    if (sprawl.hasFruit)
                    {
                        //Draw fruit.
                        //This will be a flower, small fruit, or fruit depending on the age of the crop.  The fruit flag is repurposed here from its original meaning.
                        b.Draw(sprawlerSpriteSheet,
                        Game1.GlobalToLocal(
                            Game1.viewport,
                            new Vector2((float)((double)sprawl.tileLocation.X * (double)Game1.tileSize) + (float)(Game1.tileSize / 2),
                            (float)((double)sprawl.tileLocation.Y * (double)Game1.tileSize) + (Game1.tileSize / 2) - (Game1.tileSize * heightOffset / 4))
                        ),
                        getFruitSprite(),
                        toTint,
                        0f,
                        new Vector2(8f, 24f),
                        (float)Game1.pixelZoom,
                        sprawlFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                        (float)((sprawl.tileLocation.Y * 64 + 24) + (sprawl.tileLocation.Y * 11.0 + sprawl.tileLocation.X * 7.0) % 10.0 - 5.0) / 10000f
                        //(float)((double)tileLocation.Y * (double)Game1.tileSize + (double)(Game1.tileSize / 2))
                        );
                    }
                }
            }

        }
    }
}
