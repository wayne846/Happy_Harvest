using UnityEngine;
namespace HappyHarvest
{
    [CreateAssetMenu(menuName = "2D Farming/Fish")]
    public class Fish : ScriptableObject
    {
        [SerializeField]
        private int id;
        [SerializeField]
        private Product yieldFish;

        public int GetId => id;
    }
}
