using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ScrollContent))]
public class ScrollContentEditor : Editor
{
    SerializedProperty m_Direction;
    SerializedProperty m_ScrollRect;
    SerializedProperty m_Sample;
    SerializedProperty m_Padding;
    SerializedProperty m_Spacing;
    SerializedProperty m_VCorner;
    SerializedProperty m_HCorner;
    SerializedProperty m_RawChildCount;

    void OnEnable()
    {
        m_ScrollRect = serializedObject.FindProperty("m_ScrollRect");
        m_Sample = serializedObject.FindProperty("m_Sample");
        m_Direction = serializedObject.FindProperty("m_Direction");
        m_Padding = serializedObject.FindProperty("m_Padding");
        m_Spacing = serializedObject.FindProperty("m_Spacing");
        m_VCorner = serializedObject.FindProperty("m_VCorner");
        m_HCorner = serializedObject.FindProperty("m_HCorner");
        m_RawChildCount = serializedObject.FindProperty("m_RawChildCount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_ScrollRect);
        EditorGUILayout.PropertyField(m_Sample);
        EditorGUILayout.PropertyField(m_Padding, true);
        EditorGUILayout.PropertyField(m_Spacing);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Direction);
        EditorGUILayout.PropertyField(m_VCorner);
        EditorGUILayout.PropertyField(m_HCorner);
        EditorGUILayout.PropertyField(m_RawChildCount);
        if (EditorGUI.EndChangeCheck())
        {
            var dir = (ScrollContent.Direction)m_Direction.enumValueIndex;
            foreach (var o in serializedObject.targetObjects)
            {
                ScrollContent c = o as ScrollContent;
                c.SetDirection(dir);
            }
        }
        serializedObject.ApplyModifiedProperties();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear"))
        {
            var content = target as ScrollContent;
            content.ClearLaout();
        }
        if (GUILayout.Button("Layout"))
        {
            var content = target as ScrollContent;
            content.LayoutCompleted();
        }
        GUILayout.EndHorizontal();
    }
}
