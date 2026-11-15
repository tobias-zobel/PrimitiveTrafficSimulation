using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Road : MonoBehaviour
{
    private SplineContainer _splineContainer;
    private MeshFilter _meshFilter;

    [Tooltip("Width of the street (meters)")]
    public float roadWidth = 4f;

    [Tooltip("Amount of Segments per meter")]
    public int resolutionPerMeter = 2;

    void Start()
    {
        _splineContainer = GetComponent<SplineContainer>();
        _meshFilter = GetComponent<MeshFilter>();

        GenerateRoadMesh();
    }

    public void GenerateRoadMesh()
    {
        if (_splineContainer == null) _splineContainer = GetComponent<SplineContainer>();
        if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
        
        Spline spline = _splineContainer.Spline;
        float length = spline.GetLength();
        int vertexCount = Mathf.Max(2, Mathf.RoundToInt(length*resolutionPerMeter));
        
        Vector3[] vertices = new Vector3[vertexCount * 2];
        Vector2[] uvs = new Vector2[vertexCount * 2];
        int[] triangles = new int[(vertexCount - 1) * 6];

        float step = 1f / (vertexCount - 1);

        for (int i = 0; i < vertexCount; i++)
        {
            float t = i * step;

            // get position and direction of spline at position of t
            spline.Evaluate(t, out float3 position, out float3 tangent, out float3 upVector);

            Vector3 pos = position;
            Vector3 forward = math.normalize(tangent);
            
            // calc for left and right side of street
            Vector3 right = Vector3.Cross(upVector, forward).normalized;

            // left and right border
            vertices[i * 2] = pos - (right * (roadWidth * 0.5f));
            vertices[i * 2 + 1] = pos + (right * (roadWidth * 0.5f));
            
            // uv for texturing
            uvs[i * 2] = new Vector2(0, t * (length / 5f));
            uvs[i * 2 + 1] = new Vector2(1, t * (length / 5f));

            // connecting meshes (starts at second segment)
            if (i < vertexCount - 1)
            {
                int vertIndex = i * 2;
                int triIndex = i * 6;
            
                triangles[triIndex] = vertIndex;
                triangles[triIndex + 1] = vertIndex + 2;
                triangles[triIndex + 2] = vertIndex + 1;
            
                triangles[triIndex + 3] = vertIndex + 1;
                triangles[triIndex + 4] = vertIndex + 2;
                triangles[triIndex + 5] = vertIndex + 3;
            }
        }
        
        // creating new mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        _meshFilter.mesh = mesh;
    }
}
