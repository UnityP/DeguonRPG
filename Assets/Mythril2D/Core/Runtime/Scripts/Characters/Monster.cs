using System;
using System.Collections.Generic;

namespace Gyvr.Mythril2D
{
    public class Monster : Character<MonsterSheet>
    {
        protected override void Awake()
        {
            base.Awake();
            UpdateStats();
        }

        public void SetLevel(int level)
        {
            m_level = level;
            UpdateStats();
        }

        public void UpdateStats()
        {
            m_stats.Set(m_sheet.stats[m_level]);
        }

        protected override void Die()
        {
            base.Die();
            GameManager.NotificationSystem.monsterKilled.Invoke(m_sheet);

            foreach (Loot loot in m_sheet.potentialLoot)
            {
                if (GameManager.Player.level >= loot.minimumPlayerLevel && m_level >= loot.minimumMonsterLevel && loot.ResolveDrop())
                {
                    GameManager.InventorySystem.AddToBag(loot.item, loot.quantity);
                }
            }

            GameManager.Player.AddExperience(m_sheet.experience[m_level]);
            GameManager.InventorySystem.AddMoney(m_sheet.money[m_level]);

            if (m_sheet.executeOnDeath != null)
            {
                foreach (ActionHandler actionHandler in m_sheet.executeOnDeath)
                {
                    actionHandler.Execute();
                }
            }
        }
    }
}
