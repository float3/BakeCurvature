#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System;
using System.IO;

namespace ThreeCurvatureBaker
{
    [ExecuteAlways]
    public class BakeCurvature : MonoBehaviour
    {
        public enum Dest
        {
            VertexColors = 0,
            UVS = 1
        }

        public enum UVSet
        {
            UV0 = 0,
            UV1 = 1,
            UV2 = 2,
            UV3 = 3,
            UV4 = 4,
            UV5 = 5,
            UV6 = 6,
            UV7 = 7
        }

        private static readonly int UVSetProp = Shader.PropertyToID("_UVSet");
        private static readonly int DestProp = Shader.PropertyToID("_Dest");
        [HideInInspector] public ComputeShader shader;
        public Material material;
        public Dest dest;
        public UVSet uvSet;
        private int _kernelMain;
        public void OnEnable()
        {
            shader = Resources.Load<ComputeShader>("BakeCurvature");
            _kernelMain = shader.FindKernel("ComputeCurvature");
        }

        public void RunKernel()
        {
            Mesh sourcePositionMesh;
            Mesh sourceMesh;
            SkinnedMeshRenderer smr = null;
            MeshFilter mf = null;
            if (gameObject.TryGetComponent(out smr))
            {
                sourceMesh = smr.sharedMesh;
                sourcePositionMesh = new Mesh();
                smr.BakeMesh(sourcePositionMesh);
            }
            else if (gameObject.TryGetComponent(out mf))
            {
                sourceMesh = mf.sharedMesh;
                sourcePositionMesh = sourceMesh;
            }
            else return;

            GetMeshData(sourcePositionMesh, out Vector3[] verts, out Vector3[] normals, out Vector4[] tangents, out Vector3Int[] indices);

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

            string parentName = sourceMesh.name;
            Mesh newMesh = new Mesh();
            int size = sourceMesh.vertices.Length;
            List<Vector4> srcUV = new List<Vector4>();
            List<Vector4> srcUV2 = new List<Vector4>();
            List<Vector4> srcUV3 = new List<Vector4>();
            List<Vector4> srcUV4 = new List<Vector4>();
            List<Vector4> srcUV5 = new List<Vector4>();
            List<Vector4> srcUV6 = new List<Vector4>();
            List<Vector4> srcUV7 = new List<Vector4>();
            List<Vector4> srcUV8 = new List<Vector4>();
            sourceMesh.GetUVs(0, srcUV);
            sourceMesh.GetUVs(1, srcUV2);
            sourceMesh.GetUVs(2, srcUV3);
            sourceMesh.GetUVs(3, srcUV4);
            sourceMesh.GetUVs(4, srcUV5);
            sourceMesh.GetUVs(5, srcUV6);
            sourceMesh.GetUVs(6, srcUV7);
            sourceMesh.GetUVs(7, srcUV8);
            Vector3[] srcPosVertices = sourcePositionMesh.vertices;
            Vector3[] srcVertices = sourceMesh.vertices;
            Color[] srcColors = sourceMesh.colors; // FIXME: Should use colors?
            Vector3[] srcNormals = sourceMesh.normals;
            Vector4[] srcTangents = sourceMesh.tangents;
            Matrix4x4[] srcBindposes = sourceMesh.bindposes;
            BoneWeight[] srcBoneWeights = sourceMesh.boneWeights;
            Vector4[] newUV2 = new Vector4[size];
            newMesh.vertices = srcVertices;
            if (srcNormals != null && srcNormals.Length > 0)
            {
                newMesh.normals = srcNormals;
            }
            if (srcTangents != null && srcTangents.Length > 0)
            {
                newMesh.tangents = srcTangents;
            }
            if (srcBoneWeights != null && srcBoneWeights.Length > 0)
            {
                newMesh.boneWeights = srcBoneWeights;
            }
            if (srcColors != null && srcColors.Length > 0)
            {
                newMesh.colors = srcColors;
            }
            if (srcUV.Count > 0)
            {
                newMesh.SetUVs(0, srcUV);
            }
            if (srcUV2.Count > 0)
            {
                newMesh.SetUVs(1, srcUV2);
            }
            if (srcUV3.Count > 0)
            {
                newMesh.SetUVs(2, srcUV3);
            }
            if (srcUV4.Count > 0)
            {
                newMesh.SetUVs(3, srcUV4);
            }
            if (srcUV5.Count > 0)
            {
                newMesh.SetUVs(4, srcUV5);
            }
            if (srcUV6.Count > 0)
            {
                newMesh.SetUVs(5, srcUV6);
            }
            if (srcUV7.Count > 0)
            {
                newMesh.SetUVs(6, srcUV7);
            }
            if (srcUV8.Count > 0)
            {
                newMesh.SetUVs(7, srcUV8);
            }
            newMesh.subMeshCount = sourceMesh.subMeshCount;
            for (int i = 0; i < sourceMesh.subMeshCount; i++)
            {
                int[] curIndices = sourceMesh.GetIndices(i);
                newMesh.SetIndices(curIndices, sourceMesh.GetTopology(i), i);
            }
            newMesh.bounds = sourceMesh.bounds;
            if (srcBindposes != null && srcBindposes.Length > 0)
            {
                newMesh.bindposes = sourceMesh.bindposes;
            }
            for (int i = 0; i < sourceMesh.blendShapeCount; i++)
            {
                string blendShapeName = sourceMesh.GetBlendShapeName(i);
                int blendShapeFrameCount = sourceMesh.GetBlendShapeFrameCount(i);
                for (int frameIndex = 0; frameIndex < blendShapeFrameCount; frameIndex++)
                {
                    float weight = sourceMesh.GetBlendShapeFrameWeight(i, frameIndex);
                    Vector3[] deltaVertices = new Vector3[size];
                    Vector3[] deltaNormals = new Vector3[size];
                    Vector3[] deltaTangents = new Vector3[size];
                    sourceMesh.GetBlendShapeFrameVertices(i, frameIndex, deltaVertices, deltaNormals, deltaTangents);
                    newMesh.AddBlendShapeFrame(blendShapeName, weight, deltaVertices, deltaNormals, deltaTangents);
                }
            }


            if (dest == Dest.VertexColors)
            {
                List<Color> colors = new List<Color>();
                for (int i = 0; i < verts.Length; i++)
                {
                    Color color = Color.black;
                    color.r = Mathf.Max(output[i], 0f);
                    color.g = -Mathf.Min(output[i], 0f);
                    colors.Add(color);
                }

                newMesh.SetColors(colors);
            }
            else if (dest == Dest.UVS)
            {
                List<Vector2> curvature = new List<Vector2>();
                for (int i = 0; i < verts.Length; i++)
                {
                    Vector2 curv = Vector2.zero;
                    curv.x = Mathf.Max(output[i], 0f);
                    curv.y = -Mathf.Min(output[i], 0f);
                    curvature.Add(curv);
                }

                newMesh.SetUVs((int)uvSet, curvature);
            }

            newMesh.name = sourceMesh.name + "_uvmerged";
            Mesh meshAfterUpdate = newMesh;
            if (smr != null)
            {
                Undo.RecordObject(smr, "Switched SkinnedMeshRenderer to baked Mesh");
                smr.sharedMesh = newMesh;
                meshAfterUpdate = smr.sharedMesh;
                // No need to change smr.bones: should use same bone indices and blendshapes.
            }
            if (mf != null)
            {
                Undo.RecordObject(mf, "Switched MeshFilter to baked Mesh");
                mf.sharedMesh = newMesh;
                meshAfterUpdate = mf.sharedMesh;
            }
            string pathToGenerated = "Assets/Curvature/Generated";
            if (!Directory.Exists(pathToGenerated))
            {
                Directory.CreateDirectory(pathToGenerated);
            }
            int lastSlash = parentName.LastIndexOf('/');
            string outFileName = lastSlash == -1 ? parentName : parentName.Substring(lastSlash + 1);
            outFileName = outFileName.Split('.')[0];
            string fileName = pathToGenerated + "/" + outFileName + "_Curvature_" + dest.ToString();
            fileName += dest == Dest.UVS ? uvSet.ToString() : "";
            fileName += "_" + DateTime.UtcNow.ToString("s").Replace(':', '_') + ".asset";
            AssetDatabase.CreateAsset(meshAfterUpdate, fileName);
            AssetDatabase.SaveAssets();
            if (smr == null && mf == null)
            {
                EditorGUIUtility.PingObject(meshAfterUpdate);
            }

            AssetDatabase.Refresh();
            if (material != null)
            {
                material.SetInt(DestProp, (int)dest);
                material.SetInt(UVSetProp, (int)uvSet);
            }
        }
        private void GetMeshData(Mesh mesh, out Vector3[] verts, out Vector3[] normals, out Vector4[] tangents,
            out Vector3Int[] indices)
        {
            verts = new Vector3[] { };
            normals = new Vector3[] { };
            tangents = new Vector4[] { };
            indices = new Vector3Int[] { };

            List<Vector3> vertsTmp = new List<Vector3>();
            List<Vector3> normalsTmp = new List<Vector3>();
            List<Vector4> tangentsTmp = new List<Vector4>();
            List<Vector3Int> indicesTmp = new List<Vector3Int>();

            Vector3[] meshVerts = mesh.vertices;
            Vector3[] meshNormals = mesh.normals;
            Vector4[] meshTangents = mesh.tangents;
            int[] meshIndices = mesh.triangles;

            Vector3Int offset = Vector3Int.one * vertsTmp.Count;
            foreach (Vector3 vert in meshVerts) vertsTmp.Add(vert);

            foreach (Vector3 normal in meshNormals) normalsTmp.Add(normal);

            foreach (Vector4 tangent in meshTangents) tangentsTmp.Add(tangent);

            for (int i = 0; i < meshIndices.Length; i += 3)
                indicesTmp.Add(new Vector3Int(meshIndices[i], meshIndices[i + 1], meshIndices[i + 2]) + offset);

            verts = vertsTmp.ToArray();
            normals = normalsTmp.ToArray();
            tangents = tangentsTmp.ToArray();
            indices = indicesTmp.ToArray();
        }
    }

    [CustomEditor(typeof(BakeCurvature)), CanEditMultipleObjects]
    public class BakeCurvatureEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BakeCurvature tar = target as BakeCurvature;

            if (GUILayout.Button("Bake Curvature"))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Debug.Log("Bake Started");

                tar.RunKernel();

                sw.Stop();
                Debug.Log($"Process took {sw.ElapsedMilliseconds} ms.");
                GUIUtility.ExitGUI();
            }
        }
    }
}
#endif