using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Netcode;
using _SyrupFramework;

namespace Perennials
{
    class CropSoil : TerrainFeature, IModdedItem
    {
        private Color c = Color.White;

        public int height;
        public const int Raised = 1;
        public const int Flat = 0;
        public const int Lowered = -1;

        public const int RaisedHeight = 16;
        public const int LoweredHeight = -16;

        private const int Up = 0;
        private const int Right = 1;
        private const int Down = 2;
        private const int Left = 3;

        public const double weedsChance = 0.05f;

        public bool hydrated;
        public bool flooded;
        public bool holdOver;
        public SoilCrop crop;
        public bool weeds;
        public int[] npk = new int[] { 0, 0, 0 };

        public static Texture2D lowTexture;
        public static Texture2D highTexture;
        public static Texture2D flatTexture;
        public static Dictionary<int, int> drawGuide;
        
        private bool shakeLeft;
        private float shakeRotation;
        private float maxShake;
        private float shakeRate;
        
        private string spoofedSeason;

        private int index1 = 0;
        private int index2 = 0;
        private int index3 = 0;


        public CropSoil() : base(true)
        {
            height = Flat;
            hydrated = false;
            flooded = false;
            holdOver = false;
        }

        public override Rectangle getBoundingBox(Vector2 tileLocation)
        {
            return new Rectangle((int)((double)tileLocation.X * (double)Game1.tileSize), (int)((double)tileLocation.Y * (double)Game1.tileSize), Game1.tileSize, Game1.tileSize);
        }

        private int distanceFromEdge(Point centerPoint, Vector2 tile, int edge)
        {
            int scale = Game1.tileSize;
            if(edge == Up)
            {
                return (int)(centerPoint.Y - (tile.Y * scale));
            }
            else if (edge == Right)
            {
                return (int)(((tile.X + 1) * scale) - centerPoint.X);
            }
            else if (edge == Down)
            {
                return (int)(((tile.Y + 1) * scale) - centerPoint.Y);
            }
            else if (edge == Left)
            {
                return (int)(centerPoint.X - (tile.X * scale));
            }
            return -1;
        }

        private bool withinEdge(Point centerPoint, Vector2 tile, int widthOfEdge, int edge = -1)
        {
            int scale = Game1.tileSize;
            Rectangle bounds;
            if(edge == Up)
            {
                bounds = new Rectangle((int)tile.X * scale, (int)tile.Y * scale, scale, widthOfEdge);
            }
            else if(edge == Left)
            {
                bounds = new Rectangle((int)tile.X * scale, (int)tile.Y * scale, widthOfEdge, scale);
            }
            else if (edge == Down)
            {
                bounds = new Rectangle((int)tile.X * scale, (int)(tile.Y + 1) * scale - widthOfEdge, scale, widthOfEdge);
            }
            else if (edge == Right)
            {
                bounds = new Rectangle((int)(tile.X + 1) * scale - widthOfEdge, (int)tile.Y * scale, widthOfEdge, scale);
            }
            else
            {
                bounds = new Rectangle((int)tile.X * scale + widthOfEdge, (int)tile.Y * scale + widthOfEdge, scale - (widthOfEdge * 2), scale - (widthOfEdge * 2));
            }

            if (edge != -1)
                return bounds.Contains(centerPoint);
            return !bounds.Contains(centerPoint) && new Rectangle((int)tile.X * scale, (int)tile.Y * scale, scale, scale).Contains(centerPoint);
        }

