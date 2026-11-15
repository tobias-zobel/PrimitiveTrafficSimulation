using UnityEngine;
using UnityEngine.Splines;

public class GridGenerator : MonoBehaviour
{
    [Header("Grid Settings")] 
    public int gridWidth = 3;
    public int gridHeight = 3;
    public float spacing = 30f;
    
    [Header("Variation Settings")]
    public int seed;

    [Range(0f, 1f)] public float roadMissingChance = .25f; // 25% of segment will be empty

    public float positionJitter = 5f; // moving junction a little
    
    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        seed = Random.Range(0, int.MaxValue);
        Random.InitState(seed);
        
        // clearing
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        // generating horizontal roads with jitter
        Vector3[,] nodePositions = new Vector3[gridWidth, gridHeight];
        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth - 1; x++)
            {
                float jitterX = (x == 0 || x == gridWidth - 1) ? 0f : Random.Range(-positionJitter, positionJitter);
                float jitterZ = (z == 0 || z == gridHeight - 1) ? 0f : Random.Range(-positionJitter, positionJitter);

                nodePositions[x, z] = new Vector3(x * spacing + jitterX, 0, z * spacing + jitterZ);
            }
        }
        
        // generating horizontal roads with break chance
        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth - 1; x++)
            {
                if (x > 0 && x < gridWidth - 2 && Random.value < roadMissingChance) continue;

                Vector3 startPos = nodePositions[x, z];
                Vector3 endPos = nodePositions[x + 1, z];
                CreateRoadSegment(startPos, endPos, $"Road_Horizontal_x:{x}_z:{z}");
            }
        }
        
        // generating vertical roads with break chance
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight - 1; z++)
            {
                if (z > 0 && z < gridHeight - 2 && Random.value < roadMissingChance) continue;

                Vector3 startPos = nodePositions[x, z];
                Vector3 endPos = nodePositions[x, z + 1];
                CreateRoadSegment(startPos, endPos, $"Road_Vertical_x:{x}_z:{z}");
            }
        }
        
        
        
        Debug.Log("GridGenerator.cs: grid successful generated...");
    }

    void CreateRoadSegment(Vector3 startPos, Vector3 endPos, string roadName)
    {
        // new gameobj for a road
        GameObject roadObject = new GameObject(roadName);
        roadObject.transform.parent = transform;
        
        // placing the road/spline
        SplineContainer splineContainer = roadObject.AddComponent<SplineContainer>();
        Spline spline = splineContainer.Spline;

        spline.Clear();
        spline.Add(new BezierKnot(startPos));
        spline.Add(new BezierKnot(endPos));
        
        // adding the road-script so the mesh is being build
        Road roadMesh = roadObject.AddComponent<Road>();

        if (roadMesh != null)
        {
            roadMesh.GenerateRoadMesh();
        }
    }
}
