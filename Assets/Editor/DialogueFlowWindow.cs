using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using DialogueFlow;

namespace DialogueFlowEditor {
  public class DialogueFlowWindow : EditorWindow {

    private DialogueFlowAsset selected;

    private Mode viewMode {
      get {
        int i = EditorPrefs.GetInt("DialogueFlowMode", 0);
        return (Mode)i;
      }
      set {
        EditorPrefs.SetInt("DialogueFlowMode", (int)value);
      }
    }
    private enum Mode {
      Small, Medium//, Full
    }

    private int overNodeId = -2;
    static public int activeNodeId {
      get; private set;
    }
    private int playtestFlowPosition = -1;
    private int draggingNodeId = -2;

    private int connectionOutId = -2;
    private int connectionOutSlot = 0;
    private Vector2 connectionOutPos;

    public static System.Action OnRequestRepaint = () => { };
    private int requestContext = 0;

    private Vector2 offset;
    private Vector2 drag;

    private bool isSelecting = false;
    private Vector2 selectionDragStart;
    private Vector2 selectionDragCurrent;
    private HashSet<DialogueNode> selection = new HashSet<DialogueNode>();
    private List<DialogueNode> nodeCopies = new List<DialogueNode>();

    private GUIStyle styleSelection;
    private GUIStyle styleConnectionUsed;
    private GUIStyle styleConnectionFree;
    private GUIStyle styleConnectionActive;
    private GUIStyle styleConnectionCurrentUsed;
    private GUIStyle styleConnectionCurrentFree;
    private GUIStyle styleNode;
    private GUIStyle styleNodeActive;
    private GUIStyle styleNodeCurrent;
    private bool stylesAreSet = false;

    private Color gridBgColor = new Color(97.0f / 255.0f, 97.0f / 255.0f, 97.0f / 255.0f);

    private Rect toolBarRect = new Rect(0, 0, 0, 0);
    private Rect nodeFieldRect = new Rect(0, 0, 0, 0);

    private GUIContent contentClear = new GUIContent("Clear", "Deletes all nodes");
    private GUIContent contentMode = new GUIContent("View Mode");

    [MenuItem("Window/Dialogue Flow")]
    static void Init() {
      EditorWindow.GetWindow<DialogueFlowWindow>("Dialogue Flow", true);
    }

    private void Awake() {
      OnSelectionChange();
    }

    private void SetupStyles() {
      styleSelection = new GUIStyle("SelectionRect");
      styleConnectionUsed = new GUIStyle();
      styleConnectionUsed.normal.background = Resources.Load<Texture2D>("connectionUsed");
      styleConnectionUsed.hover.background = Resources.Load<Texture2D>("connectionHover");
      styleConnectionFree = new GUIStyle();
      styleConnectionFree.normal.background = Resources.Load<Texture2D>("connectionFree");
      styleConnectionFree.hover.background = Resources.Load<Texture2D>("connectionHover");
      styleConnectionActive = new GUIStyle();
      styleConnectionActive.normal.background = Resources.Load<Texture2D>("connectionHover");
      styleConnectionCurrentUsed = new GUIStyle();
      styleConnectionCurrentUsed.normal.background = Resources.Load<Texture2D>("connectionCurrentUsed");
      styleConnectionCurrentUsed.hover.background = Resources.Load<Texture2D>("connectionHover");
      styleConnectionCurrentFree = new GUIStyle();
      styleConnectionCurrentFree.normal.background = Resources.Load<Texture2D>("connectionCurrentFree");
      styleConnectionCurrentFree.hover.background = Resources.Load<Texture2D>("connectionHover");
      styleNode = new GUIStyle();
      styleNode.normal.background = Resources.Load<Texture2D>("node");
      styleNode.border = styleNode.margin = new RectOffset(8, 8, 8, 12);
      styleNodeActive = new GUIStyle(styleNode);
      styleNodeActive.normal.background = Resources.Load<Texture2D>("nodeActive");
      styleNodeCurrent = new GUIStyle(styleNode);
      styleNodeCurrent.normal.background = Resources.Load<Texture2D>("nodeCurrent");
    }

    private void OnSelectionChange() {
      if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(DialogueFlowAsset)) {
        Select(Selection.activeObject);
      }
    }

    private void Select(Object obj) {
      if (selected != obj) {
        selected = (DialogueFlowAsset)obj;
        Repaint();
      }
    }

