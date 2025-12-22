using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;


[CanEditMultipleObjects]
[CustomEditor(typeof(FocusTMPInputField), true)]
public class FocusTMPInputFieldEditor : TMP_InputFieldEditor {
    private SerializedProperty _usePreset;
    private SerializedProperty _height;
    private SerializedProperty _minY;
    private SerializedProperty _presetMinY;
    private SerializedProperty _maxY;
    protected override void OnEnable() {
        base.OnEnable();
        _usePreset = serializedObject.FindProperty("_usePreset");
        _height = serializedObject.FindProperty("_height");
        _minY = serializedObject.FindProperty("_minY");
        _presetMinY = serializedObject.FindProperty("_presetMinY");
        _maxY = serializedObject.FindProperty("_maxY");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.LabelField("Focus Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_usePreset);
        EditorGUILayout.PropertyField(_height);
        if (_usePreset.boolValue) {
            EditorGUILayout.PropertyField(_presetMinY, new GUIContent("Min Y"));
        } else {
            EditorGUILayout.PropertyField(_minY);
        }
        EditorGUILayout.PropertyField(_maxY);
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Input Field Properties", EditorStyles.boldLabel);
        base.OnInspectorGUI();
    }
}
