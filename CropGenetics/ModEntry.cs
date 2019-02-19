using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Locations;
using StardewValley.Characters;
using Netcode;
using _SyrupFramework;

namespace Perennials
{
    public class ModEntry : Mod
    {
        private bool gameLoaded = false;

        public override void Entry(IModHelper helper)
        {
            //Register items for saving
            Global.addType(new Shovel());
            Global.addType(new Trowel());
            Global.addType(new CropSoil());
            Global.addType(new SeedPacket());
            Global.addType(new UtilityWand());
            Global.addType(new Fruit());
            Global.addType(new Tree());
            Global.addType(new Spigot());
            Global.addType(new Fertilizer());
            Global.addType(new CompostBin());

            //Register this mod's handler for modded saving events
            Global.addHandler(new PerennialsHandler());
            StepSoundHelper.getSounds();

            //Populate draw guides
            CropSoil.populateDrawGuide();
            CropSprawler.populateDrawGuide();
            IrrigationBridge.populateDrawGuide();

            //Initialize spritesheet dictionaries
            CropBush.bushSprites = new Dictionary<string, Texture2D>();
            CropSprawler.sprawlerSprites = new Dictionary<string, Texture2D>();
            Tree.trunkSheets = new Dictionary<string, Texture2D>();

            //Load spritesheets
            CropSoil.flatTexture = helper.Content.Load<Texture2D>("assets/cropsoil_flat.png", ContentSource.ModFolder);
            CropSoil.highTexture = helper.Content.Load<Texture2D>("assets/cropsoil_raised.png", ContentSource.ModFolder);
            CropSoil.lowTexture = helper.Content.Load<Texture2D>("assets/cropsoil_lowered.png", ContentSource.ModFolder);
            SeedPacket.parentSheet = helper.Content.Load<Texture2D>("assets/seed_packets.png", ContentSource.ModFolder);
            SoilCrop.cropSpriteSheet = helper.Content.Load<Texture2D>("assets/crops_new.png", ContentSource.ModFolder);
            Fruit.fruitSheet = helper.Content.Load<Texture2D>("assets/fruits.png", ContentSource.ModFolder);
            Fertilizer.fertilizerSheet = helper.Content.Load<Texture2D>("assets/fertilizers.png", ContentSource.ModFolder);
            IrrigationBridge.floorSheet = helper.Content.Load<Texture2D>("TerrainFeatures/Flooring", ContentSource.GameContent);
            IrrigationBridge.fenceSheet = helper.Content.Load<Texture2D>("assets/bridgeFenceTemplate.png", ContentSource.ModFolder);
            PerennialsGlobal.objectSpriteSheet = helper.Content.Load<Texture2D>("assets/craftables.png", ContentSource.ModFolder);

            //Load xnb files
            SeedPacket.seeds = helper.Content.Load<Dictionary<string, string>>("data/Seeds.xnb", ContentSource.ModFolder);
            SoilCrop.cropDictionary = helper.Content.Load<Dictionary<string, string>>("data/Crops.xnb", ContentSource.ModFolder);
            Fruit.fruits = helper.Content.Load<Dictionary<string, string>>("data/Fruits.xnb", ContentSource.ModFolder);
            Fertilizer.fertilizers = helper.Content.Load<Dictionary<string, string>>("data/Fertilizers.xnb", ContentSource.ModFolder);

            //Initialize player height dictionaries
            PerennialsGlobal.initDictionaries();

            //Hook into the multiplayer object?
            PerennialsGlobal.multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            //Add handlers for events
            Helper.Events.GameLoop.DayStarted += SaveEvents_AfterLoad;
            //Helper.Events.Input.ButtonPressed += DetectToolUse;
            Helper.Events.Display.MenuChanged += MenuEvents_MenuChanged;
            //Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicking;

            //Load the sprite sheets for specialized crops
            loadCropData();

            //Load the sprite sheets for trees
            loadTreeData();
            //Helper.Events.Display.RenderingActiveMenu += MenuEvents_MenuChanged;
        }

