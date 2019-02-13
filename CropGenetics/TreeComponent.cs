using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using _SyrupFramework;

namespace Perennials
{
    public class TreeComponent
    {
        public const int AlwaysRender = 0;
        public const int FadeInNear = 1;
        public const int FadeWhenSouth = 2;

        public const int Stump = 0;
        public const int Trunk = 1;
        public const int Limb = 2;
        public const int Branch = 3;


        //Individual-specific
        public int flag;
        public int type;
        private List<TreeComponent> children;
        public Vector2 offset;
        public float rotation;
        public Texture2D spriteSheet;
        public Rectangle sprite;

        public TreeComponent(Texture2D sheet, Rectangle spriteRect, int type, int renderFlag = AlwaysRender)
        {
            children = new List<TreeComponent>();
            spriteSheet = sheet;
            sprite = spriteRect;
            offset = Vector2.Zero;
            rotation = 0f;
            flag = renderFlag;
            this.type = type;
        }

        public TreeComponent(Texture2D sheet, Rectangle spriteRect, int x, int y, int type, int renderFlag = AlwaysRender) : this(sheet, spriteRect, type, renderFlag)
        {
            offset = new Vector2(x, y);
        }

        public TreeComponent(Texture2D sheet, Rectangle spriteRect, Vector2 location, int type, int renderFlag = AlwaysRender) : this(sheet, spriteRect, type, renderFlag)
        {
            offset = location;
        }

        public void rotateConstrainedDegrees(float origin, float rot, float strength)
        {
            this.rotateConstrained(degreesToRadians(origin), degreesToRadians(rot), strength);
        }

        public void rotateConstrained(float origin, float rot, float strength)
        {
            string breakdown = "Constrained rotation breakdown:\nInitial Rotation: " + rotation + "\nRotated By: " + rot;
            float newRot = rotation + rot;
            breakdown += "\nNew Rotation: " + newRot;
            //Example: distanceFromOrigin = 45 - 0 = 45;
            //Example: distanceFromOrigin = -90 - 0 = -90;
            //Example: distanceFromOrigin = 0 - 90 = -90;
            //Example: distanceFromOrigin = (-25 + 45) - 0 = 20;
            //Example: distanceFromOrigin = (-15 + 0) - 65 = -80;
            breakdown += "\nConstrained To: " + origin;
            float distanceFromOrigin = newRot - origin;
            breakdown += "\nDistance From Constraint: " + distanceFromOrigin;
            //45 - (45 * 0.5) = 22.5;
            //-90 - (-90 * 0.5) = -45;
            //0 - (-90 * 0.5) = -45;
            //20 - (20 * 0.5) = 10;
            //-15 - (-65 * 0.5) = 17.5;
            newRot -= (distanceFromOrigin * strength);
            rotation = newRot;
            breakdown += "\nModified rotation: " + newRot + "\nRotation Final: " + rotation;
            //Logger.Log(breakdown);
        }

        public void rotate(float rot)
        {
            rotation += rot;
        }

        public bool addChild(TreeComponent child)
        {
            //We don't want the possibility of a loop, where either the component is set as its own child, or is made the parent of a brach of which it is a child.
            if (child == this || child.isWithinChildren(this))
                return false;
            child.rotation = rotation;
            children.Add(child);
            return true;
        }

        //public TreeComponent getFinalChildOfType(int componentType)
        //{
        //    //Returns the final child of that type, in a lineage of children of that type.
        //    //Currently always seeks the topmost branch, so in a tree with two trunks, it will always choose whichever trunk is first in the split, regardless of length.
        //    foreach(TreeComponent child in children)
        //    {
        //        if(child.type == componentType)
        //        {
        //            return child.getFinalChildOfType(componentType);
        //        }
        //    }
        //    //If there is no child of the searched type, this is the final child of that type.
        //    return this;
        //}

