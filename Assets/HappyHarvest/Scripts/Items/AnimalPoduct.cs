
using UnityEngine;

namespace HappyHarvest
{
    [CreateAssetMenu(menuName = "2D Farming/Items/ AnimalPoduct")]
    public class AnimalPoduct : Item
    {
        public int SellPrice = 1;

        public override bool CanUse(Vector3Int target)
        {
            return false;
        }

        public override bool Use(Vector3Int target)
        {
            return false;
        }

        public override bool NeedTarget()
        {
            return false;
        }
    }
}
