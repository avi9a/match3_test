﻿using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Board))]
public class CustPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
		EditorGUI.PrefixLabel(position,label);
		Rect newPosition = position;
		newPosition.y += 18f;
		SerializedProperty board = property.FindPropertyRelative("board");
        if (board.arraySize != 6)
	        board.arraySize = 6;
		for(int j=0;j<6;j++){
			SerializedProperty elements = board.GetArrayElementAtIndex(j).FindPropertyRelative("elements");
			newPosition.height = 18f;
			if(elements.arraySize != 5)
				elements.arraySize = 5;
			newPosition.width = position.width/5;
			for(int i=0;i<5;i++){
				EditorGUI.PropertyField(newPosition,elements.GetArrayElementAtIndex(i),GUIContent.none);
				newPosition.x += newPosition.width;
			}

			newPosition.x = position.x;
			newPosition.y += 18f;
		}
	}

	public override float GetPropertyHeight(SerializedProperty property,GUIContent label)
	{
		return 9f * 15;
	}
}
