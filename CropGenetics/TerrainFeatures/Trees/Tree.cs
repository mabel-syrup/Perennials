using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;
using _SyrupFramework;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Perennials
{
    public class Tree : TerrainFeature, IModdedItem
    {
        //The sprite is split into multiple parts, allowing it to hide pieces of the tree as needed for vision
        public Rectangle trunkBase;
        public Rectangle trunk;

        //Global
        public static Dictionary<string, Texture2D> trunkSheets;

        //Species-specific
        public string treeName;
        public string treeID;
        public Texture2D trunkSheet;
        public int shadeRadius;
        public float shakeRate;
        public float shakeDecayRate;
        public int maxHealth;

        //How many segments tall the tree can get
        public int maxHeight;
        //Base daily growth (in segments per day)
        public float growthRate;
        //How much variance there is among heights in identally-aged trees, where 0 is identical heights and 1 is anything from no growth to double the average for its age.
        public float variance;
        //How many times, on average, a tree of this species will split its trunk
        public float averageTrunkSplits;
        //How likely the tree is to split during any particular growth.  High numbers concentrate the splits towards the bottom of the tree.
        public float splitTendency;

        //How well the tree supports its own weight.  Low trunk strength results in rotations becoming greater over time and with more weight above.  1 has no bending, 0 topples immediately.
        public float trunkStrength;
        //How vertical the trunk seeks to be.  1 seeks to be perfectly straight, 0 is completely happy growing horizontally.
        public float trunkVerticalness;
        //How much the growth attempts to stay true to its ideal verticalness.  1 will curve at the highest rate possible to meet it, 0 will make no attempt at all.
        public float trunkSeeking;
        //Maximum rotation angle between segments
        public float trunkWavering;

        //Individual-specific
        public int ageDays;
        public int ageYears;
        public bool stump;
        public bool tapped;
        public float health;
        public string birthSeason;
        public int birthDay;

        //Structure
        public TreeComponent treeStructure;

        public Tree() : base(true) {}

        public override Rectangle getBoundingBox(Vector2 tileLocation)
        {
            return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
        }

        public TreeComponent newStump()
        {
            return new TreeComponent(trunkSheet, trunkBase, TreeComponent.Stump, TreeComponent.AlwaysRender);
        }

        public virtual void extendTrunk()
        {
            //Gets the terminating trunk piece, and adds the new addition to it.
            //TreeComponent terminatingTrunk = treeStructure.getFinalChildOfType(TreeComponent.Trunk);
            string trunkReport = "Extending trunk...";
            if (treeStructure.thisBranchTerminatesHere())
            {
                trunkReport += "\nTree is still stump, adding one trunk piece...";
                TreeComponent newAddition = new TreeComponent(trunkSheet, trunk, new Vector2(0, -8 * Game1.pixelZoom), TreeComponent.Trunk);
                treeStructure.addChild(newAddition);
                newAddition.rotateConstrainedDegrees(0f, (float)Game1.random.NextDouble() * 60f - 30f, 0.4f);
                trunkReport += "\nAdded trunk with a rotation of " + newAddition.rotation / 0.0175f + " degrees.";
            }
            else {
                List<TreeComponent> trunkEnds = treeStructure.getAllFinalChildrenOfType(TreeComponent.Trunk);
                trunkReport += "\nTree has a trunk, found " + trunkEnds.Count + " trunk ends.";
                foreach (TreeComponent trunkEnd in trunkEnds)
                {
                    TreeComponent newAddition = new TreeComponent(trunkSheet, trunk, new Vector2(0, -8 * Game1.pixelZoom), TreeComponent.Trunk);
                    trunkEnd.addChild(newAddition);
                    newAddition.rotateConstrainedDegrees(0f, (float)Game1.random.NextDouble() * 60f - 30f, 0.4f);
                    trunkReport += "\nAdded a trunk piece with a rotation of " + newAddition.rotation / 0.0175f + " degrees.";
                    if (Game1.random.Next() % 9 == 0)
                    {
                        trunkReport += "\nAdding split...";
                        TreeComponent newSplit = new TreeComponent(trunkSheet, trunk, new Vector2(0, -8 * Game1.pixelZoom), TreeComponent.Trunk);
                        trunkEnd.addChild(newSplit);
                        newSplit.rotateConstrainedDegrees(0f, (float)Game1.random.NextDouble() * 60f - 30f, 0.1f);
                        trunkReport += "\nAdded a split trunk piece with a rotation of " + newSplit.rotation / 0.0175f + " degrees.";
                    }
                }
            }
            Logger.Log(trunkReport);
        }

        public override bool tickUpdate(GameTime time, Vector2 tileLocation, GameLocation location)
        {
            //treeStructure.rotate(0.01f);
            return base.tickUpdate(time, tileLocation, location);
        }

        public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
        {
            ageDays++;
            if (ageDays % 112 == 0)
                ageYears++;
            doDailyGrowth(environment, tileLocation);
        }

        public virtual void doDailyGrowth(GameLocation environment, Vector2 tileLocation)
        {

            int currentHeight = treeStructure.getHeight();
            float averageHeight = growthRate * ageDays;
            //Grow if the difference between the average height and the tree's height, plus a random value between a negative half its average height and half its average height, is positive.
            bool willGrow = (averageHeight - currentHeight) + ((Game1.random.NextDouble() * variance * averageHeight) - (averageHeight / 2)) >= 0;
            if (willGrow)
            {
                List<TreeComponent> trunkEnds = treeStructure.getAllFinalChildrenOfType(TreeComponent.Trunk);
                TreeComponent highestEnd = treeStructure.getFinalChildOfType(TreeComponent.Trunk);
                bool shouldSplit = (averageTrunkSplits - (trunkEnds.Count - 1) * splitTendency + (Game1.random.NextDouble() * averageTrunkSplits - (averageTrunkSplits / 2)) > 0);
                if (shouldSplit)
                {
                    
                }
            }
        }

        public virtual void addTrunkExtension(float rotation, TreeComponent parent)
        {
            TreeComponent newAddition = new TreeComponent(trunkSheet, trunk, new Vector2(0, -8 * Game1.pixelZoom), TreeComponent.Trunk);
            parent.addChild(newAddition);
            newAddition.rotateConstrainedDegrees(trunkVerticalness, rotation, trunkSeeking);
        }

        public virtual void addTrunkExtension(TreeComponent parent)
        {
            addTrunkExtension(getRandomNewAngle(), parent);
        }

        public virtual void addSplit(float splitAngle, TreeComponent parent, float difference = 30)
        {
            float angle = getRandomNewAngle();
            angle -= (float)(difference * 0.5);
            float angleB = angle + difference;

        }

        public virtual float getRandomNewAngle()
        {
            return (float)Game1.random.NextDouble() * trunkWavering - (trunkWavering / 2);
        }

        public override void draw(SpriteBatch b, Vector2 tileLocation)
        {
            treeStructure.draw(b, (tileLocation * Game1.tileSize) + new Vector2(32f, 64f), Color.White, 0f, false, (float)((tileLocation.Y * 64 + 32) + (tileLocation.Y * 11.0 + tileLocation.X * 7.0) % 10.0 - 5.0) / 10000f);
        }

        public void Load(Dictionary<string, string> data)
        {
            CustomData saveData = new CustomData(data);
            saveData.get("days", ref ageDays);
            saveData.get("years", ref ageYears);
            saveData.get("stump", ref stump);
            saveData.get("tapped", ref tapped);
            saveData.get("health", ref health);
            Load(saveData);
        }

        public virtual void Load(CustomData saveData) {}

        public virtual Dictionary<string,string> Save(CustomData saveData)
        {
            saveData.add("days", ageDays);
            saveData.add("years", ageYears);
            saveData.add("stump", stump);
            saveData.add("tapped", tapped);
            saveData.add("health", health);
            return saveData.build();
        }

        public Dictionary<string, string> Save()
        {
            return Save(new CustomData());
        }
    }
}
