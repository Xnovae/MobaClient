#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Polybrush
{
	/**
	 *	Workaround for bug in `MeshRenderer.additionalVertexStreams`.
	 * 
	 *	Namely, the mesh is not persistent in the editor and needs to be "refreshed" constantly.
	 *
	 *		- https://issuetracker.unity3d.com/issues/meshrenderer-dot-additionalvertexstreams-collapse-static-meshes
	 *		- https://issuetracker.unity3d.com/issues/api-mesh-cannot-change-vertex-colors-using-meshrender-dot-additionalvertexstreams
	 *		- https://issuetracker.unity3d.com/issues/meshrenderer-dot-additionalvertexstreams-discards-data-if-set-in-awake
	 *		- https://issuetracker.unity3d.com/issues/meshrenderer-dot-additionalvertexstreams-looses-color-fast-in-editor
	 */
	[ExecuteInEditMode]
	public class z_AdditionalVertexStreams : MonoBehaviour
	{
		public Mesh mesh = null;
		
		private MeshRenderer _meshRenderer;

		private MeshRenderer meshRenderer
		{
			get {
				if(_meshRenderer == null)
					_meshRenderer = gameObject.GetComponent<MeshRenderer>();
				return _meshRenderer;
			}
		}

		public void SetMesh(Mesh mesh)
		{
			this.mesh = mesh;
			meshRenderer.additionalVertexStreams = mesh;
		}

		void Update()
		{
			if(meshRenderer == null || mesh == null || EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			meshRenderer.additionalVertexStreams = mesh;
		}
	}
}
#endif
