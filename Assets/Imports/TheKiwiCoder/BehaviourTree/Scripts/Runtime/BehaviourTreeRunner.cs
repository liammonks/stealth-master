using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public class BehaviourTreeRunner : MonoBehaviour {

        // The main behaviour tree asset
        public BehaviourTree Tree => m_Tree;
        [SerializeField] private BehaviourTree m_Tree;

        // Storage container object to hold game object subsystems
        Context context;

        // Start is called before the first frame update
        void Start() {
            context = CreateBehaviourTreeContext();
            SetTree(m_Tree);
        }
        
        public void SetTree(BehaviourTree tree)
        {
            m_Tree = tree.Clone();
            m_Tree.Bind(context);
        }

        // Update is called once per frame
        void Update() {
            if (Tree) {
                Tree.Update();
            }
        }

        Context CreateBehaviourTreeContext() {
            return Context.CreateFromGameObject(gameObject);
        }

        private void OnDrawGizmosSelected() {
            if (!Tree) {
                return;
            }

            BehaviourTree.Traverse(Tree.rootNode, (n) => {
                if (n.drawGizmos) {
                    n.OnDrawGizmos();
                }
            });
        }
    }
}