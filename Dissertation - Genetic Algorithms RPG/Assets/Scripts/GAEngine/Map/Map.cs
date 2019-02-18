using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GAEngine.Map;
using Zenject;
using Assets.Entities;
using System;
using System.Threading.Tasks;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Engine;
using GeneticAlgorithmEngine;

public class Map : MonoBehaviour
{
    public List<Material> TerrainMaterials;
    public Mesh ret;




    /*[Inject]
    public IHeightMapGenerator _heightmapGenerator;
    
    [Header("Can be used to create lakes/mesas")]
    public bool FlattenTerrain = true;
    [Header("Used to add additional hills to the tops of mesas")]
    public bool AdditionalHills = true;

    public List<GeneticData> GeneData { get; set; } = new List<GeneticData>();
    public ExpressedMapData ExpressedData { get; set; } 

    public List<RandomInt> Genes { get; set; } = new List<RandomInt>();

    public int TotalVertices = 0;
    
    public int Width = 255;
    public float Spacing = 1.4f;
    public GameObject TerrainMesh;
    public GameObject Water;
    public List<Material> TerrainMaterials;
    public Mesh ret;

    public Material interpolatedMat;

    [Header("Variables for the fitness function")]
    public int MountainousVertices;
    public int UnderwaterVertices;
    public int GrasslandVertices;
    public float AverageVertexHeight;
    public int numberOfPeaks;

    
    public float Fitness { get; set; }

    public string Name { get; set; }

    public float fitValue;

    [Range(15, 40)]
    public float MaxHeight = 20f;
    [Range(0, 0.4f)]
    public float waterLevel;
    [Range(0.005f, 0.05f)]
    public float xFreq;
    [Range(0.005f, 0.05f)]
    public float zFreq;
    [Range(0.1f, 5)]
    public float waveLength;
    [Range(0.5f, 4)]
    public float exponent;
    [Range(1, 10)]
    public int octaves = 10;
    [Range(0.5f, 0.95f)]
    public float mountainLevel;

    // The y component for the water's position
    float waterHeight = 0;
    // The y threshold for vertices being classed as "mountainous"
    float mountainHeight = 0;
    // The highest vertex in the scene
    float highestVertex = 0;
    float ceiling = 0;
    float totalVertexHeights;

    //public List<List<Vector3>> settleViableArea = new List<List<Vector3>>();
    public Vector3[,] settleViableArea;
    List<int> settlementTris = new List<int>();
    List<Triangle> settlementTriangles = new List<Triangle>();
    public Mesh settleMesh;
    List<Vector2> potentialPeaks = new List<Vector2>();
    Vector3[,] verts { get; set; }
    List<int> tris = new List<int>();
    Vector2[] uvs;
    Vector2[] uv2s;
    Vector2[] uv3s;
    Vector2[] uv4s;
  /*List<Vector2> uvs = new List<Vector2>();
    List<Vector2> uv2s = new List<Vector2>();
    List<Vector2> uv3s = new List<Vector2>();
    List<Vector2> uv4s = new List<Vector2>();#1#

    int settlementTriCount = 0;
    Vector3[] unfoldedVerts;*/

    private MeshFilter MeshFilter;

    private object mapLock = new object();

    public void Awake()
    {
        /*MeshFilter = GetComponent<MeshFilter>();*/
    }


