using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilsShared
{
    public class IntervalAction
    {
        private readonly float intervalSeconds;
        private readonly Action action;
        private float elapsedTime = 0f;

        public IntervalAction(float intervalSeconds, Action action)
        {
            this.intervalSeconds = intervalSeconds;
            this.action = action;
        }

        public void Update(float deltaTime)
        {
            elapsedTime += deltaTime;
            if (elapsedTime >= intervalSeconds)
            {
                action();
                elapsedTime = 0f;
            }
        }

        public void Reset()
        {
            elapsedTime = 0f;
        }
    }
}