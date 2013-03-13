using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG.Entities
{
    public struct EntityStats
    {
        int level;
        int hp, maxHp;
        float attackPower;

        public float headMultiplier, bodyMultiplier, legsMultiplier;
        float headArmour, bodyArmour, legsArmour;

        public EntityStats(int hp, float atkPower) {
            level = 1;
            this.hp = maxHp = hp;
            attackPower = atkPower;

            headArmour = bodyArmour = legsArmour = 0f;
            headMultiplier = bodyMultiplier = legsMultiplier = 1f;
        }

        public void resetReducers() {
            headMultiplier = 1 - headArmour;
            bodyMultiplier = 1 - bodyArmour;
            legsMultiplier = 1 - legsArmour;
        }

        public void levelUp() {
            level++;
            float hpPercent = hp / (float) maxHp;
            maxHp = (int) (maxHp * 1.1f);
            hp = (int) (maxHp * hpPercent);
            addHp((int) (maxHp * 0.1f)); // Heal by 10% hp

            attackPower = attackPower * 1.1f;
        }

        public void addHp(int amount) {
            hp += amount;
            if (hp > maxHp)
                hp = maxHp;
        }

        public float HpPercent { get { return (hp / (float) maxHp); } }
        public int Hp { get { return hp; } }
        public int MaxHp { get { return maxHp; } }
        public float AttackPower { get { return attackPower; } }
        public int Level { get { return level; } }

        public float THeadMultiplier { get { return Entity.BASE_HEAD_MULT * headMultiplier; } }
        public float TBodyMultiplier { get { return bodyMultiplier; } }
        public float TLegsMultiplier { get { return Entity.BASE_LEGS_MULT * legsMultiplier; } }
    }
}
