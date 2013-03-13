using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using RPG.Helpers;

namespace RPG.Entities
{
    public enum EntityPart { Head, Body, Legs, None, Miss };
    public struct EntityHit {
        public static EntityHit NONE = new EntityHit(EntityPart.None, 0);
        public static EntityHit MISS = new EntityHit(EntityPart.Miss, 0);

        public EntityPart Part;
        public float PercFromCenter;
        public EntityHit(EntityPart part, float percFromCenter) { 
            Part = part; PercFromCenter = percFromCenter;
        }
    }

    public struct EntityBounds {
        private static Rectangle ZERO_RECT = new Rectangle(0, 0, 0, 0);

        Entity entity;
        Rectangle rect, standRect;
        readonly int HEAD_HEIGHT, BODY_HEIGHT, LEGS_HEIGHT;
        Rectangle head, body, legs;
        bool canCollide;

        public EntityBounds(Entity e, int x, int y, int width, int headHeight, int bodyHeight, int legsHeight, int legsWidth) {
            entity = e;
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
            entity.Stats.resetReducers();
        }

        public void die() {
            // No colliding
            canCollide = false;
        }

        public void duck() {
            resetPositions();

            entity.Stats.legsMultiplier += 0.2f;
            head.Y += 4;
            body.Y += 4;
            body.Height -= 3;
            legs.Height -= 1;
        }

        public void block(Direction dir) {
            resetPositions();

            entity.Stats.headMultiplier += 0.15f;
            entity.Stats.bodyMultiplier += 0.5f;
            entity.Stats.legsMultiplier += 0.15f;
            body.Width /= 2;
            if (dir == Direction.Left)
                body.X += body.Width;
        }

        public EntityHit collide(Rectangle otherRect, Point p) {
            if (canCollide && rect.Intersects(otherRect)) {
                if (head.Contains(p)) {
                     return new EntityHit(EntityPart.Head, Math.Abs(head.Center.Y - p.Y) / (float) head.Height);
                } else if (body.Contains(p)) {
                    return new EntityHit(EntityPart.Body, Math.Abs(body.Center.Y - p.Y) / (float) body.Height);
                } else if (legs.Contains(p)) {
                    return new EntityHit(EntityPart.Legs, Math.Abs(legs.Top - p.Y) / (float) legs.Height);
                } else {
                    return EntityHit.MISS;
                }
            }

            return EntityHit.NONE;
        }

        public int getFireFrom(EntityPart part) {
            if (part == EntityPart.Legs)
                return legs.Top + 1;
            else if (part == EntityPart.Head)
                return head.Center.Y;
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
        public Rectangle LegsRect { get { return legs; } }
        public Rectangle BodyRect { get { return body; } }
        public Rectangle HeadRect { get { return head; } }
        public Rectangle StandRect { get { return standRect; } }
        public Point Location { get { return rect.Location; } }

        public int Top { get { return rect.Top; } }
        public int Bottom { get { return rect.Bottom; } }
        public int Left { get { return rect.Left; } }
        public int Right { get { return rect.Right; } }
    }
}
