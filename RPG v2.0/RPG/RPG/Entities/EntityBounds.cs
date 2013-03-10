using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using RPG.Helpers;

namespace RPG.Entities
{
    public enum EntityPart { Head, Body, Legs, None, Miss };

    public struct EntityBounds {
        private static Rectangle ZERO_RECT = new Rectangle(0, 0, 0, 0);

        Rectangle rect, standRect;
        readonly int HEAD_HEIGHT, BODY_HEIGHT, LEGS_HEIGHT;
        Rectangle head, body, legs;
        bool canCollide;

        public EntityBounds(int x, int y, int width, int headHeight, int bodyHeight, int legsHeight, int legsWidth) {
            rect = new Rectangle(x, y, width, headHeight + bodyHeight + legsHeight);

            standRect = new Rectangle(x + (rect.Width / 2) - (legsWidth / 2), y, legsWidth, rect.Height);

            HEAD_HEIGHT = headHeight;
            head = new Rectangle(x, y, width, HEAD_HEIGHT);

            BODY_HEIGHT = bodyHeight;
            body = new Rectangle(x, y + HEAD_HEIGHT, width, BODY_HEIGHT);

            LEGS_HEIGHT = legsHeight;
            legs = new Rectangle(x, y + HEAD_HEIGHT + BODY_HEIGHT, width, LEGS_HEIGHT);

            canCollide = true;
            resetPositions();
        }

        public void resetPositions() {
            head = new Rectangle(rect.X, rect.Y, rect.Width, HEAD_HEIGHT);
            body = new Rectangle(rect.X, rect.Y + HEAD_HEIGHT, rect.Width, BODY_HEIGHT);
            legs = new Rectangle(rect.X, rect.Y + HEAD_HEIGHT + BODY_HEIGHT, rect.Width, BODY_HEIGHT);
        }

        public void die() {
            // No colliding
            canCollide = false;
        }

        public void duck() {
            resetPositions();
            head.Y += 2;
            head.Height -= 2;
        }

        public void block(Direction dir) {
            resetPositions();

            body.Width /= 2;
            if (dir == Direction.Left)
                body.X += body.Width;
        }

        public EntityPart collide(Rectangle otherRect, Point p) {
            if (canCollide && rect.Intersects(otherRect)) {
                if (head.Contains(p))
                    return EntityPart.Head;
                else if (body.Contains(p))
                    return EntityPart.Body;
                else if (legs.Contains(p))
                    return EntityPart.Legs;
                else
                    return EntityPart.Miss;
            }

            return EntityPart.None;
        }

        public int getFireFrom(EntityPart part) {
            if (part == EntityPart.Legs)
                return legs.Top + 3;
            else if (part == EntityPart.Head)
                return head.Bottom - 3;
            else
                return body.Center.Y;
        }

        public Rectangle getRectFromPart(EntityPart part) {
            if (part == EntityPart.Body)
                return body;
            else if (part == EntityPart.Head)
                return head;
            else if (part == EntityPart.Legs)
                return legs;

            return ZERO_RECT;
        }

        public void moveY(int y) {
            rect.Y += y;
            standRect.Y += y;
            head.Y += y;
            body.Y += y;
            legs.Y += y;
        }

        public void moveX(int x) {
            rect.X += x;
            standRect.X += x;
            head.X += x;
            body.X += x;
            legs.X += x;
        }

        public int X { 
            get { return rect.X; }
        }

        public int Y { 
            get { return rect.Y; }
        }

        public int Height { get { return rect.Height; } }
        public int Width { get { return rect.Width; } }

        public Rectangle Rect { get { return rect; } }
        public Rectangle LegsRect { get { return standRect; } }
        public Point Location { get { return rect.Location; } }

        public int Top { get { return rect.Top; } }
        public int Bottom { get { return rect.Bottom; } }
        public int Left { get { return rect.Left; } }
        public int Right { get { return rect.Right; } }
    }
}