    public IChromosome GenerateGene(int length)
    {
        throw new Exception();
        /*var random = new System.Random();
        Water = MapParameters.MP.Water;
        lock (mapLock)
            GeneData.Clear();
        FlattenTerrain = true;
        lock (mapLock)
            GeneData.Add(new GeneticData(FlattenTerrain));
        AdditionalHills = true;
        lock (mapLock)
            GeneData.Add(new GeneticData(AdditionalHills));
        Genes = new List<RandomInt>();
        lock (mapLock)
            GeneData.Add(new GeneticData());
        for (int i = 0; i < length; ++i)
        {
            Genes.Add(new RandomInt(-8000, 8000));//.Next(-8000, 8000));//UnityEngine.Random.Range(10, 80));
            GeneData[2].DataSet.Add(Genes[i]);
        };
        MaxHeight = Math.Max((float)random.NextDouble() * 40, 15); //UnityEngine.Random.Range(15f, 40f);
        lock(mapLock)
            GeneData.Add(new GeneticData(MaxHeight));
        waterLevel = (float)random.NextDouble() * 0.4f; //UnityEngine.Random.Range(0, 0.4f);
        lock (mapLock)
            GeneData.Add(new GeneticData(waterLevel));
        xFreq = (float)Math.Max(random.NextDouble() * 0.04f, 0.005); //UnityEngine.Random.Range(0.005000f, 0.04000f);
        lock (mapLock)
            GeneData.Add(new GeneticData(xFreq));
        zFreq = xFreq;
        lock (mapLock)
            GeneData.Add(new GeneticData(zFreq));
        waveLength = (float)Math.Max(random.NextDouble() * 5f, 1.3); //UnityEngine.Random.Range(1.3f, 5f);
        lock (mapLock)
            GeneData.Add(new GeneticData(waveLength));
        exponent = (float)Math.Max(random.NextDouble() * 4f, 1f); //UnityEngine.Random.Range(1f, 4f);
        lock (mapLock)
            GeneData.Add(new GeneticData(exponent));
        mountainLevel = (float)Math.Max(random.NextDouble() * 0.95f, 0.65); //UnityEngine.Random.Range(0.65f, 0.95f);
        lock (mapLock)
            GeneData.Add(new GeneticData(mountainLevel));
        unfoldedVerts = new Vector3[Width * Width];
        settleViableArea = new Vector3[Width, Width];

        uvs = new Vector2[Width * Width];
        uv2s = new Vector2[Width * Width];
        uv3s = new Vector2[Width * Width];
        uv4s = new Vector2[Width * Width];

        if (TerrainMaterials.Count == 0)
        {
            lock (mapLock)
                TerrainMaterials.Add(MaterialController.MC.Materials[0]);
        }

        GenerateMesh();
        return this;*/
    }

    public void AssignData(List<GeneticData> dataToAdd)
    {
        /*Water = MapParameters.MP.Water;
        FlattenTerrain = (bool)dataToAdd[0].DataSet[0];
        AdditionalHills = (bool)dataToAdd[1].DataSet[0];
        Genes.Clear();
        
        for (int i = 0; i < dataToAdd[2].DataSet.Count; ++i)
        {
            Genes.Add((RandomInt)dataToAdd[2].DataSet[i]);
        }
        MaxHeight = (float)dataToAdd[3].DataSet[0];
        waterLevel = (float)dataToAdd[4].DataSet[0];
        xFreq = (float)dataToAdd[5].DataSet[0];
        zFreq = (float)dataToAdd[6].DataSet[0];
        waveLength = (float)dataToAdd[7].DataSet[0];
        exponent = (float)dataToAdd[8].DataSet[0];
        mountainLevel = (float)dataToAdd[9].DataSet[0];
        GenerateMesh();
        SetWater();*/
    }
    
    public void Initialise()
    {
        /*Water = MapParameters.MP.Water;
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshCollider>();
        GetComponent<MeshRenderer>().material = Resources.Load<Material>("Terrain");
        var mesh = GetComponent<Map>();
        mesh.ret = new Mesh();
        mesh.ret.MarkDynamic();
        mesh.ret.name = "mapMesh";
        mesh.settleMesh = new Mesh();
        mesh.settleMesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = ret;
        GeneData = new List<GeneticData>();
        GenerateGene(10);
        gameObject.name = "Map";
        gameObject.tag = "Map";*/
    }

