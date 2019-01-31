using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Characters;
using _SyrupFramework;

namespace Perennials
{
    public class ModEntry : Mod
    {
        private bool gameLoaded = false;

        public override void Entry(IModHelper helper)
        {
            Global.addType(new Shovel());
            Global.addType(new Trowel());
            Global.addType(new CropSoil());
            Global.addType(new SeedPacket());
            Global.addType(new UtilityWand());
            Global.addType(new Fruit());
            Global.addHandler(new PerennialsHandler());
            CropSoil.populateDrawGuide();
            CropBush.bushSprites = new Dictionary<string, Texture2D>();
            CropSoil.flatTexture = helper.Content.Load<Texture2D>("assets/cropsoil_flat.png", ContentSource.ModFolder);
            CropSoil.highTexture = helper.Content.Load<Texture2D>("assets/cropsoil_raised.png", ContentSource.ModFolder);
            CropSoil.lowTexture = helper.Content.Load<Texture2D>("assets/cropsoil_lowered.png", ContentSource.ModFolder);
            SeedPacket.parentSheet = helper.Content.Load<Texture2D>("assets/seed_packets.png", ContentSource.ModFolder);
            SoilCrop.cropSpriteSheet = helper.Content.Load<Texture2D>("assets/crops_new.png", ContentSource.ModFolder);
            Fruit.fruitSheet = helper.Content.Load<Texture2D>("assets/fruits.png", ContentSource.ModFolder);
            SeedPacket.seeds = helper.Content.Load<Dictionary<string, string>>("data/Seeds.xnb", ContentSource.ModFolder);
            SoilCrop.cropDictionary = helper.Content.Load<Dictionary<string, string>>("data/Crops.xnb", ContentSource.ModFolder);
            Fruit.fruits = helper.Content.Load<Dictionary<string, string>>("data/Fruits.xnb", ContentSource.ModFolder);
            Helper.Events.GameLoop.DayStarted += SaveEvents_AfterLoad;
            Helper.Events.Input.ButtonPressed += DetectToolUse;
            Helper.Events.Display.MenuChanged += MenuEvents_MenuChanged;
            loadCropData();
            //Helper.Events.Display.RenderingActiveMenu += MenuEvents_MenuChanged;
        }

        private void loadCropData()
        {
            foreach(string crop in SoilCrop.cropDictionary.Keys)
            {
                Dictionary<string, string> cropData = SoilCrop.getCropFromXNB(SoilCrop. cropDictionary[crop]);
                string specialType = cropData["specialType"];
                if (specialType.Equals("Bush"))
                {
                    Logger.Log("Loading bush sprite sheet for " + crop + "...");
                    try
                    {
                        CropBush.bushSprites[crop] = Helper.Content.Load<Texture2D>("assets/bush/" + crop + ".png", ContentSource.ModFolder);
                    }
                    catch (Microsoft.Xna.Framework.Content.ContentLoadException)
                    {
                        Logger.Log("Could not find image file 'assets/bush/" + crop + ".png'!", LogLevel.Error);
                    }
                }
            }
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            giveTestSeeds();
            gameLoaded = true;
        }


        //TODO: Allow click and hold to mass-place seeds like a normal seed packet.
        private void DetectToolUse(object sender, ButtonPressedEventArgs e)
        {
            if (!SButtonExtensions.IsUseToolButton(e.Button))
                return;
            if (!gameLoaded)
                return;
            if (Game1.player.CurrentItem is null)
                return;
            if (Game1.fadeToBlack)
                return;
            if (!(Game1.currentLocation is Farm || (Game1.currentLocation.Name != null && Game1.currentLocation.name.Equals("Greenhouse"))))
                return;
            if(Game1.player.CurrentItem is SeedPacket)
            {
                Vector2 vector2 = !Game1.wasMouseVisibleThisFrame ? Game1.player.GetToolLocation(false) : new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y));

                ((SeedPacket)Game1.player.CurrentItem).placementAction(Game1.currentLocation, (int)vector2.X, (int)vector2.Y, Game1.player);
            }
        }

        private void MenuEvents_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            IClickableMenu menu = e.NewMenu;
            if (menu is ShopMenu)
            {
                ShopMenu sMenu = (ShopMenu)menu;
                if (sMenu.portraitPerson == Game1.getCharacterFromName("Pierre", false))
                {
                    //It's pierre's shop!
                    List<Item> newShopInventory = new List<Item>();
                    Dictionary<Item, int[]> newShopPriceAndStock = new Dictionary<Item, int[]>();
                    foreach (string cropName in SeedPacket.seeds.Keys)
                    {
                        SeedPacket seed = new SeedPacket(cropName);
                        newShopInventory.Add(seed);
                        newShopPriceAndStock.Add(seed, new int[2]
                        {
                            seed.salePrice(),
                            int.MaxValue
                        });
                    }
                    IReflectedField<List<Item>> shopInventory = this.Helper.Reflection.GetField<List<Item>>(sMenu, "forSale");
                    foreach (Item item in shopInventory.GetValue())
                    {
                        if (item.Category != -74)
                        {
                            newShopInventory.Add(item);
                            newShopPriceAndStock.Add(item, new int[]
                            {
                                item.salePrice(),
                                item.Stack
                            });
                        }

                    }
                    shopInventory.SetValue(newShopInventory);
                    IReflectedField<Dictionary<Item, int[]>> shopPriceAndStock = this.Helper.Reflection.GetField<Dictionary<Item, int[]>>(sMenu, "itemPriceAndStock");
                    shopPriceAndStock.SetValue(newShopPriceAndStock);
                }
            }
        }

        private void giveTestSeeds()
        {
            foreach(StardewValley.Farmer farmer in Game1.getAllFarmers())
            {
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new Trowel());
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new Shovel());
                farmer.addItemByMenuIfNecessaryElseHoldUp(new SeedPacket("Cauliflower", 28));
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new SeedPacket("Strawberry", 28));
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new UtilityWand());
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new Fruit("Blueberry", 5, Fruit.lowQuality));
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new Fruit("Blueberry", 5, Fruit.medQuality));
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new Fruit("Blueberry", 5, Fruit.highQuality));
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new Fruit("Blueberry", 5, Fruit.bestQuality));
            }
        }
    }
}