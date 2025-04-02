#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class MeshPolyCountCalculator : Editor
{
    [MenuItem("MyUtils/Calculate Mesh Poly Count")]
    private static void CalculateMeshPolyCount()
    {
        int totalPolyCount = 0, totalEnableObjsPolyCount = 0;

        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            MeshFilter[] filters = selectedObject.GetComponentsInChildren<MeshFilter>(true);

            foreach (MeshFilter filter in filters)
            {
                MyUtils.Log($"{filter.gameObject.name} Poly Count: {GetPolyCount(filter.sharedMesh)} Gameobject activeSelf: { filter.gameObject.activeSelf}");
                totalPolyCount += GetPolyCount(filter.sharedMesh);

                if (filter.gameObject.activeSelf)
                    totalEnableObjsPolyCount += GetPolyCount(filter.sharedMesh);
            }

            SkinnedMeshRenderer[] skinnedMeshRenderers = selectedObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                totalPolyCount += GetPolyCount(skinnedMeshRenderer.sharedMesh);
                if (skinnedMeshRenderer.gameObject.activeSelf)
                {
                    totalEnableObjsPolyCount += GetPolyCount(skinnedMeshRenderer.sharedMesh);
                }
            }
        }

        MyUtils.Log("Total Enable objs Poly Count: " + totalEnableObjsPolyCount);
        MyUtils.Log("Total Poly Count: " + totalPolyCount);
    }

    private static int GetPolyCount(Mesh mesh)
    {
        if (mesh == null)
        {
            return 0;
        }

        int polyCount = mesh.triangles.Length / 3;
        return polyCount;
    }
}

#endif
