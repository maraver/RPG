using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG.GameObjects
{
    public class EntityStats
    {
        int level;
        int hp, maxHp;
        float attackPower;

        public EntityStats(int hp, float atkPower) {
            level = 1;
            this.hp = maxHp = hp;
            attackPower = atkPower;
        }

        public void levelUp() {
            level++;
            float hpPercent = hp / (float) maxHp;
            maxHp = (int) (maxHp * 1.1f);
            hp = (int) (maxHp * hpPercent);

            attackPower = (int) (attackPower * 1.1f);
        }

        public void hurt(int amount) {
            hp -= amount;
        }

        public float HpPercent { get { return (hp / (float) maxHp); } }
        public int Hp { get { return hp; } }
        public float AttackPower { get { return attackPower; } }
        public int Level { get { return level; } }
    }
}
