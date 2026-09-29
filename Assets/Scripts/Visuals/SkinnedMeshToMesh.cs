using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
public class SkinnedMeshToMesh : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMesh;
    public VisualEffect VFXGraph;
    public float refreshRate;

    // Made once and refilled each refresh: the pose baked, then its vertices alone, which the graph samples
    private Mesh baked;
    private Mesh points;
    private readonly List<Vector3> vertices = new();

    // Start is called before the first frame update
    void Start()
    {
        baked = new Mesh();
        points = new Mesh();
        StartCoroutine(UpdateVFXGraph());
    }

    private void OnDestroy()
    {
        Destroy(baked);
        Destroy(points);
    }

    IEnumerator UpdateVFXGraph()
    {
        var wait = new WaitForSeconds(refreshRate);
        while (gameObject.activeSelf)
        {
            skinnedMesh.BakeMesh(baked);
            baked.GetVertices(vertices);
            points.SetVertices(vertices);

            VFXGraph.SetMesh("Mesh", points);

            yield return wait;
        }
    }
}