    private void Update() {
      if (EditorApplication.isPlaying && DialogueController.current != null) {
        if (playtestFlowPosition != DialogueController.current.flowIndex) {
          playtestFlowPosition = DialogueController.current.flowIndex;
          Repaint();
        }
      } else if (playtestFlowPosition != -1) {
        playtestFlowPosition = -1;
        Repaint();
      }
    }

    private void OnGUI() {
      if (!stylesAreSet) {
        SetupStyles();
        stylesAreSet = true;
      }
      overNodeId = -2;
      wantsMouseMove = true;
      DrawToolbar();
      Color prevColor = GUI.color;
      GUI.color = Color.white;
      DrawNodeField();
      GUI.color = prevColor;
      if (selected != null) {
        ProcessEvents();
      }
    }

    private void ProcessEvents() {
      Event e = Event.current;
      bool isOverNodeField = nodeFieldRect.Contains(e.mousePosition);
      drag = Vector2.zero;
      if (requestContext > 0) {
        switch (requestContext) {
          case 1:
            ShowSelectionContext();
            break;
          case 2:
            Vector2 pos = e.mousePosition;
            pos.x -= nodeFieldRect.x;
            pos.y -= nodeFieldRect.y;
            ShowAddContext(pos);
            break;
          case 3:
            ShowNodeContext();
            break;
        }
        requestContext = 0;
        return;
      }
      switch (e.type) {
        case EventType.MouseDown:
          if (connectionOutId > -2) {
            if (selected != null) {
              selected.Connect(connectionOutId, -2, connectionOutSlot);
            }
            connectionOutId = -2;
            e.Use();
          }
          if (isOverNodeField) {
            if (e.button == 0) { // Left click
              if (overNodeId > -2) {
                SetActive(overNodeId);
                draggingNodeId = overNodeId;
                var node = selected.Get(overNodeId);
                if (!selection.Contains(node)) {
                  isSelecting = false;
                  selection.Clear();
                }
              } else {
                if (activeNodeId != -2) {
                  SetActive(-2);
                }
                isSelecting = true;
                selectionDragStart = selectionDragCurrent = e.mousePosition;
                selection.Clear();
              }
              e.Use();
            } else if (e.button == 1) { // right click
              if (overNodeId >= 0 && selection.Count > 0) {
                requestContext = 1;
              } else if (overNodeId == -2) {
                isSelecting = false;
                selection.Clear();
                requestContext = 2;
              } else if (overNodeId >= 0) {
                isSelecting = false;
                selection.Clear();
                SetActive(overNodeId);
                requestContext = 3;
              }
              e.Use();
            }
          }
          break;
        case EventType.MouseUp:
          isSelecting = false;
          draggingNodeId = -2;
          e.Use();
          break;
        case EventType.MouseDrag:
          if (draggingNodeId > -2) {
            var node = selected.Get(draggingNodeId);
            if (node != null) {
              if (selection.Contains(node)) {
                foreach (DialogueNode node2 in selection) {
                  var rect = node2.window;
                  rect.x += e.delta.x;
                  rect.y += e.delta.y;
                  node2.window = rect;
                }
              } else {
                var rect = node.window;
                rect.x += e.delta.x;
                rect.y += e.delta.y;
                node.window = rect;
              }
            } else {
              draggingNodeId = -2;
            }
            Repaint();
          } else if (e.button == 2) {
            drag = e.delta;
            for (int i = -1; i < selected.nodes.Count; i++) {
              var node = selected.Get(i);
              if (node != null && !node.isEmpty) {
                var rect = node.window;
                rect.x += e.delta.x;
                rect.y += e.delta.y;
                node.window = rect;
              }
            }
            Repaint();
          } else if (isSelecting && e.button == 0) {
            selectionDragCurrent = e.mousePosition;
            UpdateSelection();
            Repaint();
          }
          break;
        case EventType.MouseMove:
          Repaint();
          break;
      }
      if (e.commandName == "UndoRedoPerformed") {
        Repaint();
      }
    }

