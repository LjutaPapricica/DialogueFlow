using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DialogueFlow;

namespace DialogueFlowEditor {
  [CustomEditor(typeof(DialogueFlowAsset))]
  public class DialogueFlowEditor : Editor {
    SerializedProperty nodes;
    SerializedProperty currentNode;

    private void OnEnable() {
      nodes = serializedObject.FindProperty("nodes");
      DialogueFlowWindow.OnRequestRepaint += Repaint;
    }

    private void OnDestroy() {
      nodes = null;
      currentNode = null;
      DialogueFlowWindow.OnRequestRepaint -= Repaint;
    }

    public override void OnInspectorGUI() {
      serializedObject.Update();
      int selectedI = DialogueFlowWindow.activeNodeId;
      if (selectedI >= 0 && selectedI < nodes.arraySize) {
        currentNode = nodes.GetArrayElementAtIndex(selectedI);
      } else {
        currentNode = null;
      }
      if (currentNode != null) {
        var type = (DialogueNode.Type)currentNode.FindPropertyRelative("type").intValue;
        int id = currentNode.FindPropertyRelative("id").intValue;
        EditorGUILayout.LabelField(type + " Node " + id, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        if (type == DialogueNode.Type.Message) {
          EditorGUILayout.PropertyField(currentNode.FindPropertyRelative("name"));
          EditorGUILayout.PropertyField(currentNode.FindPropertyRelative("text"));
        } else if (type == DialogueNode.Type.Choice) {
          var choices = currentNode.FindPropertyRelative("choices");
          choices.arraySize = Mathf.Clamp(EditorGUILayout.IntField("Size", choices.arraySize), 2, 6);
          var outs = currentNode.FindPropertyRelative("connectionsOut");
          if (outs.arraySize != choices.arraySize) {
            int startI = outs.arraySize;
            outs.arraySize = choices.arraySize;
            if (startI < choices.arraySize) {
              for (int i = startI; i < choices.arraySize; i++) {
                outs.GetArrayElementAtIndex(i).intValue = -2;
              }
            }
          }
          for (int i = 0; i < choices.arraySize; i++) {
            EditorGUILayout.PropertyField(choices.GetArrayElementAtIndex(i), new GUIContent("Choice " + i));
          }
        } else if (type == DialogueNode.Type.Event) {
          EditorGUILayout.PropertyField(currentNode.FindPropertyRelative("eventMsg"));
        }
        EditorGUI.indentLevel--;
      }
      serializedObject.ApplyModifiedProperties();
    }
  }
}