    public void GenerateMesh()
    {
        /*numberOfPeaks = 0;
        Water = MapParameters.MP.Water;
        totalVertexHeights = 0;
        TotalVertices = 0;
        MountainousVertices = 0;
        UnderwaterVertices = 0;
        GrasslandVertices = 0;
        ceiling = MaxHeight * 5;
        if(GetComponent<MeshFilter>() != null)
            GetComponent<MeshFilter>()?.mesh.Clear();
        /*if (MeshFilter != null)
        {
            Array.Clear(MeshFilter?.mesh.vertices, 0, MeshFilter.mesh.vertices.Length);
            Array.Clear(MeshFilter?.mesh.triangles, 0, MeshFilter.mesh.triangles.Length);
            Array.Clear(MeshFilter?.mesh.uv, 0, MeshFilter.mesh.uv.Length);
            Array.Clear(MeshFilter?.mesh.uv2, 0, MeshFilter.mesh.uv2.Length);
            Array.Clear(MeshFilter?.mesh.uv3, 0, MeshFilter.mesh.uv3.Length);
            Array.Clear(MeshFilter?.mesh.uv4, 0, MeshFilter.mesh.uv4.Length);
            Array.Clear(MeshFilter?.mesh.tangents, 0, MeshFilter.mesh.tangents.Length);
            Array.Clear(MeshFilter?.mesh.normals, 0, MeshFilter.mesh.normals.Length);

        }#1#
           // MeshFilter?.mesh.Clear();
        if (ret == null)
            ret = new Mesh();
        ret.Clear();
        if (settleMesh == null)
            settleMesh = new Mesh();
        settleMesh.Clear();

        // Generate everything.
        // This could be an additional function, which is then threaded.
        var mapGenParameters = new MapGenerationParameters
        {
            AdditionalHills = AdditionalHills,
            Exponent = exponent,
            FlattenTerrain = FlattenTerrain,
            Genes = Genes,
            MaximumHeight = MaxHeight,
            MountainLevel = mountainLevel,
            NumberOfOctaves = octaves,
            Spacing = Spacing,
            WaterLevel = waterLevel,
            Wavelength = waveLength,
            Width = Width,
            XFrequency = xFreq,
            ZFrequency = zFreq            
        };
        var heightMap = _heightmapGenerator.GenerateHeightMap(mapGenParameters);
        //verts = heightMap.Verts;
        tris = heightMap.Tris;
        waterHeight = heightMap.WaterHeight;
        highestVertex = heightMap.HighestVertex;
        mountainHeight = heightMap.MountainHeight;
        totalVertexHeights = heightMap.VertexCount;

        // This is where we go over an additional loop of the whole array to calculate peaks and shears
        CalculatePeaksAndSettlements(heightMap);
        
        //SetWater();
        highestVertex = 0;

        AverageVertexHeight = heightMap.SumOfAllVertexHeights / Mathf.Pow(Width, 2);

        // Unfold the 2d array of verticies into a 1d array.
          
        for(var i = 0; i < Width; i++)
        {
            for(var j = 0; j < Width; j++)
            {
                unfoldedVerts[(i * Width) + j] = heightMap.Verts[(i * Width) + j].Location;
            }
        }

        Vector3[] settlementUnfoldedVerts = new Vector3[Width * Width];
        for (var i = 0; i < Width; i++)
        {
            for (var j = 0; j < Width; j++)
            {
                settlementUnfoldedVerts[(i * Width) + j] = settleViableArea[i, j];
            }
        }


        settleMesh.vertices = unfoldedVerts;
        settleMesh.triangles = settlementTris.ToArray();

        settleMesh.RecalculateBounds();
        settleMesh.RecalculateNormals();
        // Generate the mesh object.
        ret.vertices = unfoldedVerts;
        ret.triangles = tris.ToArray();
        ret.uv = uvs.ToArray();
        ret.uv2 = uv2s.ToArray();
        ret.uv3 = uv3s.ToArray();
        ret.uv4 = uv4s.ToArray();

        // Assign the mesh object and update it.
        ret.RecalculateBounds();
        ret.RecalculateNormals();

        if(transform.childCount == 0)
        TerrainMesh = GameObject.CreatePrimitive(PrimitiveType.Plane);
        TerrainMesh.GetComponent<MeshFilter>().mesh = ret;
        TerrainMesh.transform.parent = transform;
        TerrainMesh.GetComponent<MeshRenderer>().material = TerrainMaterials[0];

        // Clear no longer needed data

        settlementTriCount = 0;
        Array.Clear(unfoldedVerts, 0, Width * Width);
        settlementUnfoldedVerts = null;
        settlementTriangles.Clear();
        settlementTriangles = null;
        Array.Clear(settleViableArea, 0, settleViableArea.Length);
        potentialPeaks.Clear();
        tris.Clear();
        settlementTris.Clear();
        Array.Clear(uvs, 0, Width * Width);
        Array.Clear(uv2s, 0, Width * Width);
        Array.Clear(uv3s, 0, Width * Width);
        Array.Clear(uv4s, 0, Width * Width);
        WaterController.SetHeight(waterHeight);*/
    }

