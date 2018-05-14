using UnityEngine;
using UnityEditor;

namespace Polybrush
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(z_AdditionalVertexStreams))]
	public class z_AdditionalVertexStreamsEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			z_AdditionalVertexStreams avs = target as z_AdditionalVertexStreams;

			if(avs == null)
				return;

			MeshRenderer mr = avs.gameObject.GetComponent<MeshRenderer>();

			GUILayout.Label("Additional Vertex Streams");

			if(targets.Length > 1) 
				EditorGUI.showMixedValue = true;

			EditorGUILayout.ObjectField(mr.additionalVertexStreams, typeof(Mesh), true);

			EditorGUI.showMixedValue = false;

			if(GUILayout.Button("Delete"))	
			{
				foreach(z_AdditionalVertexStreams a in targets)
				{			
					if(a == null)
						continue;

					mr = a.GetComponent<MeshRenderer>();
					
					if(mr != null)
						mr.additionalVertexStreams = null;

					if(a.mesh != null)
					{
						Undo.DestroyObjectImmediate(a);
						Undo.RecordObject(mr, "Delete AdditionalVertexStreams");
					}				
				}
			}
		}
	}
}
