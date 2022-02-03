using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace f3BC
{
    public class BakeCurvature : MonoBehaviour
    {
        [HideInInspector] public ComputeShader shader;
        public Material material;
        public OutputType destinationVertexStream;
        private int _kernelMain;
        private static readonly int Output = Shader.PropertyToID("_Output");

        public void OnEnable()
        {
            _kernelMain = shader.FindKernel("CSMain");
        }

        public void RunKernel()
        {
            GetMeshData(out Vector3[] verts, out Vector3[] normals, out Vector4[] tangents, out Vector3Int[] indices);

            Debug.Log($"Mesh has {verts.Length} vertices and {indices.Length} triangles");

            ComputeBuffer vertBuffer = new ComputeBuffer(verts.Length, sizeof(float) * 3);
            ComputeBuffer normalBuffer = new ComputeBuffer(normals.Length, sizeof(float) * 3);
            ComputeBuffer tangentBuffer = new ComputeBuffer(tangents.Length, sizeof(float) * 4);
            ComputeBuffer indiceBuffer = new ComputeBuffer(indices.Length, sizeof(int) * 3);
            ComputeBuffer outBuffer = new ComputeBuffer(verts.Length, sizeof(float));

            vertBuffer.SetData(verts);
            normalBuffer.SetData(normals);
            tangentBuffer.SetData(tangents);
            indiceBuffer.SetData(indices);

            shader.SetBuffer(_kernelMain, "verts", vertBuffer);
            shader.SetBuffer(_kernelMain, "normals", normalBuffer);
            shader.SetBuffer(_kernelMain, "tangents", tangentBuffer);
            shader.SetBuffer(_kernelMain, "indices", indiceBuffer);
            shader.SetBuffer(_kernelMain, "output", outBuffer);
            shader.SetInt("indicelength", indices.Length);

            shader.Dispatch(_kernelMain, Mathf.CeilToInt(indices.Length / 64f), 1, 1);

            float[] output = new float[verts.Length];

            outBuffer.GetData(output);

            vertBuffer.Release();
            normalBuffer.Release();
            tangentBuffer.Release();
            indiceBuffer.Release();
            outBuffer.Release();

            Mesh mesh;
            if (gameObject.TryGetComponent(out SkinnedMeshRenderer smr))
            {
                mesh = smr.sharedMesh;
            }
            else if (gameObject.TryGetComponent(out MeshFilter mf))
            {
                mesh = mf.sharedMesh;
            }
            else
            {
                return;
            }

            if (destinationVertexStream == OutputType.VertexColors)
            {
                List<Color> colors = new List<Color>();
                for (int i = 0; i < verts.Length; i++)
                {
                    Color color = Color.black;
                    color.r = Mathf.Max(output[i], 0f);
                    color.g = -Mathf.Min(output[i], 0f);
                    colors.Add(color);
                }

                mesh.SetColors(colors);
            }
            else
            {
                List<Vector4> curvature = new List<Vector4>();
                for (int i = 0; i < verts.Length; i++)
                {
                    Vector4 curv = Vector4.zero;
                    curv.x = Mathf.Max(output[i], 0f);
                    curv.y = -Mathf.Min(output[i], 0f);
                    curvature.Add(curv);
                }

                mesh.SetUVs((int) destinationVertexStream, curvature);
            }

            mesh.MarkModified();
#if UNITY_EDITOR
            EditorUtility.SetDirty(mesh);
            AssetDatabase.Refresh();
#endif
            material.SetInt(Output, (int) destinationVertexStream);
        }

        private void GetMeshData(out Vector3[] verts, out Vector3[] normals, out Vector4[] tangents,
            out Vector3Int[] indices)
        {
            verts = new Vector3[] { };
            normals = new Vector3[] { };
            tangents = new Vector4[] { };
            indices = new Vector3Int[] { };

            Mesh mesh;
            if (gameObject.TryGetComponent(out SkinnedMeshRenderer smr))
            {
                mesh = smr.sharedMesh;
            }
            else if (gameObject.TryGetComponent(out MeshFilter mf))
            {
                mesh = mf.sharedMesh;
            }
            else
            {
                return;
            }

            List<Vector3> vertsTmp = new List<Vector3>();
            List<Vector3> normalsTmp = new List<Vector3>();
            List<Vector4> tangentsTmp = new List<Vector4>();
            List<Vector3Int> indicesTmp = new List<Vector3Int>();

            Vector3[] meshVerts = mesh.vertices;
            Vector3[] meshNormals = mesh.normals;
            Vector4[] meshTangents = mesh.tangents;
            int[] meshIndices = mesh.triangles;

            Vector3Int offset = Vector3Int.one * vertsTmp.Count;
            foreach (var vert in meshVerts)
            {
                vertsTmp.Add(gameObject.transform.localToWorldMatrix.MultiplyPoint(vert));
            }

            foreach (var normal in meshNormals)
            {
                normalsTmp.Add(normal);
            }

            foreach (var tangent in meshTangents)
            {
                tangentsTmp.Add(tangent);
            }

            for (int i = 0; i < meshIndices.Length; i += 3)
            {
                indicesTmp.Add(new Vector3Int(meshIndices[i], meshIndices[i + 1], meshIndices[i + 2]) + offset);
            }

            verts = vertsTmp.ToArray();
            normals = normalsTmp.ToArray();
            tangents = tangentsTmp.ToArray();
            indices = indicesTmp.ToArray();
        }

        public enum OutputType
        {
            VertexColors = 8,
            UV0 = 0,
            UV1 = 1,
            UV2 = 2,
            UV3 = 3,
            UV4 = 4,
            UV5 = 5,
            UV6 = 6,
            UV7 = 7,
        }

        public struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector4 Tangent;
            public int Index;
        }

        public struct Edge
        {
            public Vertex A;
            public Vertex B;
        }

        public struct Indice
        {
            public Vertex A;
            public Vertex B;
            public Vertex C;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BakeCurvature)), CanEditMultipleObjects]
    public class BakeCurvatureEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BakeCurvature tar = target as BakeCurvature;

            if (GUILayout.Button("Bake Curvature"))
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                Debug.Log("Bake Started");

                tar.RunKernel();

                sw.Stop();
                Debug.Log($"Bake took {sw.ElapsedMilliseconds} ms.");
                GUIUtility.ExitGUI();
            }
        }
    }
#endif
}