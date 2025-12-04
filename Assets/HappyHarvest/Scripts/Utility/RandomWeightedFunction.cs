using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

namespace Utility
{
    public class RandomWeightedFunction
    {
        public static Action Pick(List<WeightedFunction> functionList)
        {
            float totalWeight = 0;
            foreach (WeightedFunction wf in functionList)
            {
                totalWeight += wf.weight;
            }

            float randomValue = Random.Range(0f, totalWeight);

            foreach (WeightedFunction wf in functionList)
            {
                if (randomValue < wf.weight)
                {
                    return wf.action;
                }

                randomValue -= wf.weight;
            }

            return null;
        }

        public static void DoNothing(){}
    }

    [System.Serializable]
    public class WeightedFunction
    {
        public float weight;
        public Action action;
    }
}