    private void DrawToolbar() {
      toolBarRect.width = position.width;
      toolBarRect.height = EditorStyles.toolbar.fixedHeight;
      GUI.BeginGroup(toolBarRect, EditorStyles.toolbar);
      if (selected != null) {
        float textW = EditorStyles.toolbarButton.CalcSize(contentClear).x;
        Rect button1 = new Rect(5.0f, 0.0f, textW, toolBarRect.height);
        if (GUI.Button(button1, contentClear, EditorStyles.toolbarButton)) {
          OnClear();
        }
        textW = EditorStyles.toolbarDropDown.CalcSize(contentMode).x;
        Rect button2 = new Rect(5.0f + button1.x + button1.width, 0.0f, textW, toolBarRect.height);
        if (GUI.Button(button2, contentMode, EditorStyles.toolbarDropDown)) {
          GenericMenu menu = new GenericMenu();
          menu.AddItem(new GUIContent("Small"), viewMode == Mode.Small, () => {
            viewMode = Mode.Small;
            Repaint();
          });
          menu.AddItem(new GUIContent("Medium"), viewMode == Mode.Medium, () => {
            viewMode = Mode.Medium;
            Repaint();
          });
          // menu.AddItem(new GUIContent("Full"), viewMode == Mode.Full, () => {
          //   viewMode = Mode.Full;
          //   Repaint();
          // });
          menu.DropDown(button2);
        }
      } else {
        GUI.Label(new Rect(5.0f, 0.0f, 150.0f, toolBarRect.height), "No dialogue asset set", EditorStyles.miniLabel);
      }
      GUI.EndGroup();
    }

    private void DrawNodeField() {
      nodeFieldRect.y = toolBarRect.height;
      nodeFieldRect.width = position.width;
      nodeFieldRect.height = position.height - toolBarRect.height;
      GUI.BeginGroup(nodeFieldRect);
      Handles.BeginGUI();
      Handles.color = Color.white;
      Handles.DrawSolidRectangleWithOutline(new Rect(0, 0, nodeFieldRect.width, nodeFieldRect.height), gridBgColor, gridBgColor);
      Handles.EndGUI();
      DrawGrid(12, 0.2f);
      DrawGrid(120, 0.3f);
      if (selected != null) {
        if (isSelecting) {
          DrawSelectionBox();
        }
        if (connectionOutId > -2) {
          DrawConnection(connectionOutPos, Event.current.mousePosition);
          if (Event.current.type == EventType.MouseMove) {
            Repaint();
          }
        }
        for (int i = -1; i < selected.nodes.Count; i++) {
          var node = selected.Get(i);
          if (node != null && !node.isEmpty) {
            for (int j = 0; j < node.connectionsOut.Length; j++) {
              if (node.connectionsOut[j] > -2) {
                var inNode = selected.Get(node.connectionsOut[j]);
                DrawConnection(GetConnectionOutCenter(node, j), GetConnectionInCenter(inNode));
              }
            }
          }
        }
        for (int i = -1; i < selected.nodes.Count; i++) {
          if (i == activeNodeId) {
            continue;
          }
          DrawNode(selected.Get(i));
        }
        if (activeNodeId > -2) {
          DrawNode(selected.Get(activeNodeId));
        }
      }
      GUI.EndGroup();
    }

    private void DrawGrid(float spacing, float gridOpacity) {
      int gridWidth = Mathf.CeilToInt(nodeFieldRect.width / spacing);
      int gridHeight = Mathf.CeilToInt(nodeFieldRect.height / spacing);
      Handles.BeginGUI();
      Handles.color = new Color(0, 0, 0, gridOpacity);
      offset += drag * 0.5f;
      Vector3 newOffset = new Vector3(offset.x % spacing, offset.y % spacing, 0.0f);
      for (int x = 0; x <= gridWidth; x++) {
        Vector3 top = new Vector3(spacing * x, -spacing, 0.0f);
        Vector3 bottom = new Vector3(spacing * x, nodeFieldRect.height + spacing, 0.0f);
        Handles.DrawLine(top + newOffset, bottom + newOffset);
      }
      for (int y = 0; y <= gridHeight; y++) {
        Vector3 left = new Vector3(-spacing, spacing * y, 0.0f);
        Vector3 right = new Vector3(nodeFieldRect.width + spacing, spacing * y, 0.0f);
        Handles.DrawLine(left + newOffset, right + newOffset);
      }
      Handles.EndGUI();
    }

    private void DrawSelectionBox() {
      Rect selectionRect = GetSelectionRect(toolBarRect.height);
      if (selectionRect.height > 0 && selectionRect.width > 0) {
        GUI.Box(selectionRect, "", styleSelection);
      }
    }

