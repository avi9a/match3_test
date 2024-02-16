using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(Board))]
public class CustPropertyDrawer : PropertyDrawer {

	public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
		EditorGUI.PrefixLabel(position,label);
		Rect newposition = position;
		newposition.y += 18f;
		SerializedProperty board = property.FindPropertyRelative("board");
        if (board.arraySize != 14)
	        board.arraySize = 14;
		//data.rows[0][]
		for(int j=0;j<14;j++){
			SerializedProperty elements = board.GetArrayElementAtIndex(j).FindPropertyRelative("elements");
			newposition.height = 18f;
			if(elements.arraySize != 9)
				elements.arraySize = 9;
			newposition.width = position.width/9;
			for(int i=0;i<9;i++){
				EditorGUI.PropertyField(newposition,elements.GetArrayElementAtIndex(i),GUIContent.none);
				newposition.x += newposition.width;
			}

			newposition.x = position.x;
			newposition.y += 18f;
		}
	}

	public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
		return 18f * 15;
	}
}
