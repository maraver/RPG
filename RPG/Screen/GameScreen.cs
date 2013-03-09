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

namespace RPG.Screen
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameScreen : Screen
    {
        // -------------------
        // Game Variables
        // -------------------
        TileMap gameTileMap;
        int offsetX;

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

            sprAttacks = new Dictionary<AttackSpriteId, Texture2D>();
            sprAttacks.Add(AttackSpriteId.FireBall, Content.Load<Texture2D>("Fireball/Fireball"));

            sprTerrains = new Dictionary<TerrainSpriteId, Texture2D>();
            sprTerrains.Add(TerrainSpriteId.None, null);
            sprTerrains.Add(TerrainSpriteId.Stone_Wall, Content.Load<Texture2D>("Terrain/stone_wall"));

            sprBackground = Content.Load<Texture2D>("Terrain/cave1_background");

            gameTileMap = new TileMap(40, 5, MapType.Hall, this);
        }

        public override void UnloadContent() {
            
        }

        public override void Update(GameTime gTime) {
            KeyboardState kb = Keyboard.GetState();

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
            else if (GamePlayer.getState() == EntityState.Ducking || GamePlayer.getState() == EntityState.Blocking)
                GamePlayer.stand();

            if (kb.IsKeyDown(Keys.D1))
                GamePlayer.doAttack(gameTileMap, EntityPart.Head);
            else if (kb.IsKeyDown(Keys.D2))
                GamePlayer.doAttack(gameTileMap, EntityPart.Body);
            else if (kb.IsKeyDown(Keys.D3))
                GamePlayer.doAttack(gameTileMap, EntityPart.Legs);

            // ### Update
            foreach (Attack a in Attacks)
                a.update(gameTileMap, gTime.ElapsedGameTime);
            // ## Remove attacks that aren't alive
            Attacks.RemoveAll(new Predicate<Attack>(IsAttackAlive));
            
            foreach (Entity e in Entities)
                e.update(gameTileMap, gTime.ElapsedGameTime);            

            // ### Offset
            offsetX = (int)GamePlayer.getLocation().X - ((int) getScreenManager().getSize().X / 2);

            if (offsetX < 0) 
                offsetX = 0;
            else if (offsetX + getScreenManager().getSize().X > gameTileMap.getPixelWidth()) 
                offsetX = (int) (gameTileMap.getPixelWidth() - getScreenManager().getSize().X);
        }

        public static bool IsAttackAlive(Attack attack) {
            return !attack.Alive;
        }

        private void newRoom() {
            GamePlayer.newMap();
            gameTileMap = new TileMap(40, 5, MapType.Hall, this);
        }
        
        public override void Draw(GameTime time) {
            Vector2 background_resize = getScreenManager().getSize();
            int background_offsetX = offsetX % ((int) background_resize.X * 2);

            SpriteBatch.Draw(sprBackground, new Rectangle(-background_offsetX, 0, (int) background_resize.X, (int) background_resize.Y), Color.White);

            // # Draw a second one in front of first to wrap around
            background_offsetX -= (int) background_resize.X;
            SpriteBatch.Draw(sprBackground, new Rectangle(-background_offsetX, 0, (int) background_resize.X, (int) background_resize.Y), Color.White);

            // Draw each from tilemap
            for (int w = 0; w < gameTileMap.getWidth(); w++)
                for (int h = 0; h < gameTileMap.getHeight(); h++) {
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

        public Player GamePlayer { get { return gameTileMap.getPlayer(); } }
        public List<Attack> Attacks { get { return gameTileMap.getAttacks(); } }
        public List<Entity> Entities { get { return gameTileMap.getEntities(); } }

        public Dictionary<EntitySpriteId, EntitySprite> SprEntity { get { return sprEntities; } }
        public Dictionary<AttackSpriteId, Texture2D> SprAttack { get { return sprAttacks; } }
        public Dictionary<TerrainSpriteId, Texture2D> SprTerrains { get { return sprTerrains; } }
    }
}