        private void loadTreeData()
        {
            //For testing purposes, only loads the white oak
            string treeID = "whiteoak";
            Logger.Log("Loading trunk sprite sheet for " + treeID + "...");
            try
            {
                Tree.trunkSheets[treeID] = Helper.Content.Load<Texture2D>("assets/tree/" + treeID + "/trunk.png", ContentSource.ModFolder);
            }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException)
            {
                Logger.Log("Could not find image file 'assets/tree/" + treeID + "/trunk.png'!", LogLevel.Error);
            }
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
                else if (specialType.Equals("Sprawler"))
                {
                    Logger.Log("Loading sprawler sprite sheet for " + crop + "...");
                    try
                    {
                        CropSprawler.sprawlerSprites[crop] = Helper.Content.Load<Texture2D>("assets/sprawler/" + crop + ".png", ContentSource.ModFolder);
                    }
                    catch (Microsoft.Xna.Framework.Content.ContentLoadException)
                    {
                        Logger.Log("Could not find image file 'assets/sprawler/" + crop + ".png'!", LogLevel.Error);
                    }
                }
            }
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            giveTestSeeds();
            gameLoaded = true;
        }

        private void GameLoop_UpdateTicking(object sender, EventArgs e)
        {
            if (!gameLoaded || Game1.player.CurrentItem is null || Game1.fadeToBlack || Game1.menuUp)
                return;
            //if(!Game1.eventUp || Game1.isFestival())
                //handleWhileDown();
        }

        private void handleWhileDown()
        {
            //SButton action =   Options.input_actionButton
            SButton actionButton = SButton.MouseRight;
            if (!this.Helper.Input.IsDown(actionButton) && !this.Helper.Input.IsDown(SButton.MouseLeft))
                return;
            if (!(Game1.currentLocation is Farm || (Game1.currentLocation.Name != null && Game1.currentLocation.name.Equals("Greenhouse"))))
                return;
            if (Game1.player.CurrentItem is SeedPacket)
            {
                Vector2 vector2 = !Game1.wasMouseVisibleThisFrame ? Game1.player.GetToolLocation(false) : new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y));

                if(((SeedPacket)Game1.player.CurrentItem).placementAction(Game1.currentLocation, (int)vector2.X, (int)vector2.Y, Game1.player)){
                    Game1.player.reduceActiveItemByOne();
                }
            }
        }


        //Vanilla code, slightly altered.  Possibly going to use this in the future, but right now is unused.
        private void tryToPlaceItem(GameLocation location, Item item, int x, int y)
        {
            if (item is Tool)
                return;
            Vector2 vector2 = new Vector2((float)(x / 64), (float)(y / 64));
            if (Utility.playerCanPlaceItemHere(location, item, x, y, Game1.player))
            {
                if (item is StardewValley.Objects.Furniture)
                    Game1.player.ActiveObject = (StardewValley.Object)null;
                if (((StardewValley.Object)item).placementAction(location, x, y, Game1.player))
                {
                    Game1.player.reduceActiveItemByOne();
                }
                else
                {
                    if (!(item is StardewValley.Objects.Furniture))
                        return;
                    Game1.player.ActiveObject = (StardewValley.Object)(item as StardewValley.Objects.Furniture);
                }
            }
            else
                Utility.withinRadiusOfPlayer(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y, 3, Game1.player);
        }

        private bool playerCanPlaceItemHere(GameLocation location, Item item, int x, int y, Farmer f)
        {
            if (item == null || item is Tool || (Game1.eventUp || (bool)((NetFieldBase<bool, NetBool>)f.bathingClothes)) || !Utility.withinRadiusOfPlayer(x, y, 1, f) && (!Utility.withinRadiusOfPlayer(x, y, 2, f) || !Game1.isAnyGamePadButtonBeingPressed() || (double)Game1.mouseCursorTransparency != 0.0) && (!(item is Furniture) && !(item is Wallpaper) || !(location is DecoratableLocation)))
                return false;
            Vector2 vector2 = new Vector2((float)(x / 64), (float)(y / 64));
            if (item.canBePlacedHere(location, vector2))
            {
                if (!((StardewValley.Object)item).isPassable())
                {
                    foreach (Character farmer in location.farmers)
                    {
                        if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)vector2.X * 64, (int)vector2.Y * 64, 64, 64)))
                            return false;
                    }
                }
                if (itemCanBePlaced(location, vector2, item) || Utility.isViableSeedSpot(location, vector2, item))
                    return true;
            }
            return false;
        }

        private static bool itemCanBePlaced(GameLocation location, Vector2 tileLocation, Item item)
        {
            if (!location.isTilePlaceable(tileLocation, item) || !item.isPlaceable())
                return false;
            if (!((StardewValley.Object)item).isPassable())
                return !new Microsoft.Xna.Framework.Rectangle((int)((double)tileLocation.X * 64.0), (int)((double)tileLocation.Y * 64.0), 64, 64).Intersects(Game1.player.GetBoundingBox());
            return true;
        }

        private void tryObjectDropIn(ButtonPressedEventArgs e)
        {
            if (!SButtonExtensions.IsActionButton(e.Button))
                return;
            //this.Helper.Input.IsDown(button)
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
            if (Game1.menuUp)
                return;
            if(Game1.player.CurrentItem is Spigot)
            {
                Vector2 vector2 = !Game1.wasMouseVisibleThisFrame ? Game1.player.GetToolLocation(false) : new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y));

                if(((StardewValley.Object)Game1.player.CurrentItem).placementAction(Game1.currentLocation, (int)vector2.X, (int)vector2.Y, Game1.player))
                {
                    Game1.player.reduceActiveItemByOne();
                }
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
                farmer.addItemByMenuIfNecessaryElseHoldUp(new CompostBin(Vector2.Zero));
                farmer.addItemByMenuIfNecessaryElseHoldUp(new Fertilizer("Wood Ash", 12));
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new Spigot(Vector2.Zero));
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new Trowel());
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new Shovel());
                //farmer.addItemByMenuIfNecessaryElseHoldUp(new SeedPacket(Vector2.Zero, "Cauliflower", 28));
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