    void CalculatePeaksAndSettlements(MapGenerationResult result)
    {
        /*settlementTriCount = 0;
        settlementTriangles = new List<Triangle>();

        var settlementViableAreaLock = new object();
        var potentialPeaksLock = new object();
        var settlementTrisLock = new object();
        Parallel.For(0, Width, i =>
        {
            Parallel.For(0, Width, j =>
            {
                lock (settlementViableAreaLock)
                {
                    //Mountainous
                    if (result.Verts[(i * Width) + j].Location.y > (mountainHeight))
                    {
                        uvs[(i * Width) + j] = new Vector2(0.5f, 1.0f);
                        uv2s[(i * Width) + j] = new Vector2(1.0f, 1.0f);
                        uv3s[(i * Width) + j] = new Vector2(0.5f, 0.5f);
                        uv4s[(i * Width) + j] = new Vector2(1.0f, 0.5f);
                        MountainousVertices++;
                    }
                    //Underwater
                    else if (result.Verts[(i * Width) + j].Location.y < waterHeight)
                    {
                        UnderwaterVertices++;

                        uvs[(i * Width) + j] =  new Vector2(0.0f, 0.5f);
                        uv2s[(i * Width) + j] = new Vector2(0.25f, 0.5f);
                        uv3s[(i * Width) + j] = new Vector2(0.0f, 0.25f);
                        uv4s[(i * Width) + j] = new Vector2(0.25f, 0.25f);
                    }
                    //Grassland
                    else if (result.Verts[(i * Width) + j].Location.y >= waterHeight && result.Verts[(i * Width) + j].Location.y <= mountainHeight)
                    {
                        GrasslandVertices++;
                        uvs[(i * Width) + j] = new Vector2(0.0f, 1.0f);
                        uv2s[(i * Width) + j] = (new Vector2(0.5f, 1.0f));
                        uv3s[(i * Width) + j] = (new Vector2(0f, 0.5f));
                        uv4s[(i * Width) + j] = (new Vector2(0.5f, 0.5f));
                    }
                }

                lock (settlementViableAreaLock)
                {
                    if (result.Verts[(i * Width) + j].Location.y >= mountainHeight)
                    {
                        potentialPeaks.Add(new Vector2(i, j));
                    }
                }
                lock (settlementViableAreaLock)
                {
                    if ((result.Verts[(i * Width) + j].Location.y < mountainHeight) && (result.Verts[(i * Width) + j].Location.y > waterHeight))
                    {
                        if ((i + 1 < Width) && (i - 1 > 0) && (j + 1 < Width) && (j - 1 > 0))
                        {
                            {
                                int current_x = i;
                                if (current_x - 1 <= 0 || j <= 0 || current_x >= Width)
                                {
                                    return;
                                }
                                Triangle tempTri = new Triangle(new PointIndex(result.Verts[(i * Width) + j].Location, i + j * Width),
                                                                new PointIndex(result.Verts[(i * Width) + j + 1].Location, current_x + (j - 1) * Width),
                                                                new PointIndex(result.Verts[((i +1) * Width) + j].Location, (current_x - 1) + (j - 1) * Width));
                                if (tempTri.IsValid(Spacing * 2))
                                {
                                    if (tempTri.IsWithinShearTolerance(10f))
                                    {
                                        settlementTris.AddRange(new List<int> { tempTri.First.Index, tempTri.Second.Index, tempTri.Third.Index });
                                        settlementTriCount++;
                                    }
                                }

                                if (i - 1 <= 0 || j <= 0)
                                {
                                    return;
                                }
                                tempTri = new Triangle(new PointIndex(result.Verts[(i * Width) + j].Location, i + j * Width),
                                                       new PointIndex(result.Verts[(i * Width) + j + 1].Location, (current_x - 1) + (j - 1) * Width),
                                                       new PointIndex(result.Verts[((i + 1) * Width) + j].Location, (i - 1) + j * Width));
                                if (tempTri.IsValid(Spacing * 2))
                                {
                                    if (tempTri.IsWithinShearTolerance(10f))
                                    {
                                        settlementTris.AddRange(new List<int> { tempTri.First.Index, tempTri.Second.Index, tempTri.Third.Index });
                                        settlementTriCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            });
        });

        List<Vector3> calculatedPeaks = new List<Vector3>();
        
        int searchRange = 10;
        //for (int i = 0; i < potentialPeaks.Count; ++i)
        /*Parallel.For(0, potentialPeaks.Count, (i) =>
        {
            Vector2 highestIndex = new Vector2();
            float highestPoint = 0;
            Parallel.For(0, searchRange, (k) =>
            //for (int k = -searchRange; k < searchRange; ++k)
            {
                //for (int l = -searchRange; l < searchRange; ++l)
                /*Parallel.For(0, searchRange, (l) =>
                {
                    if ((potentialPeaks[i].x + k < Width) && (potentialPeaks[i].x + k > 0) && (potentialPeaks[i].y + l < Width) && (potentialPeaks[i].y + l > 0))
                    {
                        if (result.Verts[(int)potentialPeaks[i].x + k, (int)potentialPeaks[i].y + l].Location.y > highestPoint)
                        {
                            highestPoint = result.Verts[(int)potentialPeaks[i].x + k, (int)potentialPeaks[i].y + l].Location.y;
                            highestIndex.x = (int)potentialPeaks[i].x + k;
                            highestIndex.y = (int)potentialPeaks[i].y + l;
                        }
                    }
                });#2#
            });
            lock (settlementViableAreaLock)
            {
                calculatedPeaks.Add(result.Verts[(int)highestIndex.x, (int)highestIndex.y].Location);
            }
        });#1#/*
        calculatedPeaks.Sort((a, b) => a.y.CompareTo(b.y));#1#
        for (int i = calculatedPeaks.Count -1; i > 0; i--)
        {
            if (i - 1 > 0)
            {
                if (calculatedPeaks[i] == calculatedPeaks[i - 1])
                {
                    calculatedPeaks.RemoveAt(i- 1);
                }
            }
        }
        numberOfPeaks = calculatedPeaks.Count;*/
    }