        public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who, GameLocation location)
        {
            //Logger.Log("COLLISION\nFARMER (" + positionOfCollider.X + ", " + positionOfCollider.Y + ", " + positionOfCollider.Width + ", " + positionOfCollider.Height + ")\n" +
            //    "HEADING " + (who.getDirection() == 0 ? "UP" : who.getDirection() == 1 ? "RIGHT" : who.getDirection() == 2 ? "DOWN" : "LEFT") + "\n" +
            //    "TILE (" + tileLocation.X * Game1.tileSize + ", " + tileLocation.Y * Game1.tileSize + ", " + (tileLocation.X + 1) * Game1.tileSize + ", " + (tileLocation.Y + 1) * Game1.tileSize + ")");
            Point footCenter = new Point(positionOfCollider.X + (positionOfCollider.Width / 2), positionOfCollider.Y + positionOfCollider.Height);
            if(who is Farmer)
                processHeightManipulation(who as Farmer, footCenter, tileLocation);
            if (this.crop != null && this.crop.getCurrentPhase() != 0 && (speedOfCollision > 0 && (double)this.maxShake == 0.0) && (positionOfCollider.Intersects(this.getBoundingBox(tileLocation)) && Utility.isOnScreen(Utility.Vector2ToPoint(tileLocation), Game1.tileSize, location)))
            {
                if (Game1.soundBank != null && (who == null || who.GetType() != typeof(FarmAnimal)) && !Grass.grassSound.IsPlaying)
                {
                    Grass.grassSound = Game1.soundBank.GetCue("grassyStep");
                    Grass.grassSound.Play();
                }
                this.shake((float)(0.392699092626572 / (double)((5 + Game1.player.addedSpeed) / speedOfCollision) - (speedOfCollision > 2 ? (double)this.crop.currentPhase * 3.14159274101257 / 64.0 : 0.0)), (float)Math.PI / 80f / (float)((5 + Game1.player.addedSpeed) / speedOfCollision), (double)positionOfCollider.Center.X > (double)tileLocation.X * (double)Game1.tileSize + (double)(Game1.tileSize / 2));
            }
            if (this.crop == null || this.crop.currentPhase == 0 || (!(who is StardewValley.Farmer) || !(who as StardewValley.Farmer).running))
                return;
            (who as StardewValley.Farmer).temporarySpeedBuff = -1f;
        }

