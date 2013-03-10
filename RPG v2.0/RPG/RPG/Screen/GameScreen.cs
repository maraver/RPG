using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using RPG.Sprite;
using RPG.GameObjects;
using RPG.Helpers;
using RPG.Entities;

namespace RPG.Screen
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameScreen : Screen
    {
        public static int TRANSITION_MS = 500;

        // -------------------
        // Game Variables
        // -------------------
        public Player GamePlayer;
        TileMap gameTileMap;
        int offsetX;

        int transitionMs;

        // -------------------
        // Game Textures
        // -------------------
        Dictionary<EntitySpriteId, EntitySprite> sprEntities;
        Dictionary<AttackSpriteId, Texture2D> sprAttacks;        
        Dictionary<TerrainSpriteId, Texture2D> sprTerrains;
        
        Texture2D sprBackground;

        public GameScreen(ScreenManager screenManager) : base(screenManager) {
            offsetX = 0;
        }

        public override void LoadContent() {
            sprEntities = new Dictionary<EntitySpriteId, EntitySprite>();
            sprEntities.Add(EntitySpriteId.Warrior, new EntitySprite(Content, "Warrior"));
            sprEntities.Add(EntitySpriteId.Warlock, new EntitySprite(Content, "Warlock"));
            sprEntities.Add(EntitySpriteId.Wraith, new EntitySprite(Content, "Wraith"));

            sprAttacks = new Dictionary<AttackSpriteId, Texture2D>();
            sprAttacks.Add(AttackSpriteId.Fireball, Content.Load<Texture2D>("Fireball/Fireball"));
            sprAttacks.Add(AttackSpriteId.Iceball, Content.Load<Texture2D>("Iceball/Iceball"));
            sprAttacks.Add(AttackSpriteId.Scurge_Shot, Content.Load<Texture2D>("Scurge_Shot/Scurge_Shot"));

            sprTerrains = new Dictionary<TerrainSpriteId, Texture2D>();
            sprTerrains.Add(TerrainSpriteId.None, null);
            sprTerrains.Add(TerrainSpriteId.Stone_Wall, Content.Load<Texture2D>("Terrain/stone_wall"));
            sprTerrains.Add(TerrainSpriteId.Door, Content.Load<Texture2D>("Terrain/door"));

            sprBackground = Content.Load<Texture2D>("Terrain/cave1_background");

            GamePlayer = new Player(0, 3 * TileMap.SPRITE_HEIGHT, SprEntity[EntitySpriteId.Warrior]);
            gameTileMap = new TileMap(40, 5, GamePlayer, MapType.Hall, this);
        }

        public override void UnloadContent() {
            
        }

        public override void Update(GameTime gTime) {
            KeyboardState kb = Keyboard.GetState();

            // Don't do anything while transitioning
            if (transitionMs > 0) {
                transitionMs -= gTime.ElapsedGameTime.Milliseconds;
                return;
            }

            // ### Movement input
            if (kb.IsKeyDown(Keys.D) && !kb.IsKeyDown(Keys.A))
                GamePlayer.doMove(Direction.Right);
            else if (!kb.IsKeyDown(Keys.D) && kb.IsKeyDown(Keys.A))
                GamePlayer.doMove(Direction.Left);
            else
                GamePlayer.doMove(Direction.Stopped);

            // ### Jump/Duck input
            if (kb.IsKeyDown(Keys.Space))
                GamePlayer.doJump(gameTileMap);
            else if (!kb.IsKeyDown(Keys.Space) && kb.IsKeyDown(Keys.S) && !kb.IsKeyDown(Keys.W))
                GamePlayer.doDuck(gameTileMap);
            else if (!kb.IsKeyDown(Keys.Space) && !kb.IsKeyDown(Keys.S) && kb.IsKeyDown(Keys.W))
                GamePlayer.doBlock(gameTileMap);
            else if (GamePlayer.State == EntityState.Ducking || GamePlayer.State == EntityState.Blocking)
                GamePlayer.stand();

            if (kb.IsKeyDown(Keys.D1))
                GamePlayer.doAttack(gameTileMap, EntityPart.Head);
            else if (kb.IsKeyDown(Keys.D2))
                GamePlayer.doAttack(gameTileMap, EntityPart.Body);
            else if (kb.IsKeyDown(Keys.D3))
                GamePlayer.doAttack(gameTileMap, EntityPart.Legs);

            // Interact with tile block
            if (kb.IsKeyDown(Keys.Enter)) {
                Rectangle rect = GamePlayer.Rect;
                gameTileMap.getPixel(rect.Center.X, rect.Center.Y).interact(this);
                /*
                if (GamePlayer.isFacingForward())
                    gameTileMap.getPixel(rect.Right - 2, rect.Center.Y).interact(this);
                else
                    gameTileMap.getPixel(rect.Left + 2, rect.Center.Y).interact(this);
                 */
            }

            // ### Update
            foreach (Attack a in Attacks)
                a.update(gameTileMap, gTime.ElapsedGameTime);
            // ## Remove attacks that aren't alive
            Attacks.RemoveAll(new Predicate<Attack>(IsAttackAlive));
            
            foreach (Entity e in Entities)
                e.update(gameTileMap, gTime.ElapsedGameTime);            

            // ### Offset
            offsetX = (int)GamePlayer.Location.X - ((int) getScreenManager().getSize().X / 2);

            if (offsetX < 0) 
                offsetX = 0;
            else if (offsetX + getScreenManager().getSize().X > gameTileMap.getPixelWidth()) 
                offsetX = (int) (gameTileMap.getPixelWidth() - getScreenManager().getSize().X);
        }

        public static bool IsAttackAlive(Attack attack) {
            return !attack.Alive;
        }

        public void newRoom() {
            GamePlayer.newMap();
            gameTileMap = new TileMap(40, 5, GamePlayer, MapType.Hall, this);
            transitionMs = TRANSITION_MS;
        }
        
        public override void Draw(GameTime time) {
            // While transitioning all black
            if (transitionMs > 0) {
                SpriteBatch.GraphicsDevice.Clear(Color.Black);
                return;
            }

            Vector2 background_resize = getScreenManager().getSize();
            int background_offsetX = offsetX % ((int) background_resize.X * 2);

            SpriteBatch.Draw(sprBackground, new Rectangle(-background_offsetX, 0, (int) background_resize.X, (int) background_resize.Y), Color.White);

            // # Draw a second one in front of first to wrap around
            background_offsetX -= (int) background_resize.X;
            SpriteBatch.Draw(sprBackground, new Rectangle(-background_offsetX, 0, (int) background_resize.X, (int) background_resize.Y), Color.White);

            // Draw each from tilemap
            for (int w = 0; w < gameTileMap.Width; w++)
                for (int h = 0; h < gameTileMap.Height; h++) {
                    Texture2D texture = sprTerrains[gameTileMap.getSpriteId(w, h)];
                    if (texture != null)
                        SpriteBatch.Draw(texture, new Vector2(w * TileMap.SPRITE_WIDTH - offsetX, h * TileMap.SPRITE_HEIGHT), Color.White);
                }

            // Draw each attack based on facing direction
            foreach (Attack a in Attacks) {
                Rectangle rect = a.Rectangle;
                rect.X -= offsetX;
                Texture2D sprite = a.getSprite();
                if (sprite != null) {
                    if (a.isFacingForward())
                        SpriteBatch.Draw(a.getSprite(), rect, Color.White);
                    else
                        SpriteBatch.Draw(a.getSprite(), rect, a.getSprite().Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                }
            }

            // Draw player based on facing direction
            foreach (Entity e in Entities)
                e.Draw(SpriteBatch, offsetX, time.ElapsedGameTime);
        }

        public List<Attack> Attacks { get { return gameTileMap.Attacks; } }
        public List<Entity> Entities { get { return gameTileMap.Entities; } }
        public int KilledEntities { get { return gameTileMap.killedEntities; } }

        public Dictionary<EntitySpriteId, EntitySprite> SprEntity { get { return sprEntities; } }
        public Dictionary<AttackSpriteId, Texture2D> SprAttack { get { return sprAttacks; } }
        public Dictionary<TerrainSpriteId, Texture2D> SprTerrains { get { return sprTerrains; } }
    }
}