    float GetHeight(float x, float z)
    {
        /*int leftGeneSum = 0;
        int rightGeneSum = 0;
        for (int i = 0; i < Genes.Count; ++i)
        {
            if (i < (Genes.Count / 2))
            {
                leftGeneSum += Genes[i].Value;
            }
            else
            {
                rightGeneSum += Genes[i].Value;
            }
        }
        float returnVal = waveLength * Mathf.PerlinNoise(xFreq * (x + leftGeneSum), zFreq * (z + rightGeneSum));
        for (int i = 1; i < octaves; i++)
        {
            returnVal += (1 / Mathf.Pow(2, i)) * waveLength * Mathf.PerlinNoise(i * Mathf.Sin(xFreq) * (x - leftGeneSum), (i * Mathf.Sin(zFreq)) * (z - rightGeneSum));
        }
        return returnVal;*/
        throw new Exception();
    }

    // Update is called once per frame
    /*void Update()
    {
        /*CalculateFitness();#1#
        fitValue = Fitness;
        //GenerateMesh();
    }*/

    public void CalculateFitness()
    {
        /*float temp = TotalVertices;
        temp /= 100;
        float tempM = MountainousVertices / 100;
        float tempG = GrasslandVertices / 100;
        float tempU = UnderwaterVertices / 100;
        float mountainPer = (tempM) / (temp);
        float grasslandPer = (tempG) / (temp);
        float underwaterPer = (tempU) / (temp);
        float peakPer = (float)numberOfPeaks / (float)MapParameters.MP.TargetPeaks;


        float mountFitness = 1 - Mathf.Abs(mountainPer - MapParameters.MP.Mountainous);
        float grassFitness = 1 - Mathf.Abs(grasslandPer - MapParameters.MP.Grassland);
        float underwaterFitness = 1 - Mathf.Abs(underwaterPer - MapParameters.MP.Underwater);
        float peakFitness = 1 - Mathf.Abs(peakPer - 1);

        // Requires work on flatness using AverageVertexHeight
        //Fitness = mountFitness + grassFitness + underwaterFitness;
        Fitness = GAFunctions.Average(mountFitness, grassFitness, underwaterFitness, peakFitness);*/
    }