        private void processHeightManipulation(Farmer who, Point footCenter, Vector2 tileLocation)
        {
            int currentHeight = PerennialsGlobal.getFarmerHeight(who);
            if (currentHeight < LoweredHeight)
                PerennialsGlobal.setFarmerHeight(who, currentHeight % LoweredHeight);
            if (currentHeight > RaisedHeight)
                PerennialsGlobal.setFarmerHeight(who, currentHeight % RaisedHeight);
            if (height == Lowered)
            {
                if (!flooded)
                    who.temporarySpeedBuff = -1f;
                else
                {
                    who.temporarySpeedBuff = -2f;
                }
                if (withinEdge(footCenter, tileLocation, 10))
                {
                    //Farmer is within 20 pixels of the tile border.
                    if (index1 < 1000 && withinEdge(footCenter, tileLocation, 10, Up))
                    {
                        //Top edge
                        if (who.FacingDirection == Up || who.FacingDirection == Down)
                        {
                            who.temporarySpeedBuff = 4f;
                            if (who.FacingDirection == Up)
                                PerennialsGlobal.setFarmerHeight(who, 0);
                            if (who.FacingDirection == Down)
                                PerennialsGlobal.setFarmerHeight(who, LoweredHeight);
                        }
                    }
                    if (index1 % 1000 < 500 && withinEdge(footCenter, tileLocation, 10, Down))
                    {
                        //Bottom Edge
                        if (who.FacingDirection == Up || who.FacingDirection == Down)
                        {
                            who.temporarySpeedBuff = -10f;
                            if (who.FacingDirection == Up)
                                PerennialsGlobal.setFarmerHeight(who, LoweredHeight);
                            if (who.FacingDirection == Down)
                                PerennialsGlobal.setFarmerHeight(who, 0);
                        }
                    }
                    int slopeHeight = PerennialsGlobal.getFarmerHeight(who);
                    if (index1 % 500 < 100 && withinEdge(footCenter, tileLocation, 10, Right))
                    {
                        //Right Edge
                        slopeHeight = (int)((distanceFromEdge(footCenter, tileLocation, Right) / 10f) * LoweredHeight);
                    }
                    if (index1 % 100 != 10 && withinEdge(footCenter, tileLocation, 10, Left))
                    {
                        //Left Edge
                        slopeHeight = (int)((distanceFromEdge(footCenter, tileLocation, Left) / 10f) * LoweredHeight);
                    }
                    PerennialsGlobal.raiseFarmerTo(who, slopeHeight);
                }
            }
            else if (height == Raised)
            {
                //(who as StardewValley.Farmer).temporarySpeedBuff = speedOfCollision < 5 ? -5f : -2f;
                if (withinEdge(footCenter, tileLocation, 10))
                {
                    //Farmer is within 20 pixels of the tile border.
                    if (index1 < 1000 && withinEdge(footCenter, tileLocation, 10, Up))
                    {
                        //Top Edge
                        if (who.FacingDirection == Up || who.FacingDirection == Down)
                        {
                            who.temporarySpeedBuff = -10f;
                            if (who.FacingDirection == Up)
                                PerennialsGlobal.setFarmerHeight(who, 0);
                            if (who.FacingDirection == Down)
                                PerennialsGlobal.setFarmerHeight(who, RaisedHeight);
                        }
                    }
                    if (index1 % 1000 < 500 && withinEdge(footCenter, tileLocation, 10, Down))
                    {
                        //Bottom Edge
                        if (who.FacingDirection == Up || who.FacingDirection == Down)
                        {
                            who.temporarySpeedBuff = 4f;
                            if (who.FacingDirection == Up)
                                PerennialsGlobal.setFarmerHeight(who, RaisedHeight);
                            if (who.FacingDirection == Down)
                                PerennialsGlobal.setFarmerHeight(who, 0);
                        }
                    }
                    int slopeHeight = PerennialsGlobal.getFarmerHeight(who);
                    if (index1 % 500 < 100 && withinEdge(footCenter, tileLocation, 10, Right))
                    {
                        //Right Edge
                        slopeHeight = (int)((distanceFromEdge(footCenter, tileLocation, Right) / 10f) * RaisedHeight);
                    }
                    if (index1 % 100 != 10 && withinEdge(footCenter, tileLocation, 10, Left))
                    {
                        //Left Edge
                        slopeHeight = (int)((distanceFromEdge(footCenter, tileLocation, Left) / 10f) * RaisedHeight);
                    }
                    PerennialsGlobal.raiseFarmerTo(who, slopeHeight);
                }
                //else if (new Rectangle((int)tileLocation.X * Game1.tileSize, (int)tileLocation.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize).Contains(footCenter))
                //{

                //}
            }
        }

        private void shake(float shake, float rate, bool left)
        {
            if (this.crop == null)
                return;
            this.maxShake = shake * (this.crop.impassable ? 0.6f : 1.5f) * (this.crop.shakable ? 1f : 0.2f);
            this.shakeRate = rate * 0.5f;
            this.shakeRotation = 0.0f;
            this.shakeLeft = left;
        }

        public static void populateDrawGuide()
        {
            CropSoil.drawGuide = new Dictionary<int, int>();
            CropSoil.drawGuide.Add(0, 0);
            CropSoil.drawGuide.Add(10, 15);
            CropSoil.drawGuide.Add(100, 13);
            CropSoil.drawGuide.Add(1000, 12);
            CropSoil.drawGuide.Add(500, 4);
            CropSoil.drawGuide.Add(1010, 11);
            CropSoil.drawGuide.Add(1100, 9);
            CropSoil.drawGuide.Add(1500, 8);
            CropSoil.drawGuide.Add(600, 1);
            CropSoil.drawGuide.Add(510, 3);
            CropSoil.drawGuide.Add(110, 14);
            CropSoil.drawGuide.Add(1600, 5);
            CropSoil.drawGuide.Add(1610, 6);
            CropSoil.drawGuide.Add(1510, 7);
            CropSoil.drawGuide.Add(1110, 10);
            CropSoil.drawGuide.Add(610, 2);
        }

