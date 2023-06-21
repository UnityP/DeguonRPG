using UnityEngine.Events;

namespace Gyvr.Mythril2D
{
    public class ObservableStats
    {
        public Stats stats => m_stats;
        public UnityEvent changed => m_changed;

        private Stats m_stats;
        private UnityEvent m_changed = new UnityEvent();

        public ObservableStats() : this(new Stats())
        {
        }

        public ObservableStats(Stats stats)
        {
            m_stats = stats;
        }

        public int this[EStat stat]
        {
            get => m_stats[stat];
            set
            {
                m_stats[stat] = value;
                m_changed.Invoke();
            }
        }

        public void Set(Stats stats)
        {
            m_stats = new Stats(stats);
            m_changed.Invoke();
        }
    }
}