    public List<IChromosome> CrossOver(IChromosome partner)
    {
        throw new Exception();
        /*List<IChromosome> children = new List<IChromosome>();
        List<GeneticData> child1 = new List<GeneticData>();
        List<GeneticData> child2 = new List<GeneticData>();

        Map temp = (Map)partner;

        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(FlattenTerrain));
            child2.Add(new GeneticData(temp.FlattenTerrain));
        }
        else
        {
            child1.Add(new GeneticData(temp.FlattenTerrain));
            child2.Add(new GeneticData(FlattenTerrain));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(AdditionalHills));
            child2.Add(new GeneticData(temp.AdditionalHills));
        }
        else
        {
            child1.Add(new GeneticData(temp.AdditionalHills));
            child2.Add(new GeneticData(AdditionalHills));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData());
            child2.Add(new GeneticData());
            for (int i = 0; i < 5; ++i)
            {
                child1[2].DataSet.Add(Genes[i]);
                child2[2].DataSet.Add(partner.Genes[i]);
            }
            for (int i = 0; i < 5; ++i)
            {
                child1[2].DataSet.Add(partner.Genes[i]);
                child2[2].DataSet.Add(Genes[i]);
            }
        }
        else
        {
            child1.Add(new GeneticData());
            child2.Add(new GeneticData());
            for (int i = 0; i < 5; ++i)
            {
                child1[2].DataSet.Add(partner.Genes[i]);
                child2[2].DataSet.Add(Genes[i]);
            }
            for (int i = 0; i < 5; ++i)
            {
                child1[2].DataSet.Add(Genes[i]);
                child2[2].DataSet.Add(partner.Genes[i]);
            }
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(MaxHeight));
            child2.Add(new GeneticData(temp.MaxHeight));
        }
        else
        {
            child1.Add(new GeneticData(temp.MaxHeight));
            child2.Add(new GeneticData(MaxHeight));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(waterLevel));
            child2.Add(new GeneticData(temp.waterLevel));
        }
        else
        {
            child1.Add(new GeneticData(temp.waterLevel));
            child2.Add(new GeneticData(waterLevel));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(xFreq));
            child2.Add(new GeneticData(temp.xFreq));
        }
        else
        {
            child1.Add(new GeneticData(temp.xFreq));
            child2.Add(new GeneticData(xFreq));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(zFreq));
            child2.Add(new GeneticData(temp.zFreq));
        }
        else
        {
            child1.Add(new GeneticData(temp.zFreq));
            child2.Add(new GeneticData(zFreq));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(waveLength));
            child2.Add(new GeneticData(temp.waveLength));
        }
        else
        {
            child1.Add(new GeneticData(temp.waveLength));
            child2.Add(new GeneticData(waveLength));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(exponent));
            child2.Add(new GeneticData(temp.exponent));
        }
        else
        {
            child1.Add(new GeneticData(temp.exponent));
            child2.Add(new GeneticData(exponent));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(mountainLevel));
            child2.Add(new GeneticData(temp.mountainLevel));
        }
        else
        {
            child1.Add(new GeneticData(temp.mountainLevel));
            child2.Add(new GeneticData(mountainLevel));
        }
        AssignData(child1);
        children.Add(GetComponent<Map>());
        AssignData(child2);
        children.Add(GetComponent<Map>());
        return children;*/
    }

    public List<IChromosome> Crossover<Map>(Map a)
    {
        throw new Exception();
        /*return new List<IChromosome>();*/
    } 

