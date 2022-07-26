using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AudioVisualizer : MonoBehaviour
{
    public AudioSource Src;
    public Color Col;
    public Color ProximityCol;
    public ComputeShader AudioVisualizationComputer;
    public GameObject MeshCarrier;
    public Material GridMaterial;
    public float Size;

    [Range(20, 252)]
    public int LinearVertexCount = 252;

    [Range(-5f, 5f)]
    public float InnerRadHeight = 1f;
    [Range(0, 3000f)]
    public float Radius = 0.5f;
    [Range(0, 1000f)]
    public float MaxHeight = 0.5f;

    private int ComputeId = -1;
    private GridElement[] GridData;
    private ComputeBuffer GridBuffer;
    private ComputeBuffer SpectrumBuffer;
    private ComputeBuffer VolumeBuffer;

    private MeshFilter Filter;
    private MeshRenderer Rend;
    private Material MaterialInstance;
    private Mesh GridMesh;

    void Start()
    {
        InitGrid();
        StartCoroutine(Beat());
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
            Temp = Vector3.right * (Size *1.5f/ 2 - X *1.5f* GridElementSize) + Vector3.forward * (Size / 2 - Y * GridElementSize * Mathf.Sqrt(3)+ GridElementSize * (X % 2) * Mathf.Sqrt(3) / 2.0f);

            UV.x = (float)X / LinearVertexCount;
            UV.y = (float)Y / LinearVertexCount;

            Positions.Add(Temp);
            indices.Add(i);
            Uvs.Add(UV);

            GridElement element = new GridElement();
            element.Proximity = 0;
            element.Running = 0;
            element.Height = 0;
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
        ComputeId = AudioVisualizationComputer.FindKernel("ComputeGrid");

        //Buffer
        int ByteSize = sizeof(float) * 6;
        GridBuffer = new ComputeBuffer(GridData.Length, ByteSize);
        GridBuffer.SetData(GridData);

        AudioVisualizationComputer.SetBuffer(ComputeId, "GridElements", GridBuffer);
        MaterialInstance.SetBuffer("GridElements", GridBuffer);
        MaterialInstance.SetInt("LinearVertexCount", LinearVertexCount);
        MaterialInstance.SetFloat("Scale", Size / (float)LinearVertexCount);
    }

    private void SetParams()
    {
       // float[] spectrum = new float[2048];
       // Src.GetOutputData(spectrum, 0);
       //// Src.GetSpectrumData(spectrum, 0, FFTWindow.Hamming);
       // Debug.Log(spectrum.Length);
        //for (int i = 1; i < spectrum.Length - 1; i++)
        //{
        //    Debug.DrawLine(new Vector3(i - 1, spectrum[i] * 5 + 10, 0), new Vector3(i, spectrum[i + 1] * 5 + 10, 0), Color.red);
        //    //if (spectrum[i] > 0.1)
        //    //    Debug.Log(spectrum[i] + "  " + i);
        //    Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2), Color.cyan);
        //    //Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
        //    // Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3), Color.blue);
        //}

        AudioVisualizationComputer.SetFloat("DeltaTime", Time.deltaTime);
        AudioVisualizationComputer.SetFloat("Time", Time.realtimeSinceStartup);
        MaterialInstance.SetColor("_Color", Col);
        MaterialInstance.SetColor("_ProximityCol", ProximityCol);
        AudioVisualizationComputer.SetFloat("Radius", Radius);
        AudioVisualizationComputer.SetFloat("MaxHeight", MaxHeight);
        AudioVisualizationComputer.SetFloat("InnerRadHeight", InnerRadHeight);
     //   AudioVisualizationComputer.SetFloats("Spectrum", spectrum);
        AudioVisualizationComputer.SetInt("GridElementCount", LinearVertexCount*LinearVertexCount);
        AudioVisualizationComputer.SetFloat("Volume", Src.volume);
    }

    private void InitGrid()
    {
        InitMeshAndGridData();
        SetParams();
        SetBuffersAndInitShaders();
    }

  
    private void ComputeGrid()
    {
        SetParams();
        AudioVisualizationComputer.Dispatch(ComputeId, LinearVertexCount*LinearVertexCount / 128, 1, 1);
    }

    private float RMS = 0;
    //private float Ref = 0.1f;

    private IEnumerator Beat()
    {
        yield return new WaitForSecondsRealtime(Time.deltaTime);
        while (true)
        {
            if (SpectrumBuffer != null)
                SpectrumBuffer.Release(); 
            if (VolumeBuffer != null)
                VolumeBuffer.Release();

            float[] samples = new float[512];
            Src.GetOutputData(samples, 0); // fill array with samples

            //var i: int;
            float sum = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += Mathf.Abs(samples[i]);
            }
            RMS = Mathf.Sqrt( sum / samples.Length);
            //rmsValue = Mathf.Sqrt(sum / qSamples); // rms = square root of average
            //dbValue = 20 * Mathf.Log10(rmsValue / refValue); // calculate dB
            //if (dbValue < -160) dbValue = -160; // clamp it to -160dB min


            float[] spectrum = new float[2048];
            Src.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
            //   Debug.Log(spectrum.Length);
            //Buffer
            SpectrumBuffer = new ComputeBuffer(2048, sizeof(float));
            //VolumeBuffer = new ComputeBuffer(2048, sizeof(float));
           // SpectrumBuffer.SetData(new float[2048]);
            SpectrumBuffer.SetData(spectrum);
           // VolumeBuffer.SetData(samples);

            AudioVisualizationComputer.SetBuffer(ComputeId, "SpectrumData", SpectrumBuffer);
            AudioVisualizationComputer.SetFloat( "RMSVolume", RMS);
          //  AudioVisualizationComputer.SetBuffer(ComputeId, "VolumeData", VolumeBuffer);
           // AudioVisualizationComputer.SetFloats("Spectrum", spectrum);
            yield return new WaitForSecondsRealtime(Time.deltaTime/50);
        }
    }
 

}


public struct GridElement
{
    public Vector3 Position;
    public float Proximity;
    public float Running;
    public float Height;
};
