using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

namespace BTAction
{
    public class SwitchTree : ActionNode
    {
        [SerializeField] private BehaviourTree tree;
        
        protected override void OnStart() {
            context.gameObject.GetComponent<BehaviourTreeRunner>().SetTree(tree);
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            return State.Success;
        }
    }
}