    public List<IChromosome> CrossOver(Map partner)
    {
        throw new Exception();
        /*List<IChromosome> children = new List<IChromosome>();
        List<GeneticData> child1 = new List<GeneticData>();
        List<GeneticData> child2 = new List<GeneticData>();


        if(GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(FlattenTerrain));
            child2.Add(new GeneticData(partner.FlattenTerrain));
        }
        else
        {
            child1.Add(new GeneticData(partner.FlattenTerrain));
            child2.Add(new GeneticData(FlattenTerrain));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(AdditionalHills));
            child2.Add(new GeneticData(partner.AdditionalHills));
        }
        else
        {
            child1.Add(new GeneticData(partner.AdditionalHills));
            child2.Add(new GeneticData(AdditionalHills));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData());
            child2.Add(new GeneticData());
            for(int i = 0; i < 5; ++i)
            {
                child1[2].DataSet.Add(Genes[i]);
                child2[2].DataSet.Add(partner.Genes[i]);
            }
            for(int i = 0; i < 5; ++i)
            {
                child1[2].DataSet.Add(partner.Genes[i]);
                child2[2].DataSet.Add(Genes[i]);
            }
        }
        else
        {
            child1.Add(new GeneticData());
            child2.Add(new GeneticData());
            for (int i = 0; i < 5; ++i)
            {
                child1[2].DataSet.Add(partner.Genes[i]);
                child2[2].DataSet.Add(Genes[i]);
            }
            for (int i = 0; i < 5; ++i)
            {
                child1[2].DataSet.Add(Genes[i]);
                child2[2].DataSet.Add(partner.Genes[i]);
            }
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(MaxHeight));
            child2.Add(new GeneticData(partner.MaxHeight));
        }
        else
        {
            child1.Add(new GeneticData(partner.MaxHeight));
            child2.Add(new GeneticData(MaxHeight));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(waterLevel));
            child2.Add(new GeneticData(partner.waterLevel));
        }
        else
        {
            child1.Add(new GeneticData(partner.waterLevel));
            child2.Add(new GeneticData(waterLevel));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(xFreq));
            child2.Add(new GeneticData(partner.xFreq));
        }
        else
        {
            child1.Add(new GeneticData(partner.xFreq));
            child2.Add(new GeneticData(xFreq));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(zFreq));
            child2.Add(new GeneticData(partner.zFreq));
        }
        else
        {
            child1.Add(new GeneticData(partner.zFreq));
            child2.Add(new GeneticData(zFreq));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(waveLength));
            child2.Add(new GeneticData(partner.waveLength));
        }
        else
        {
            child1.Add(new GeneticData(partner.waveLength));
            child2.Add(new GeneticData(waveLength));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(exponent));
            child2.Add(new GeneticData(partner.exponent));
        }
        else
        {
            child1.Add(new GeneticData(partner.exponent));
            child2.Add(new GeneticData(exponent));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(mountainLevel));
            child2.Add(new GeneticData(partner.mountainLevel));
        }
        else
        {
            child1.Add(new GeneticData(partner.mountainLevel));
            child2.Add(new GeneticData(mountainLevel));
        }
        AssignData(child1);
        children.Add(GetComponent<Map>());
        AssignData(child2);
        children.Add(GetComponent<Map>());
        return children;*/
    }

    public List<IChromosome> CrossOver(Settlement a) { return null; }


    

    public void Mutate(float mutationRate)
    {
        throw new Exception();
        /*if(UnityEngine.Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[0].DataSet[0] = GAFunctions.FlipACoin();
        }
        if(UnityEngine.Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[1].DataSet[0] = GAFunctions.FlipACoin();
        }
        for (int i = 0; i < 10; ++i)
        {
            if (UnityEngine.Random.Range(0, 1f) <= mutationRate)
            {
                GeneData[2].DataSet[i] = UnityEngine.Random.Range(10, 80);
            }
        }
        if (UnityEngine.Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[3].DataSet[0] = UnityEngine.Random.Range(15f, 40f);
        }
        if (UnityEngine.Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[4].DataSet[0] = UnityEngine.Random.Range(0.0f, 0.4f);
        }
        if (UnityEngine.Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[5].DataSet[0] = UnityEngine.Random.Range(0.005f, 0.04f);
        }
        if (UnityEngine.Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[6].DataSet[0] = UnityEngine.Random.Range(0.005f, 0.04f);
        }
        if(UnityEngine.Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[7].DataSet[0] = UnityEngine.Random.Range(1.3f, 5f);
        }
        if (UnityEngine.Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[8].DataSet[0] = UnityEngine.Random.Range(1f, 4);
        }
        if (UnityEngine.Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[9].DataSet[0] = UnityEngine.Random.Range(0.65f, 0.95f);
        }*/
    }

    

    public void SetWater()
    {
        /*MapParameters.MP.Water.transform.position = new Vector3(transform.position.x - 76, waterHeight, transform.position.z - 76);*/
    }

