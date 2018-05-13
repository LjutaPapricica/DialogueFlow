using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueFlow;

namespace DialogueFlow.Example {
  public class DialogueEmitter : MonoBehaviour {
    public DialogueFlowAsset flow;

    public void Run() {
      DialogueController.current.SetFlow(flow, gameObject);
    }

    void OnFlowEnded(DialogueFlowAsset flow) {
      Debug.Log("Flow " + flow.name + " ended");
    }

    void OnFlowChange(int index) {
      Debug.Log("Current flow index " + index);
    }

    void OnFlowEvent(string msg) {
      Debug.Log("Flow send the message: " + msg);
    }
  }
}