using System.Collections;

namespace Interfaces.AI.Combat
{
    public interface IDamageOverTime
    {
        public IEnumerator StartDamageOverTime();
    }
}