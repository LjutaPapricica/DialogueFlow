using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueFlow {
  [CreateAssetMenu(fileName = "DialogueFlow", menuName = "Dialogue Flow / Dialogue Flow Asset")]
  public class DialogueFlowAsset : ScriptableObject {
    [HideInInspector]
    public DialogueNode start = new DialogueNode(DialogueNode.Type.Start) {
      id = -1
    };
    public List<DialogueNode> nodes = new List<DialogueNode>();
    private List<int> openIndexes = new List<int>();

    public DialogueNode Get(int id) {
      if (id == -1) {
        return start;
      } else if (id >= 0 && id < nodes.Count) {
        return nodes[id];
      }
      return null;
    }

    public void Clear() {
      nodes = new List<DialogueNode>();
      start = new DialogueNode(DialogueNode.Type.Start) {
        id = -1
      };
      openIndexes = new List<int>();
    }

    public void Add(DialogueNode node) {
      if (openIndexes.Count > 0) {
        int i = openIndexes[0];
        openIndexes.RemoveAt(0);
        node.id = i;
        nodes[i] = node;
      } else {
        node.id = nodes.Count;
        nodes.Add(node);
      }
    }

    public void Remove(int id) {
      var node = Get(id);
      if (id < 0 || node == null) {
        return;
      }
      nodes[id].isEmpty = true;
      for (int i = 0; i < nodes[id].connectionsOut.Length; i++) {
        var inNode = Get(nodes[id].connectionsOut[i]);
        if (inNode != null) inNode.connectionsIn.Remove(id);
      }
      for (int i = 0; i < nodes[id].connectionsIn.Count; i++) {
        var outNode = Get(nodes[id].connectionsIn[i]);
        for (int j = 0; j < outNode.connectionsOut.Length; j++) {
          if (outNode.connectionsOut[j] == id) {
            outNode.connectionsOut[j] = -2;
          }
        }
      }
      openIndexes.Add(id);
      if (id == nodes.Count - 1) {
        Clean();
      }
    }

    private void Clean() {
      int i = nodes.Count - 1;
      while (i >= 0 && nodes[i].isEmpty) {
        nodes.RemoveAt(i);
        openIndexes.Remove(i);
        i--;
      }
    }

    public void Connect(int outId, int inId, int outSlot) {
      var nodeOut = Get(outId);
      if (nodeOut.connectionsOut.Length < outSlot) {
        return;
      }
      var oldIn = Get(nodeOut.connectionsOut[outSlot]);
      if (oldIn != null) {
        oldIn.connectionsIn.Remove(outId);
      }
      nodeOut.connectionsOut[outSlot] = inId;
      var nodeIn = Get(inId);
      if (nodeIn != null) {
        if (!nodeIn.connectionsIn.Contains(outId)) {
          nodeIn.connectionsIn.Add(outId);
        }
      }
    }
  }
}