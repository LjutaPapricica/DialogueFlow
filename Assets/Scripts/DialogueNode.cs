using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DialogueFlow {
  [System.Serializable]
  public class DialogueNode {
    [HideInInspector]
    public Rect window = new Rect(0, 0, 150, 20);
    [HideInInspector]
    public int id;
    public List<int> connectionsIn = new List<int>();
    public int[] connectionsOut = new int[1] { -2 };
    [HideInInspector]
    public bool isEmpty;
    [HideInInspector]
    public Type type;

    public string name;
    [Multiline(4)]
    public string text;

    public List<string> choices;

    public string eventMsg;

    public enum Type {
      Start, Message, Choice, Event
    }

    public DialogueNode(Type type) {
      this.type = type;
    }

    public DialogueNode(DialogueNode source) {
      type = source.type;
      connectionsOut = new int[source.connectionsOut.Length];
      for (int i = 0; i < connectionsOut.Length; i++) {
        connectionsOut[i] = -2;
      }
      window = new Rect(source.window);
      name = source.name;
      text = source.text;
      choices = new List<string>();
      for (int i = 0; i < source.choices.Count; i++) {
        choices.Add(source.choices[i]);
      }
    }
  }
}