    private Rect GetSelectionRect(float oy = 0.0f) {
      float minX = Mathf.Min(selectionDragStart.x, selectionDragCurrent.x);
      float minY = Mathf.Min(selectionDragStart.y, selectionDragCurrent.y);
      float maxX = Mathf.Max(selectionDragStart.x, selectionDragCurrent.x);
      float maxY = Mathf.Max(selectionDragStart.y, selectionDragCurrent.y);
      minY -= oy;
      maxY -= oy;
      return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private void UpdateSelection() {
      Rect selectionRect = GetSelectionRect();
      if (selectionRect.height > 0 && selectionRect.width > 0) {
        selection.Clear();
        for (int i = 0; i < selected.nodes.Count; i++) {
          var node = selected.Get(i);
          if (node != null && !node.isEmpty && selectionRect.Overlaps(node.window)) {
            selection.Add(node);
          }
        }
      }

    }

    private void DrawNode(DialogueNode node) {
      if (node == null || node.isEmpty) {
        return;
      }
      float lh = EditorGUIUtility.singleLineHeight;
      float spacing = 2.0f;
      switch (viewMode) {
        case Mode.Small: {
            node.window.height = lh + spacing;
            break;
          }
        case Mode.Medium: {
            if (node.id >= 0) {
              node.window.height = (lh + spacing) * 2;
            } else {
              node.window.height = lh + spacing;
            }
            break;
          }
          // case Mode.Full: {
          //     if (node.type == DialogueNode.Type.Dialogue) {
          //       node.window.height = (lh + spacing) * 5;
          //     } else if (node.type == DialogueNode.Type.Choice) {
          //       node.window.height = (lh + spacing) * (node.choices.Count + 1);
          //     } else {
          //       node.window.height = lh + spacing;
          //     }
          //     break;
          //   }
      }
      node.window.height += 4.0f;
      Rect inner = new Rect(node.window);
      inner.x = 4.0f;
      inner.y = 2.0f;
      inner.width -= 8.0f;
      inner.height -= 4.0f;
      GUIStyle style = styleNode;
      if (playtestFlowPosition >= 0 && node.id == playtestFlowPosition) {
        style = styleNodeCurrent;
      }
      if (node.id == activeNodeId || selection.Contains(node)) {
        style = styleNodeActive;
      }
      GUI.BeginGroup(node.window, style);
      GUI.BeginGroup(inner);
      GUIStyle titleLabel = new GUIStyle();
      titleLabel.alignment = TextAnchor.MiddleCenter;
      titleLabel.fontStyle = FontStyle.Bold;
      GUIStyle idLabel = new GUIStyle(titleLabel);
      idLabel.alignment = TextAnchor.MiddleRight;
      GUI.Label(new Rect(0, 0, inner.width, lh), node.type.ToString(), titleLabel);
      if (node.id >= 0) {
        GUI.Label(new Rect(0, 0, inner.width, lh), "ID:" + node.id, idLabel);
        switch (viewMode) {
          case Mode.Medium: {
              if (node.type == DialogueNode.Type.Message) {
                GUI.Label(new Rect(0, lh + spacing, inner.width, lh), node.text);
              } else if (node.type == DialogueNode.Type.Choice) {
                int count = node.choices != null ? node.choices.Count : 0;
                GUI.Label(new Rect(0, lh + spacing, inner.width, lh), count + " Choices");
              } else if (node.type == DialogueNode.Type.Event) {
                GUI.Label(new Rect(0, lh + spacing, inner.width, lh), node.eventMsg);
              }
              break;
            }
            // case Mode.Full: {
            //     if (node.type == DialogueNode.Type.Dialogue) {
            //       EditorGUI.BeginChangeCheck();
            //       string nameValue = EditorGUI.TextArea(new Rect(0, lh + spacing, inner.width, lh), node.name);
            //       string msgValue = EditorGUI.TextArea(new Rect(0, (lh + spacing) * 2, inner.width, lh * 3), node.message);
            //       if (EditorGUI.EndChangeCheck()) {
            //         Undo.RecordObject(selected, "Value change");
            //         node.name = nameValue;
            //         node.message = msgValue;
            //       }
            //     } else if (node.type == DialogueNode.Type.Choice) {
            //       GUI.Label(new Rect(0, lh + spacing, inner.width, lh), node.choices.Count + " Choices");
            //     }
            //     break;
            //   }
        }
      }
      GUI.EndGroup();
      GUI.EndGroup();
      if (node.window.Contains(Event.current.mousePosition)) {
        overNodeId = node.id;
      }
      if (node.id >= 0) {
        if (playtestFlowPosition >= 0 && node.id == playtestFlowPosition) {
          style = node.connectionsIn.Count > 0 ? styleConnectionCurrentUsed : styleConnectionCurrentFree;
        } else {
          style = node.connectionsIn.Count > 0 ? styleConnectionUsed : styleConnectionFree;
        }
        if (GUI.Button(GetConnectionInRect(node), "", style)) {
          if (connectionOutId > -2) {
            selected.Connect(connectionOutId, node.id, connectionOutSlot);
            connectionOutId = -2;
          }
        }
      }
      for (int i = 0; i < node.connectionsOut.Length; i++) {
        GUIContent label = new GUIContent("");
        if (node.type == DialogueNode.Type.Choice) {
          label.tooltip = "Choice " + i;
        }

        if (playtestFlowPosition >= 0 && node.id == playtestFlowPosition) {
          style = node.connectionsOut[i] != -2 ? styleConnectionCurrentUsed : styleConnectionCurrentFree;
        } else {
          style = node.connectionsOut[i] != -2 ? styleConnectionUsed : styleConnectionFree;
        }
        if (connectionOutId == node.id && connectionOutSlot == i) {
          style = styleConnectionActive;
        }
        if (GUI.Button(GetConnectionOutRect(node, i), label, style)) {
          selected.Connect(node.id, -2, i);
          connectionOutId = node.id;
          connectionOutSlot = i;
          connectionOutPos = GetConnectionOutCenter(node, i);
        }
      }
    }

    private void DrawConnection(Vector3 start, Vector3 end) {
      Vector3 startTan = start + Vector3.right * 25;
      Vector3 endTan = end + Vector3.left * 25;
      Handles.DrawBezier(start, end, startTan, endTan, Color.white, null, 2);
    }

    private void ShowAddContext(Vector2 pos) {
      GenericMenu menu = new GenericMenu();
      menu.AddItem(new GUIContent("Add Message"), false, OnAddMessage, pos);
      menu.AddItem(new GUIContent("Add Choice"), false, OnAddChoice, pos);
      menu.AddItem(new GUIContent("Add Event"), false, OnAddEvent, pos);
      menu.AddSeparator("");
      if (nodeCopies.Count > 0) {
        menu.AddItem(new GUIContent("Paste"), false, OnPaste, pos);
      } else {
        menu.AddDisabledItem(new GUIContent("Paste"));
      }
      menu.ShowAsContext();
    }

    private void ShowSelectionContext() {
      GenericMenu menu = new GenericMenu();
      menu.AddItem(new GUIContent("Copy selection"), false, OnCopySelection);
      menu.AddItem(new GUIContent("Delete selection"), false, OnDeleteSelection);
      menu.ShowAsContext();
    }

    private void ShowNodeContext() {
      GenericMenu menu = new GenericMenu();
      menu.AddItem(new GUIContent("Copy node"), false, OnCopyNode);
      menu.AddItem(new GUIContent("Delete node"), false, OnDeleteNode);
      menu.ShowAsContext();
    }

    private void OnAddMessage(object arg) {
      Vector2 pos = (Vector2)arg;
      DialogueNode newNode = new DialogueNode(DialogueNode.Type.Message);
      newNode.window = new Rect(pos.x, pos.y, newNode.window.width, 20);
      Undo.RecordObject(selected, "Add dialogue");
      selected.Add(newNode);
      SetActive(newNode.id);
    }

    private void OnAddChoice(object arg) {
      Vector2 pos = (Vector2)arg;
      DialogueNode newNode = new DialogueNode(DialogueNode.Type.Choice);
      newNode.window = new Rect(pos.x, pos.y, newNode.window.width, 20);
      Undo.RecordObject(selected, "Add choice");
      selected.Add(newNode);
      SetActive(newNode.id);
    }

    private void OnAddEvent(object arg) {
      Vector2 pos = (Vector2)arg;
      DialogueNode newNode = new DialogueNode(DialogueNode.Type.Event);
      newNode.window = new Rect(pos.x, pos.y, newNode.window.width, 20);
      Undo.RecordObject(selected, "Add event");
      selected.Add(newNode);
      SetActive(newNode.id);
    }

    private void OnDeleteNode() {
      Undo.RecordObject(selected, "Delete node");
      selected.Remove(overNodeId);
      SetActive(-2);
      overNodeId = draggingNodeId = -2;
    }

    private void OnDeleteSelection() {
      Undo.RecordObject(selected, "Delete nodes");
      foreach (DialogueNode node in selection) {
        selected.Remove(node.id);
      }
      SetActive(-2);
      overNodeId = draggingNodeId = -2;
    }

    private void OnCopySelection() {
      nodeCopies.Clear();
      List<DialogueNode> nodes = selection.ToList();
      float minX = nodes[0].window.x;
      float minY = nodes[0].window.y;
      for (int i = 0; i < nodes.Count; i++) {
        DialogueNode copy = new DialogueNode(nodes[i]);
        if (minX > copy.window.x) {
          minX = copy.window.x;
        }
        if (minY > copy.window.y) {
          minY = copy.window.y;
        }
        for (int j = 0; j < copy.connectionsIn.Count; j++) {
          copy.connectionsIn[j] = nodes[i].connectionsIn[j];
        }
        for (int j = 0; j < copy.connectionsOut.Length; j++) {
          copy.connectionsOut[j] = nodes[i].connectionsOut[j];
        }
        copy.id = nodes[i].id;
        nodeCopies.Add(copy);
      }
      for (int i = 0; i < nodeCopies.Count; i++) {
        DialogueNode copy = nodeCopies[i];
        copy.window.x -= minX;
        copy.window.y -= minY;
      }
    }

    private void OnCopyNode() {
      nodeCopies.Clear();
      DialogueNode current = selected.Get(overNodeId);
      if (current != null) {
        DialogueNode copy = new DialogueNode(current);
        copy.window.x = copy.window.y = 0;
        nodeCopies.Add(copy);
      }
    }

    private void OnPaste(object arg) {
      Vector2 pos = (Vector2)arg;
      Undo.RecordObject(selected, "Paste");
      List<DialogueNode> newNodes = new List<DialogueNode>();
      selection.Clear();
      for (int i = 0; i < nodeCopies.Count; i++) {
        DialogueNode copy = new DialogueNode(nodeCopies[i]);
        copy.window.x += pos.x;
        copy.window.y += pos.y;
        selected.Add(copy);
        if (i == 0) {
          SetActive(copy.id);
        }
        newNodes.Add(copy);
        selection.Add(newNodes[i]);
      }
      for (int i = 0; i < newNodes.Count; i++) {
        for (int j = 0; j < newNodes[i].connectionsOut.Length; j++) {
          int connectTo = -2;
          int oldOutId = nodeCopies[i].connectionsOut[j];
          for (int k = 0; k < newNodes.Count; k++) {
            if (oldOutId == nodeCopies[k].id) {
              connectTo = newNodes[k].id;
              break;
            }
          }
          selected.Connect(newNodes[i].id, connectTo, j);
        }
      }
    }

    private void OnClear() {
      Undo.RecordObject(selected, "Cleared");
      selected.Clear();
      offset = Vector2.zero;
      SetActive(-2);
      overNodeId = draggingNodeId = -2;
      connectionOutId = -2;
      connectionOutSlot = 0;
    }

    private void SetActive(int i) {
      if (selected) {
        activeNodeId = i;
        if (EditorWindow.focusedWindow == this) {
          Selection.activeObject = selected;
        }
      } else {
        activeNodeId = -2;
      }
      Repaint();
      OnRequestRepaint();
    }

    private Rect GetConnectionInRect(DialogueNode node) {
      return new Rect(node.window.x - 16, node.window.y + (node.window.height - 20.0f) / 2, 22, 20);
    }

    public Vector2 GetConnectionInCenter(DialogueNode node) {
      Rect rect = GetConnectionInRect(node);
      return new Vector2(rect.x + 5, rect.y + 10);
    }

    public Rect GetConnectionOutRect(DialogueNode node, int slotId) {
      float y = node.window.y + (node.window.height - 20) / 2.0f;
      y -= (node.connectionsOut.Length - 1) * 10;
      y += 20.0f * slotId;
      return new Rect(node.window.x + node.window.width, y, 22, 20);
    }

    public Vector2 GetConnectionOutCenter(DialogueNode node, int slotId) {
      Rect rect = GetConnectionOutRect(node, slotId);
      return new Vector2(rect.x + 12, rect.y + 10);
    }

  }
}
