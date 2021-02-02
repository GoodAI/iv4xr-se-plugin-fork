using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Iv4xr.SePlugin.Custom.Experiments;
using VRage.Game.Components;

namespace Iv4xr.SePlugin.Custom
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CoroutineManager : MySessionComponentBase
    {
        public static CoroutineManager Instance;
        private List<Coroutine> coroutines = new List<Coroutine>();

        public override void UpdateBeforeSimulation()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            foreach (var coroutine in coroutines.ToList())
            {
                var enumerator = coroutine.Enumerators.Peek();
                var shouldContinue = enumerator.MoveNext();

                if (shouldContinue)
                {
                    var value = enumerator.Current;

                    if (value is IEnumerator subEnumerator)
                    {
                        coroutine.Enumerators.Push(subEnumerator);
                    }
                }
                else
                {
                    coroutine.Enumerators.Pop();
                }

                if (coroutine.Enumerators.Count == 0)
                {
                    coroutines.Remove(coroutine);
                }
            }
        }

        protected override void UnloadData()
        {
            Instance = null;
        }

        public void StartCoroutine(IEnumerator coroutine)
        {
            coroutines.Add(new Coroutine()
            {
                Enumerators = new Stack<IEnumerator>(new List<IEnumerator>() { coroutine }),
            });
        }

        private class Coroutine
        {
            public Stack<IEnumerator> Enumerators { get; set; }
        }
    }
}