    void OnDestroy()
    {
        /*//settleViableArea.Clear();
        if(settleViableArea != null)
            Array.Clear(settleViableArea, 0, settleViableArea.Length);
        settlementTris.Clear();
        if(verts != null)
            Array.Clear(verts, 0, verts.Length);
        tris.Clear();
        //uvs.Clear();
        if(uvs != null)
            Array.Clear(uvs, 0, Width * Width);
        if (uv2s != null)
            Array.Clear(uv2s, 0, Width * Width);
        if (uv3s != null)
            Array.Clear(uv3s, 0, Width * Width);
        if (uv4s != null)
            Array.Clear(uv4s, 0, Width * Width);

        if (ret != null)
            if(ret.name != "null")
                ret.Clear();*/
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    /*public List<float> GetFitnessVariables()
    {
        
        /*return new List<float>
        {
            MountainousVertices,
            UnderwaterVertices,
            GrasslandVertices, 
            numberOfPeaks
        };#1#
    }*/

    public List<float> GetExpectedFitnesses()
    {
        throw new Exception();
    }

    public void Initialise(ExpressedMapData mapData, string name)
    {
        TerrainMaterials = MaterialController.MC.Materials;
        
        ret = new Mesh();
        ret.SetVertices(mapData.Verts.Select(x => x.Location).ToList());
        //ret.SetUVs(1, mapData.Verts.Select(x => x.UVy).ToList());
        ret.SetTriangles(mapData.Tris.ToArray(), 0);
        ret.RecalculateNormals();
        ret.RecalculateTangents();
        ret.RecalculateBounds();
        /*ret.uv = new Vector2[mapData.VertexCount]; /*mapData.Verts.Select(x => x.UV1).ToArray();#1#
        ret.uv2 = new Vector2[mapData.VertexCount];/*mapData.Verts.Select(x => x.UV2).ToArray();#1#
        ret.uv3 = new Vector2[mapData.VertexCount]; /*mapData.Verts.Select(x => x.UV3).ToArray();#1#
        ret.uv4 = new Vector2[mapData.VertexCount];*/ /*mapData.Verts.Select(x => x.UV4).ToArray();*/
        var uv1s = new List<Vector2>(mapData.VertexCount);
        var uv2s = new List<Vector2>(mapData.VertexCount);
        var uv3s = new List<Vector2>(mapData.VertexCount);
        var uv4s = new List<Vector2>(mapData.VertexCount);

        var width = 255;
        var arrayLength = width * width;
        for (var i = 0; i < arrayLength; i++)
        {
            uv1s.Add(new Vector2());
            uv2s.Add(new Vector2());
            uv3s.Add(new Vector2());
            uv4s.Add(new Vector2());
        }

        foreach (var vert in mapData.Verts)
        {
            uv1s[(vert.XIndex * width) + vert.YIndex] = vert.UV1;
            uv2s[(vert.XIndex * width) + vert.YIndex] = vert.UV2;
            uv3s[(vert.XIndex * width) + vert.YIndex] = vert.UV3;
            uv4s[(vert.XIndex * width) + vert.YIndex] = vert.UV4;
        }

        ret.uv = uv1s.ToArray();
        ret.uv2 = uv2s.ToArray();
        ret.uv3 = uv3s.ToArray();
        ret.uv4 = uv4s.ToArray();

        gameObject.name = name;
        gameObject.GetComponent<MeshFilter>().mesh = ret;

        gameObject.GetComponent<MeshRenderer>().material = TerrainMaterials[0];
        
        /*ret.SetUVs(0, mapData.Verts.Select(x => x.UV1).ToList());
        ret.SetUVs(1, mapData.Verts.Select(x => x.UV2).ToList());
        ret.SetUVs(2, mapData.Verts.Select(x => x.UV3).ToList());
        ret.SetUVs(3, mapData.Verts.Select(x => x.UV4).ToList());*/
        /*Name = name;*/
    }
}

public struct PointIndex
{
    public Vector3 point;
    public int Index;

    public PointIndex(Vector3 _Point, int _Index)
    {
        point = _Point;
        Index = _Index;
    }
}

public class Triangle
{
    public PointIndex First;
    public PointIndex Second;
    public PointIndex Third;

    public Vector3 Normal;

    List<Vector3> lines = new List<Vector3>();

    public Triangle(PointIndex one, PointIndex two, PointIndex three)
    {
        First = one;
        Second = two;
        Third = three;
        CalculateLines();
        CalculateNormal();
    }

    public bool IsValid(float validityDistance)
    {
        if((Vector3.Distance(First.point, Second.point) < validityDistance) && (Vector3.Distance(Second.point, Third.point) < validityDistance) && (Vector3.Distance(Third.point, Second.point) < validityDistance))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsWithinShearTolerance(float tolerance)
    {
        Normal.Normalize();
        float shearAngle = Mathf.Abs(Vector3.Dot(Vector3.up, Normal));
        if(shearAngle < tolerance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void CalculateLines()
    {
        lines.Add(First.point - Second.point);
        lines.Add(Second.point - Third.point);
        lines.Add(Third.point - First.point);
    }

    void CalculateNormal()
    {
        Normal = Vector3.Cross(lines[0], lines[1]);
    }
}