        public List<TreeComponent> getAllFinalChildrenOfType(int componentType)
        {
            //This returns every instance of TreeComponent whose type matches the argument, and which does not have a child that does so.
            List<TreeComponent> finalChildren = new List<TreeComponent>();
            List<TreeComponent> allChildren = getAllChildren();
            Logger.Log("getAllChildren() returned " + allChildren.Count + " children.");
            foreach(TreeComponent child in allChildren)
            {
                if(child.type == componentType && !child.hasChildOfType(componentType))
                {
                    Logger.Log("Found a terminating child (" + allChildren.IndexOf(child) + ") of searched type.");
                    finalChildren.Add(child);
                }
                else
                {
                    Logger.Log("Child (" + allChildren.IndexOf(child) + ") did not meet criteria: type was " + child.type + ", needed " + componentType + ";" + (!child.hasChildOfType(componentType) ? " had no children of type." : " had children of the type."));
                    //Logger.Log("Child did not fit criteria: " + (child.type == componentType ? " met required type, " : " did not meet required type, ") + (!child.hasChildOfType(componentType) ? " had no children of type." : " had children of the type."));
                }
            }
            return finalChildren;
        }

        public bool hasChildOfType(int componentType)
        {
            foreach(TreeComponent child in children)
            {
                if (child.type == componentType)
                    return true;
            }
            return false;
        }

        public List<TreeComponent> getAllChildren()
        {
            List<TreeComponent> totalchildren = new List<TreeComponent>();
            foreach (TreeComponent child in children)
            {
                totalchildren.Add(child);
                foreach(TreeComponent grandchild in child.getAllChildren())
                {
                    totalchildren.Add(grandchild);
                }
            }
            return totalchildren;
        }

        public bool thisBranchTerminatesHere()
        {
            return children.Count == 0;
        }

        public bool isWithinChildren(TreeComponent child)
        {
            //If this component already has this component in its children
            if (children.Contains(child))
                return true;
            //Iterate through all children to recursively see if the component is within the tree
            foreach (TreeComponent myChild in children)
            {
                if (myChild.isWithinChildren(child))
                    return true;
            }
            return false;
        }

        public int getHeight()
        {
            return getHeight(1);
        }

        private int getHeight(int height)
        {
            int internalHeight = height;
            foreach(TreeComponent child in children)
            {
                internalHeight = Math.Max(child.getHeight(height + 1), internalHeight);
            }
            return internalHeight;
        }

        public TreeComponent getFinalChildOfType(int type)
        {
            List<TreeComponent> ends = getAllFinalChildrenOfType(type);
            if (ends.Count == 0)
                return this;
            if (ends.Count == 1)
                return ends[0];

            int index = -1;
            int highestHeight = -1;

            foreach (TreeComponent child in ends)
            {
                int childHeight = getHeightOfComponent(child);
                if(childHeight > highestHeight)
                {
                    index = ends.IndexOf(child);
                    highestHeight = childHeight;
                }
            }
            return ends[index];
        }

        private int internalHeightOfComponent(TreeComponent component)
        {
            int currentHeight = 0;
            if (children.Contains(component))
                return 1;
            foreach(TreeComponent child in children)
            {
                currentHeight = Math.Max(currentHeight, child.internalHeightOfComponent(component));
            }
            return currentHeight != 0 ? currentHeight + 1 : currentHeight;
        }

        public int getHeightOfComponent(TreeComponent component)
        {
            return internalHeightOfComponent(component);
        }

        public int getGravityDirection()
        {
            //pi rad = 180°
            //Returns -1 if the angle is from 0-180, and 1 from 180-360
            return getGravity(rotation);
        }

        public static int getGravity(float radians)
        {
            return radians % (Math.PI * 2) >= Math.PI ? -1 : 1;
        }

        public void draw(SpriteBatch b, Vector2 parentLocation, Color toTint, float rotationAddition, bool fullRenderFlip, float treeDepth)
        {
            //Offsets are in pixels, not tiles, so they are applied directly.
            Vector2 local = Game1.GlobalToLocal(
                Game1.viewport,
                new Vector2(parentLocation.X + offset.X, parentLocation.Y + offset.Y)
            );

            b.Draw(spriteSheet,
                local,
                sprite,
                toTint,
                rotation + rotationAddition,
                new Vector2(sprite.Width / 2, sprite.Height),
                (float)Game1.pixelZoom,
                fullRenderFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                treeDepth
            );

            foreach(TreeComponent child in children)
            {
                child.draw(b, parentLocation + Vector2.Transform(child.offset, Matrix.CreateRotationZ(rotation + rotationAddition)), toTint, rotationAddition, fullRenderFlip, treeDepth + (5/10000f) + (children.IndexOf(child) * 11 / 10000f));
            }
        }

        public float degreesToRadians(float degrees)
        {
            return (float)(degrees / (180 / Math.PI));
        }

        public float radiansToDegrees(float radians)
        {
            return (float)(radians * (180 / Math.PI));
        }
    }
}