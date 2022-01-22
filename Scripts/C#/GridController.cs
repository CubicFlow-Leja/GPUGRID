using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GridController : MonoBehaviour
{
    public Color Col;
    public Color ProximityCol;
    public ComputeShader GridComputer;
    public GameObject MeshCarrier;
    public Material GridMaterial;
    public float Size;

    [Range(80, 252)]
    public int LinearVertexCount = 252;

    [Range(-5f, 5f)]
    public float InnerRadHeight = 1f;
    [Range(0.1f, 1f)]
    public float Radius = 0.5f;

    private int ComputeId = -1;
    private GridElement[] GridData;
    private ComputeBuffer GridBuffer;

    private MeshFilter Filter;
    private MeshRenderer Rend;
    private Material MaterialInstance;
    private Mesh GridMesh;
    private List<Transform> Spheres = new List<Transform>();

    void Start()
    {
        InitGrid();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            InitGrid();
        ComputeGrid();
    }
    private void InitMeshAndGridData()
    {
        if (GridBuffer != null)
            GridBuffer.Release();
        //data
        GridData = new GridElement[LinearVertexCount * LinearVertexCount];

        //material and filter
        MaterialInstance = new Material(GridMaterial);
        Filter = MeshCarrier.GetComponent<MeshFilter>();
        Rend = MeshCarrier.GetComponent<MeshRenderer>();

        List<Vector3> Positions = new List<Vector3>(LinearVertexCount * LinearVertexCount);
        List<Vector2> Uvs = new List<Vector2>(LinearVertexCount * LinearVertexCount);
        List<int> indices = new List<int>(LinearVertexCount * LinearVertexCount);

        float GridElementSize = Size / (float)LinearVertexCount;
        for (int i = 0; i < LinearVertexCount * LinearVertexCount; i++)
        {
            Vector3 Temp = Vector3.zero;
            Vector2 UV = Vector2.zero;

            int X = i % LinearVertexCount;
            int Y = Mathf.FloorToInt(i / LinearVertexCount);

            //cubes
           // Temp = Vector3.right * (SizeX / 2 + X) + Vector3.forward * (SizeZ / 2 + Y);
            //hex
            Temp = Vector3.right * (-Size *1.5f/ 2 + X *1.5f* GridElementSize) + Vector3.forward * (-Size / 2 + Y * GridElementSize * Mathf.Sqrt(3)+ GridElementSize * (X % 2) * Mathf.Sqrt(3) / 2.0f);

            UV.x = (float)X / LinearVertexCount;
            UV.y = (float)Y / LinearVertexCount;

            Positions.Add(Temp);
            indices.Add(i);
            Uvs.Add(UV);

            GridElement element = new GridElement();
            element.Proximity = 0;
            element.Position = Temp;
            GridData[i] = element;

        }

        GridMesh = new Mesh();
        GridMesh.SetVertices(Positions);
        GridMesh.SetIndices(indices, MeshTopology.Points, 0);
        GridMesh.SetUVs(0, Uvs);
        GridMesh.name = "GRIDMESH";
        GridMesh.RecalculateBounds();
        Filter.sharedMesh = GridMesh;
        Rend.sharedMaterial = MaterialInstance;
    }
  
    private void SetBuffersAndInitShaders()
    {
        ComputeId = GridComputer.FindKernel("ComputeGrid");

        //Buffer
        int ByteSize = sizeof(float) * 4;
        GridBuffer = new ComputeBuffer(GridData.Length, ByteSize);
        GridBuffer.SetData(GridData);

        GridComputer.SetBuffer(ComputeId, "GridElements", GridBuffer);
        MaterialInstance.SetBuffer("GridElements", GridBuffer);
        MaterialInstance.SetInt("LinearVertexCount", LinearVertexCount);
        MaterialInstance.SetFloat("Scale", Size / (float)LinearVertexCount);
    }

    private void SetParams()
    {
        GridComputer.SetFloat("DeltaTime", Time.deltaTime);
        GridComputer.SetFloat("Time", Time.realtimeSinceStartup);
        MaterialInstance.SetColor("_Color", Col);
        MaterialInstance.SetColor("_ProximityCol", ProximityCol);
        GridComputer.SetFloat("Radius", Radius);
        GridComputer.SetFloat("InnerRadHeight", InnerRadHeight);
    }

    private void InitGrid()
    {
        InitMeshAndGridData();
        SetParams();
        SetBuffersAndInitShaders();
    }

    private void InitAndSendSphereList()
    {
        List<Vector4> SphereData = new List<Vector4>();
        foreach(Transform tr in Spheres)
        {
            Vector4 temp = new Vector4(tr.position.x, tr.position.y, tr.position.z, tr.localScale.x);
            SphereData.Add(temp);
        }

        GridComputer.SetVectorArray("Spheres", SphereData.ToArray());
        GridComputer.SetInt("SphereCount", SphereData.Count);
    }

    private void ComputeGrid()
    {
        SetParams();
        InitAndSendSphereList();
        GridComputer.Dispatch(ComputeId, LinearVertexCount*LinearVertexCount / 128, 1, 1);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
            if(!Spheres.Contains(other.transform))
                Spheres.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
            if (Spheres.Contains(other.transform))
                Spheres.Remove(other.transform);
    }

}
public struct GridElement
{
    public Vector3 Position;
    public float Proximity;
};
