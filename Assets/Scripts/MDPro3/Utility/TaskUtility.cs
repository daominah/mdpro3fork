using System;
using System.Threading.Tasks;
using UnityEngine;


namespace MDPro3
{
    public static class TaskUtility
    {
        private static readonly int DeltaTime;

        static TaskUtility()
        {
            DeltaTime = 1000 / Application.targetFrameRate;
        }

        public static async Task WaitWhile(Func<bool> condition)
        {
            while(condition() && Application.isPlaying)
            {
                await Task.Delay(DeltaTime);
            }
        }

        public static async Task WaitUntil(Func<bool> condition)
        {
            while (!condition() && Application.isPlaying)
            {
                await Task.Delay(DeltaTime);
            }
        }
    }
}
