using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using StardewValley;
using StardewValley.Tools;
using _SyrupFramework;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace Perennials
{
    public class Trowel : Tool, IModdedItem
    {
        public Trowel() : base("Trowel", 0, 7, 7, false, 0)
        {
            numAttachmentSlots.Value = 1;
            this.attachments.SetCount((int)((NetFieldBase<int, NetInt>)this.numAttachmentSlots));
            this.upgradeLevel.Value = 0;
        }

        protected override string loadDisplayName()
        {
            return "Trowel";
        }

        protected override string loadDescription()
        {
            return "Use this to plant seeds from packets.";
        }

        public override int attachmentSlots()
        {
            return 1;
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            Logger.Log("Used trowel; no seeds.");
            //TODO: Remove this test code
            power = who.toolPower;
            who.stopJittering();
            Game1.playSound("woodyHit");
            Vector2 vector2 = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            List<Vector2> vector2List = this.tilesAffected(vector2, power, who);
            foreach (Vector2 index in vector2List)
            {
                index.Equals(vector2);
                if (location.terrainFeatures.ContainsKey(index))
                {
                    TerrainFeature feature = location.terrainFeatures[index];
                    if (feature is CropSoil && (feature as CropSoil).weeds)
                    {
                        Logger.Log("Trowel used on cropsoil @" + index.ToString() + "!\nRemoving weeds @(" + (int)index.X + ", " + (int)index.Y + ")...");
                        ((CropSoil)feature).weeds = false;
                        Logger.Log("Weeding was " + (((CropSoil)feature).weeds ? "unsuccessful." : "successful."));
                    }
                    if(feature is CropSoil && (feature as CropSoil).height == CropSoil.Lowered)
                    {
                        IrrigationBridge bridge = new IrrigationBridge();
                        location.terrainFeatures[index] = bridge;
                    }
                }
                else
                {
                    Tree tree = new WhiteOak();
                    location.terrainFeatures[index] = tree;
                }
            }
            Logger.Log("Used trowel");

        }

        public override StardewValley.Object attach(StardewValley.Object o)
        {
            if(o != null && o is SeedPacket)
            {
                StardewValley.Object @object = this.attachments[0];
                if(@object != null && @object.canStackWith((Item)o))
                {
                    @object.Stack = o.addToStack(@object.Stack);
                    if (@object.Stack <= 0)
                        @object = (StardewValley.Object)null;
                }
                this.attachments[0] = o;
                Game1.playSound("button1");
                return @object;
            }
            if (o == null)
            {
                if (this.attachments[0] != null)
                {
                    StardewValley.Object attachment = this.attachments[0];
                    this.attachments[0] = (StardewValley.Object)null;
                    Game1.playSound("dwop");
                    return attachment;
                }
            }
            return (StardewValley.Object)null;
        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            if (this.attachments[0] == null)
            {
                b.Draw(Game1.menuTexture, new Vector2((float)x, (float)y), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 36, -1, -1)), Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
            }
            else
            {
                b.Draw(Game1.menuTexture, new Vector2((float)x, (float)y), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
                this.attachments[0].drawInMenu(b, new Vector2((float)x, (float)y), 1f);
            }
        }

        public override Item getOne()
        {
            return new Trowel();
        }

        public Dictionary<string, string> Save()
        {
            return new Dictionary<string, string>();
        }

        public void Load(Dictionary<string, string> data)
        {
            return;
        }
    }
}