        public override void loadSprite()
        {
            if (CropSoil.lowTexture == null)
            {
                try
                {
                    CropSoil.lowTexture = HoeDirt.lightTexture;
                }
                catch (Exception)
                {
                }
            }
            if (CropSoil.flatTexture == null)
            {
                try
                {
                    CropSoil.flatTexture = HoeDirt.darkTexture;
                }
                catch (Exception)
                {
                }
            }
            if (CropSoil.highTexture == null)
            {
                try
                {
                    CropSoil.highTexture = HoeDirt.snowTexture;
                }
                catch (Exception)
                {
                }
            }
        }

        public bool plant(string which, StardewValley.Farmer who)
        {
            if (crop != null)
                return false;
            if (!who.currentLocation.isFarm && !who.currentLocation.name.Equals("Greenhouse"))
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13919"));
                return false;
            }
            crop = getCrop(which);
            Game1.playSound("dirtyHit");
            ++Game1.stats.seedsSown;
            return true;
        }

        public override bool isPassable(Character c)
        {
            //if (flooded && !(c as Farmer).swimming)
            //{
            //    return false;
            //}
            //else if (flooded && (c as Farmer).swimming)
            //{
            //    return true;
            //}
            if (crop != null)
                return !crop.impassable;
            return true;
        }

        public void destroyCrop(Vector2 tileLocation, bool showAnimation = true)
        {
            if (crop != null & showAnimation)
            {
                if (crop.currentPhase < 1 && !crop.dead)
                {
                    Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(12, tileLocation * (float)Game1.tileSize, Color.White, 8, false, 100f, 0, -1, -1f, -1, 0));
                    Game1.playSound("dirtyHit");
                }
                else
                    Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(50, tileLocation * (float)Game1.tileSize, crop.dead ? new Color(207, 193, 43) : Color.ForestGreen, 8, false, 100f, 0, -1, -1f, -1, 0));
            }
            crop = null;
            npk = new int[] { 0, 0, 0 };
        }

        public override bool performUseAction(Vector2 tileLocation, GameLocation location)
        {
            //Global.Log("Touched a cropsoil!  Currently " + (height == Lowered ? "a ditch, " : (height == Flat ? "flat, " : "raised, ")) + " and " + (flooded ? "flooded." : (hydrated ? "hydrated." : "dry.")));
            //return true;
            if(crop != null)
            {
                if (crop.harvest(tileLocation))
                    destroyCrop(tileLocation, true);
            }
            return false;
        }

        public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location = null)
        {
            if (t != null)
            {
                if (t.GetType() == typeof(Shovel))
                {
                    if (location != null && (location is Farm || location.name.Equals("greenhouse")))
                    {
                        height = ((height + 2) % 3) - 1;
                        if (Game1.isRaining)
                        {
                            if (height == Lowered && !flooded)
                                flooded = true;
                            if (!hydrated)
                                hydrated = true;
                            holdOver = true;
                        }
                        if (height == Lowered)
                        {
                            PerennialsGlobal.equalizeDitches(location);
                            addWaterSource(location, tileLocation);
                        }
                        else
                            removeWaterSource(location, tileLocation);
                        return false;
                    }
                    return true;
                }
                else if (t.GetType() == typeof(WateringCan))
                {
                    if (!hydrated)
                        hydrated = true;
                    if (height == Lowered && hydrated)
                        flooded = true;
                    holdOver = true;
                }
                else if (t is MeleeWeapon && (t as MeleeWeapon).Name.Equals("Scythe"))
                {
                    if (this.crop != null && this.crop.isScytheHarvest() && this.crop.harvest(tileLocation))
                        this.destroyCrop(tileLocation, true);
                    if (this.crop != null && this.crop.dead)
                        this.destroyCrop(tileLocation, true);
                }
                else if (t.GetType() == typeof(Pickaxe) && crop == null)
                {
                    removeWaterSource(location, tileLocation);
                    return true;
                }
                else if (t.isHeavyHitter() && t.GetType() != typeof(Shovel) && (!(t is MeleeWeapon) && crop != null))
                    destroyCrop(tileLocation, true);
                this.shake((float)Math.PI / 32f, (float)Math.PI / 40f, (double)tileLocation.X * (double)Game1.tileSize < (double)Game1.player.position.X);
            }
            return false;
        }

        private void removeWaterSource(GameLocation location, Vector2 tileLocation)
        {
            if (location == null)
                location = Game1.getFarm();
            if (location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Water", "Back") != null)
            {
                try
                {
                    location.Map.GetLayer("Back").Tiles[(int)tileLocation.X, (int)tileLocation.Y].Properties.Remove("Water");
                }
                catch (Exception ex)
                {
                    Logger.Log("Could not remove water source @(" + tileLocation.ToString() + ")!");
                }
            }
        }

        private void addWaterSource(GameLocation location, Vector2 tileLocation)
        {
            if (location == null)
                location = Game1.getFarm();
            //Just in case.
            if (height != Lowered)
            {
                flooded = false;
                return;
            }
            location.setTileProperty((int)tileLocation.X, (int)tileLocation.Y, "Back", "Water", "true");
        }

        public List<Vector2> getAdjacent(Vector2 origin, int size, bool square=false)
        {
            List<Vector2> adjacentTiles = new List<Vector2>();
            float leastX = origin.X - size;
            float maxX = origin.X + size;
            float leastY = origin.Y - size;
            float maxY = origin.Y + size;
            for(float x = leastX; x <= maxX; x++)
            {
                for(float y = leastY; y <= maxY; y++)
                {
                    if (x == origin.X && y == origin.Y)
                        continue;
                    if (!square && !(Math.Abs(x - origin.X) + Math.Abs(y - origin.Y) <= size))
                        continue;
                    adjacentTiles.Add(new Vector2(x, y));
                }
            }
            return adjacentTiles;
        }

        public void hydrateAdjacent(GameLocation environment, Vector2 tileLocation)
        {
            List<Vector2> adjacentTiles = getAdjacent(tileLocation, 2, false);

            foreach (Vector2 key in adjacentTiles)
            {
                if (environment.terrainFeatures.ContainsKey(key) && environment.terrainFeatures[key].GetType() == typeof(CropSoil))
                {
                    CropSoil adjacent = (CropSoil)environment.terrainFeatures[key];
                    if (adjacent.height != Lowered)
                    {
                        if (!adjacent.hydrated || !adjacent.holdOver)
                        {
                            adjacent.hydrated = true;
                            adjacent.holdOver = true;
                        }
                    }
                }
            }
        }

        public void dayUpdate(GameLocation environment, Vector2 tileLocation, string spoofSeason)
        {
            spoofedSeason = spoofSeason;
            dayUpdate(environment, tileLocation);
        }

        public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
        {
            if(!(environment is Farm) && !environment.name.Equals("greenhouse"))
            {
                height = Flat;
                crop = null;
            }
            if (Game1.isRaining)
            {
                if (height == Lowered && !flooded)
                    flooded = true;
                if (!hydrated)
                    hydrated = true;
                holdOver = true;
            }
            else
            {
                if (!holdOver || height == Raised)
                {
                    if (flooded)
                        flooded = false;
                    else if (hydrated)
                        hydrated = false;
                    if (height != Raised && hydrated)
                        holdOver = true;
                }
                if (holdOver && height != Raised)
                {
                    holdOver = false;
                }
            }
            if (flooded)
            {
                addWaterSource(environment, tileLocation);
            }
            else
                removeWaterSource(environment, tileLocation);
            if (crop != null)
            {
                crop.npk = npk;
                crop.weeds = weeds;
                crop.newDay(hydrated, flooded, (int)tileLocation.X, (int)tileLocation.Y, environment, spoofedSeason);
                if (crop.hasFruit)
                {
                    int neighbors = 0;
                    List<Vector2> adjacentTiles = getAdjacent(tileLocation, 2, true);
                    foreach(Vector2 key in adjacentTiles)
                    {
                        if(environment.terrainFeatures.ContainsKey(key) && environment.terrainFeatures[key] is CropSoil)
                        {
                            CropSoil neighbor = (CropSoil)environment.terrainFeatures[key];
                            //Same hat!
                            if(neighbor.crop != null && neighbor.crop.crop.Equals(crop.crop) && !neighbor.crop.dead && neighbor.crop.mature)
                                neighbors++;
                        }
                    }
                    crop.neighbors = neighbors;
                    //Logger.Log(crop.crop + " plant at " + tileLocation.ToString() + " found " + neighbors + " neighbors.  Neighbors is now at " + crop.neighbors);
                }
                if (!crop.isGrowingSeason(spoofedSeason, environment))
                    npk = new int[]{0,0,0};
            }
            //Weeds do not affect the crop the day they grow, so they are placed after the crop does its day update.
            //Weeds present at the start of a day update subtract from the weedless bonus.
            if (height != Raised && !flooded && Game1.random.NextDouble() <= weedsChance)
            {
                if (crop != null)
                    weeds = crop.produceWeeds();
                else
                    weeds = true;
            }
        }

        private void calculateAdjacency(Vector2 tileLocation, GameLocation location = null)
        {
            if (location is null)
                location = Game1.currentLocation;
            index1 = 0;
            index2 = 0;
            index3 = 0;
            Vector2 key = tileLocation;
            ++key.X;
            if (location.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key].GetType() == typeof(CropSoil))
            {
                if ((Game1.currentLocation.terrainFeatures[key] as CropSoil).height == height)
                {
                    index1 += 100;
                    if (((CropSoil)Game1.currentLocation.terrainFeatures[key]).hydrated == this.hydrated)
                        index2 += 100;
                    if (height == Lowered)
                    {
                        if (((CropSoil)Game1.currentLocation.terrainFeatures[key]).flooded == this.flooded)
                            index3 += 100;
                    }
                }
            }
            key.X -= 2f;
            if (location.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key].GetType() == typeof(CropSoil))
            {
                if ((Game1.currentLocation.terrainFeatures[key] as CropSoil).height == height)
                {
                    index1 += 10;
                    if (((CropSoil)Game1.currentLocation.terrainFeatures[key]).hydrated == this.hydrated)
                        index2 += 10;
                    if (height == Lowered)
                    {
                        if (((CropSoil)Game1.currentLocation.terrainFeatures[key]).flooded == this.flooded)
                            index3 += 10;
                    }
                }
            }
            ++key.X;
            ++key.Y;
            if (location.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key].GetType() == typeof(CropSoil))
            {
                if ((Game1.currentLocation.terrainFeatures[key] as CropSoil).height == height)
                {
                    index1 += 500;
                    if (((CropSoil)Game1.currentLocation.terrainFeatures[key]).hydrated == this.hydrated)
                        index2 += 500;
                    if (height == Lowered)
                    {
                        if (((CropSoil)Game1.currentLocation.terrainFeatures[key]).flooded == this.flooded)
                            index3 += 500;
                    }
                }
            }
            key.Y -= 2f;
            if (location.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key].GetType() == typeof(CropSoil))
            {
                if ((Game1.currentLocation.terrainFeatures[key] as CropSoil).height == height)
                {
                    index1 += 1000;
                    if (((CropSoil)Game1.currentLocation.terrainFeatures[key]).hydrated == this.hydrated)
                        index2 += 1000;
                    if (height == Lowered)
                    {
                        if (((CropSoil)Game1.currentLocation.terrainFeatures[key]).flooded == this.flooded)
                            index3 += 1000;
                    }
                }
            }
        }

        public override bool tickUpdate(GameTime time, Vector2 tileLocation, GameLocation location)
        {

            if ((double)this.maxShake > 0.0)
            {
                if (this.shakeLeft)
                {
                    this.shakeRotation = this.shakeRotation - this.shakeRate;
                    if ((double)Math.Abs(this.shakeRotation) >= (double)this.maxShake)
                        this.shakeLeft = false;
                }
                else
                {
                    this.shakeRotation = this.shakeRotation + this.shakeRate;
                    if ((double)this.shakeRotation >= (double)this.maxShake)
                    {
                        this.shakeLeft = true;
                        this.shakeRotation = this.shakeRotation - this.shakeRate;
                    }
                }
                this.maxShake = Math.Max(0.0f, this.maxShake - (float)Math.PI / 300f);
            }
            else
                this.shakeRotation = this.shakeRotation / 2f;
            return false;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
        {
            int index = 0;
            Vector2 key = tileLocation;
            ++key.X;
            GameLocation locationFromName = Game1.getLocationFromName("Farm");
            if (locationFromName.terrainFeatures.ContainsKey(key) && locationFromName.terrainFeatures[key].GetType() == typeof(CropSoil))
                index += 100;
            key.X -= 2f;
            if (locationFromName.terrainFeatures.ContainsKey(key) && locationFromName.terrainFeatures[key].GetType() == typeof(CropSoil))
                index += 10;
            ++key.X;
            ++key.Y;
            if (Game1.currentLocation.terrainFeatures.ContainsKey(key) && locationFromName.terrainFeatures[key].GetType() == typeof(CropSoil))
                index += 500;
            key.Y -= 2f;
            if (locationFromName.terrainFeatures.ContainsKey(key) && locationFromName.terrainFeatures[key].GetType() == typeof(CropSoil))
                index += 1000;
            int num = CropSoil.drawGuide[index];
            spriteBatch.Draw(CropSoil.flatTexture, positionOnScreen, new Rectangle?(new Rectangle(num % 4 * Game1.tileSize, num / 4 * Game1.tileSize, Game1.tileSize, Game1.tileSize)), Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth + positionOnScreen.Y / 20000f);
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            GameLocation location = Game1.currentLocation;
            if (flatTexture is null)
                loadSprite();
            calculateAdjacency(tileLocation);
            int num1 = CropSoil.drawGuide[index1];
            int num2 = CropSoil.drawGuide[index2];
            int num3 = CropSoil.drawGuide[index3];
            Texture2D texture;
            if (height == Raised)
                texture = highTexture;
            else if (height == Lowered)
                texture = lowTexture;
            else
                texture = flatTexture;
            spriteBatch.Draw(texture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)),
                new Rectangle?(new Rectangle(num1 % 4 * 16, num1 / 4 * 16, 16, 16)),
                this.c,
                0.0f,
                Vector2.Zero,
                (float)Game1.pixelZoom,
                SpriteEffects.None,
                1E-08f
            );
            if(height == Lowered)
            {
                int leftCrop = index1 % 100 == 10 ? 0 : 8;
                int rightCrop = index1 % 500 >= 100 ? 0 : 8;
                int bottomCrop = index1 % 1000 >= 500 ? 0 : 8;
                int topCrop = index1 >= 1000 ? 0 : 8;
                spriteBatch.Draw(
                    Game1.mouseCursors,
                    Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)),
                    new Microsoft.Xna.Framework.Rectangle?(
                        new Rectangle(
                            leftCrop + location.waterAnimationIndex * 64,
                            topCrop + 2064 + ((tileLocation.X + (tileLocation.Y + 1)) % 2 == 0 ? (location.waterTileFlip ? 128 : 0) : (location.waterTileFlip ? 0 : 128)),
                            64 - (leftCrop + rightCrop),
                            64 - (topCrop + bottomCrop)
                    )),
                    (Color)((NetFieldBase<Color, NetColor>)location.waterColor),
                    0.0f,
                    new Vector2(leftCrop * -1, topCrop * -1),
                    1f,
                    SpriteEffects.None,
                    1.2E-08f
                );
                spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)), new Rectangle?(new Rectangle(num3 % 4 * 16 + 128, num3 / 4 * 16, 16, 16)), this.c, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1.4E-08f);
            }
            //if (flooded)
            //    spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)), new Rectangle?(new Rectangle(num3 % 4 * 16 + 128, num3 / 4 * 16, 16, 16)), this.c, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1.2E-08f);
            //else if (hydrated)
            //    spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)), new Rectangle?(new Rectangle(num2 % 4 * 16 + 64, num2 / 4 * 16, 16, 16)), this.c, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1.2E-08f);
            if (weeds)
            {
                int num4 = (int)tileLocation.X * 7 + (int)tileLocation.Y * 11;
                int seasonNum = spoofedSeason.Equals("spring") ? 0 : (spoofedSeason.Equals("summer") ? 1 : (spoofedSeason.Equals("fall") ? 2 : 3));
                spriteBatch.Draw(SoilCrop.cropSpriteSheet,
                    Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((double)tileLocation.X * (double)Game1.tileSize) + (Game1.tileSize / 2), (float)((double)tileLocation.Y * (double)Game1.tileSize) + (Game1.tileSize / 2) - (Game1.tileSize * height / 4))),
                    new Rectangle(128 + num4 % 2 + (seasonNum * 2) * 16, 1024, 16, 32),
                    Color.White,
                    shakeRotation,
                    new Vector2(8f, 24f),
                    (float)Game1.pixelZoom,
                    num4 % 2 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    (float)(((double)tileLocation.Y * (double)Game1.tileSize + (double)(Game1.tileSize / 2) + (((double)tileLocation.Y * 11.0 + (double)tileLocation.X * 7.0) % 10.0 - 5.0)) / 10000.0 / 2.1)
                );
            }
            if (crop == null)
                return;
            crop.draw(spriteBatch, tileLocation, Color.White, shakeRotation);
        }

        private SoilCrop getCrop(string which)
        {
            string cropType = SoilCrop.getCropType(which);
            SoilCrop newCrop;
            if (cropType != "")
            {
                //Certain crops are of special types, which are their own classes.  This is where those are selected.
                if (cropType.Equals("Bush"))
                {
                    newCrop = new CropBush(which, height);
                }
                else if (cropType.Equals("Root"))
                {
                    newCrop = new CropRoot(which, height);
                }
                else if (cropType.Equals("Trellis"))
                {
                    newCrop = new CropTrellis(which, height);
                }
                else if (cropType.Equals("Sprawler"))
                {
                    newCrop = new CropSprawler(which, height);
                }
                else
                {
                    newCrop = new SoilCrop(which, height);
                }
            }
            else
            {
                newCrop = new SoilCrop(which, height);
            }
            return newCrop;
        }

        public Dictionary<string, string> Save()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["height"] = height.ToString();
            data["hydrated"] = hydrated.ToString();
            data["flooded"] = flooded.ToString();
            data["holdOver"] = holdOver.ToString();
            if(crop != null)
            {
                data["crop"] = crop.crop;
                Dictionary<string, string> cropData = crop.Save();
                foreach(string key in cropData.Keys)
                {
                    data["crop_" + key] = cropData[key];
                }
            }
            return data;
        }

        public void Load(Dictionary<string, string> data)
        {
            height = Convert.ToInt32(data["height"]);
            hydrated = Convert.ToBoolean(data["hydrated"]);
            flooded = Convert.ToBoolean(data["flooded"]);
            holdOver = Convert.ToBoolean(data["holdOver"]);
            if (data.ContainsKey("crop"))
            {
                crop = getCrop(data["crop"]);
                Dictionary<string, string> cropData = new Dictionary<string, string>();
                foreach (string key in data.Keys)
                {
                    if (key.StartsWith("crop_"))
                    {
                        string cropKey = key.Substring(5);
                        cropData[cropKey] = data[key];
                    }
                }
                crop.Load(cropData);
            }
            return;
        }
    }
}
