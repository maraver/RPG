using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RPG.Sprite
{
    public class EntitySprite : Sprite
    {
        public readonly Texture2D Move, Duck, Block, Dead, Attack, Duck_Attack;

        public EntitySprite(ContentManager content, String baseName) : base(content, baseName)
        {
            Move = content.Load<Texture2D>(baseName + "/" + baseName + "_Move");
            Duck = content.Load<Texture2D>(baseName + "/" + baseName + "_Crouch");
            Block = content.Load<Texture2D>(baseName + "/" + baseName + "_Block");
            Attack = content.Load<Texture2D>(baseName + "/" + baseName + "_Attack");
            Duck_Attack = content.Load<Texture2D>(baseName + "/" + baseName + "_Crouch-Attack");

            // Use default, same for almost all sprite
            Dead = content.Load<Texture2D>("Default_Dead");
        }
    }
}
