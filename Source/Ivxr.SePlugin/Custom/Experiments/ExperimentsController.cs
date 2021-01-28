using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.Components;

namespace Iv4xr.SePlugin.Custom.Experiments
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class ExperimentsController : MySessionComponentBase
    {
        public static ExperimentsController Instance;
        private List<IEnumerator> coroutines = new List<IEnumerator>();

        public override void UpdateBeforeSimulation()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        protected override void UnloadData()
        {
            Instance = null;
        }

        public void StartCoroutine()
        {
            foreach (var coroutine in coroutines.ToList())
            {
                var shouldContinue = coroutine.MoveNext();
                if (!shouldContinue)
                {
                    coroutines.Remove(coroutine);
                }
            }
        }
    }
}