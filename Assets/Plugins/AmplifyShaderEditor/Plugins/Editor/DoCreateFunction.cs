// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
namespace AmplifyShaderEditor
{
	public class DoCreateFunction : AssetCreationEndAction
	{
		public override void Action( EntityId instanceId, string pathName, string resourceFile )
		{
			UnityEngine.Object obj = EditorUtility.EntityIdToObject( instanceId );
			AssetDatabase.CreateAsset( obj, AssetDatabase.GenerateUniqueAssetPath( pathName ) );
			AmplifyShaderEditorWindow.LoadShaderFunctionToASE( (AmplifyShaderFunction)obj, false );
		}
	}
}
