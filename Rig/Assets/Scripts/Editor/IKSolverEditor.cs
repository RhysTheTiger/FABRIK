using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IKSolver))]
public class IKSolverEditor : Editor
{
    #region Variables.

    private bool _basicFoldout = true;
    private bool _ikFoldout = true;

    private IKSolver _ikSolver;

    #endregion

    #region Methods.

    private void DrawBasicSettingsFoldout()
    {
        _basicFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_basicFoldout, "Basic Settings");
        {
            if (_basicFoldout)
            {
                EditorGUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("iterations"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tolerance"));
                }
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawIKChainFoldout()
    {
        _ikFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_ikFoldout, "IK Chain");
        {
            if (_ikFoldout)
            {
                EditorGUILayout.BeginVertical("Box");
                {
                    DrawIKChain(serializedObject.FindProperty("bones"));
                    EditorGUILayout.Separator();

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pole"));
                }
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawIKChain(SerializedProperty ikChain)
    {
        if (ikChain.isExpanded)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chainLength"));
            EditorGUILayout.Separator();
            ikChain.arraySize = _ikSolver.chainLength + 1;

            for (int i = 0; i < ikChain.arraySize; ++i)
            {
                GUIContent label;
                if (i == 0)
                {
                    label = new GUIContent("Root Bone");
                }
                else if (i == ikChain.arraySize - 1)
                {
                    label = new GUIContent("End Bone");
                }
                else
                {
                    label = new GUIContent("Bone " + i);
                }
                EditorGUILayout.PropertyField(ikChain.GetArrayElementAtIndex(i), label);
            }
        }
    }

    #endregion

    #region Life-cycle Callbacks.

    public override void OnInspectorGUI()
    {
        _ikSolver = (IKSolver) target;
        
        serializedObject.Update();

        GUIStyle boxStyle = new GUIStyle("Box");
        boxStyle.padding.left = 20;
        boxStyle.padding.right = 20;

        EditorGUILayout.BeginVertical(boxStyle);
        {
            DrawBasicSettingsFoldout();
            EditorGUILayout.Separator();
            DrawIKChainFoldout();
            EditorGUILayout.Separator(); 
        }
        EditorGUILayout.EndVertical();
        
        serializedObject.ApplyModifiedProperties();
    }

    #endregion
}
