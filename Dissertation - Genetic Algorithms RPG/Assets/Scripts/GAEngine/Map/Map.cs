using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Assets.Scripts.GAEngine.Map;
using Zenject;
using Assets.Entities;

public class Map : MonoBehaviour, IChromosome
{
    [Inject]
    public IHeightMapGenerator _heightmapGenerator;
    
    [Header("Can be used to create lakes/mesas")]
    public bool FlattenTerrain = true;
    [Header("Used to add additional hills to the tops of mesas")]
    public bool AdditionalHills = true;

    public List<GeneticData> GeneData { get; set; } = new List<GeneticData>();

    public List<int> Genes { get; set; } = new List<int>();

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

    
    public float Fitness
    {
        get;
        set;
    }

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

    List<Vector3[]> verts = new List<Vector3[]>();
    public List<List<Vector3>> settleViableArea = new List<List<Vector3>>();
    List<int> settlementTris = new List<int>();
    List<Triangle> settlementTriangles = new List<Triangle>();
    public Mesh settleMesh;
    List<Vector2> potentialPeaks = new List<Vector2>();
    List<int> tris = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    List<Vector2> uv2s = new List<Vector2>();
    List<Vector2> uv3s = new List<Vector2>();
    List<Vector2> uv4s = new List<Vector2>();

    int settlementTriCount = 0;
    
    public IChromosome GenerateGene(int length)
    {
        Water = MapParameters.MP.Water;
        GeneData.Clear();
        FlattenTerrain = true;
        GeneData.Add(new GeneticData(FlattenTerrain));
        AdditionalHills = true;
        GeneData.Add(new GeneticData(AdditionalHills));
        Genes = new List<int>();
        GeneData.Add(new GeneticData());
        for (int i = 0; i < length; ++i)
        {
            Genes.Add(Random.Range(10, 80));
            GeneData[2].DataSet.Add(Genes[i]);
        }
        MaxHeight = Random.Range(15f, 40f);
        GeneData.Add(new GeneticData(MaxHeight));
        waterLevel = Random.Range(0, 0.4f);
        GeneData.Add(new GeneticData(waterLevel));
        xFreq = Random.Range(0.005000f, 0.04000f);
        GeneData.Add(new GeneticData(xFreq));
        zFreq = xFreq;
        GeneData.Add(new GeneticData(zFreq));
        waveLength = Random.Range(1.3f, 5f);
        GeneData.Add(new GeneticData(waveLength));
        exponent = Random.Range(1f, 4f);
        GeneData.Add(new GeneticData(exponent));
        mountainLevel = Random.Range(0.65f, 0.95f);
        GeneData.Add(new GeneticData(mountainLevel));
        GenerateMesh();
        return this;
    }

    public void AssignData(List<GeneticData> dataToAdd)
    {
        Water = MapParameters.MP.Water;
        FlattenTerrain = (bool)dataToAdd[0].DataSet[0];
        AdditionalHills = (bool)dataToAdd[1].DataSet[0];
        Genes.Clear();
        
        for (int i = 0; i < dataToAdd[2].DataSet.Count; ++i)
        {
            Genes.Add((int)dataToAdd[2].DataSet[i]);
        }
        MaxHeight = (float)dataToAdd[3].DataSet[0];
        waterLevel = (float)dataToAdd[4].DataSet[0];
        xFreq = (float)dataToAdd[5].DataSet[0];
        zFreq = (float)dataToAdd[6].DataSet[0];
        waveLength = (float)dataToAdd[7].DataSet[0];
        exponent = (float)dataToAdd[8].DataSet[0];
        mountainLevel = (float)dataToAdd[9].DataSet[0];
        GenerateMesh();
        SetWater();
    }
    
    public void Initialise()
    {
        Water = MapParameters.MP.Water;
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
        gameObject.tag = "Map";
    }

    public void GenerateMesh()
    {
        numberOfPeaks = 0;
        Water = MapParameters.MP.Water;
        totalVertexHeights = 0;
        TotalVertices = 0;
        MountainousVertices = 0;
        UnderwaterVertices = 0;
        GrasslandVertices = 0;
        ceiling = MaxHeight * 5;
        if(GetComponent<MeshFilter>() != null)
            GetComponent<MeshFilter>()?.mesh.Clear();
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
        /*var unwoundVertices = new List<Vector3>(); //new Vector3[Width * Width];
        //int l = 0;
        foreach (Vector3[] v in heightMap.Verts)
        {
            unwoundVertices.AddRange(v);
            //v.CopyTo(unwoundVertices., l * Width);
            //l++;
        }
        ret.SetVertices(unwoundVertices);*/
        //ret.SetTriangles(heightMap.Tris.ToArray(), 0);
        verts = heightMap.Verts;
        ret.triangles = heightMap.Tris.ToArray();
        tris = heightMap.Tris;
        waterHeight = heightMap.WaterHeight;
        highestVertex = heightMap.HighestVertex;
        mountainHeight = heightMap.MountainHeight;
        totalVertexHeights = heightMap.VertexCount;
        //GenerateHeightMap();

        // This is where we go over an additional loop of the whole array to calculate peaks and shears
        CalculatePeaksAndSettlements();
        
        //SetWater();
        highestVertex = 0;

        AverageVertexHeight = heightMap.SumOfAllVertexHeights / Mathf.Pow(Width, 2);

        // Unfold the 2d array of verticies into a 1d array.
        Vector3[] unfoldedVerts = new Vector3[Width * Width];
        int i = 0;
        foreach (Vector3[] v in verts)
        {
            v.CopyTo(unfoldedVerts, i * Width);
            i++;
        }

        Vector3[] settlementUnfoldedVerts = new Vector3[settleViableArea.Count * settleViableArea.Count];
        int k = 0;
        foreach(List<Vector3> v in settleViableArea)
        {
            v.CopyTo(settlementUnfoldedVerts, k * settleViableArea.Count);
            k++;
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
        //TerrainMesh.GetComponent<MeshRenderer>().material = TerrainMaterials[0];

        // Clear no longer needed data

        settlementTriCount = 0;
        unfoldedVerts = null;
        settlementUnfoldedVerts = null;
        settlementTriangles.Clear();
        settlementTriangles = null;
        //ret.Clear();
        if (verts.Count > 0)
        {
            for (int j = 0; j < Width; ++j)
            {
                verts[j] = null;
            }
        }
        verts.Clear();
        for(int j = 0; j < settleViableArea.Count; ++j)
        {
            settleViableArea[j] = null;
        }
        potentialPeaks.Clear();
        settleViableArea.Clear();
        tris.Clear();
        settlementTris.Clear();
        uvs.Clear();
        uv2s.Clear();
        uv3s.Clear();
        uv4s.Clear();
    }

    void GenerateHeightMap()
    {
        float zOff = 0;
        for (int z = 0; z < Width; z++)
        {
            float xOff = 0;
            verts.Add(new Vector3[Width]);
            for (int x = 0; x < Width; x++)
            {

                Vector3 currentPoint = new Vector3
                {
                    x = (x * Spacing - Width), // / 2f;
                    z = (z * Spacing - Width) // / 2f;
                };

                float e = GetHeight(currentPoint.x, currentPoint.z);
                currentPoint.y = Mathf.Pow(e, exponent);

                if (FlattenTerrain)
                {
                    currentPoint.y = Mathf.Clamp(currentPoint.y, -MaxHeight, MaxHeight) * (GetHeight(currentPoint.x, currentPoint.z) * 0.3f);
                }
                if (AdditionalHills && FlattenTerrain)
                {
                    if (Mathf.Pow(e, exponent) > MaxHeight * 3)
                    {
                        currentPoint.y += (Mathf.Pow(e, exponent) - (MaxHeight * ((exponent + waveLength) / 2) - 3)) / (MaxHeight);
                    }
                    if (currentPoint.y >= ceiling)
                    {
                        currentPoint.y = Mathf.Clamp(currentPoint.y, MaxHeight * 4, ceiling);
                    }
                }

                totalVertexHeights += currentPoint.y;

                verts[z][x] = currentPoint;

                if (currentPoint.y >= highestVertex)
                {
                    highestVertex = currentPoint.y;
                }

                waterHeight = waterLevel * highestVertex;
                mountainHeight = mountainLevel * highestVertex;


                // Don't generate a triangle if it would be out of bounds.
                int current_x = x;
                if (current_x - 1 <= 0 || z <= 0 || current_x >= Width)
                {
                    continue;
                }

                tris.Add(x + z * Width);
                tris.Add(current_x + (z - 1) * Width);
                tris.Add((current_x - 1) + (z - 1) * Width);

                if (x - 1 <= 0 || z <= 0)
                {
                    continue;
                }
                tris.Add(x + z * Width);
                tris.Add((current_x - 1) + (z - 1) * Width);
                tris.Add((x - 1) + z * Width);
                xOff += x / Width - 0.5f;
                TotalVertices++;
            }
            zOff += z / Width - 0.5f;
        }
    }

    void CalculatePeaksAndSettlements()
    {
        settlementTriCount = 0;
        settlementTriangles = new List<Triangle>();
        

        // for each x
        for(int i = 0; i < verts.Count; ++i)
        {
            //settleViableArea.Add(new Vector3[Width]);
            // for each y
            settleViableArea.Add(new List<Vector3>());
            for(int j = 0; j < verts[i].Length  ; j++)
            {
                //Mountainous
                if (verts[i][j].y > (mountainHeight))
                {
                    uvs.Add(new Vector2(0.5f, 1.0f));
                    uv2s.Add(new Vector2(1.0f, 1.0f));
                    uv3s.Add(new Vector2(0.5f, 0.5f));
                    uv4s.Add(new Vector2(1.0f, 0.5f));
                    MountainousVertices++;
                }
                //Underwater
                else if (verts[i][j].y < waterHeight)
                {
                    UnderwaterVertices++;
                    uvs.Add(new Vector2(0.0f, 0.5f));
                    uv2s.Add(new Vector2(0.25f, 0.5f));
                    uv3s.Add(new Vector2(0.0f, 0.25f));
                    uv4s.Add(new Vector2(0.25f, 0.25f));
                }
                //Grassland
                else if (verts[i][j].y >= waterHeight && verts[i][j].y <= mountainHeight)
                {
                    GrasslandVertices++;
                    uvs.Add(new Vector2(0.0f, 1.0f));
                    uv2s.Add(new Vector2(0.5f, 1.0f));
                    uv3s.Add(new Vector2(0f, 0.5f));
                    uv4s.Add(new Vector2(0.5f, 0.5f));
                }
                if (verts[i][j].y >= mountainHeight)
                {
                    potentialPeaks.Add(new Vector2(i, j));
                }
                
                //Vector3 averageVertex = new Vector3();
                if ((verts[i][j].y < mountainHeight) && (verts[i][j].y > waterHeight))
                {
                    if ((i + 1 < Width) && (i - 1 > 0) && (j + 1 < Width) && (j - 1 > 0))
                    {
                        {
                            int current_x = i;
                            if (current_x - 1 <= 0 || j <= 0 || current_x >= Width)
                            {
                                continue;
                            }
                            Triangle tempTri = new Triangle(new PointIndex(verts[i][j], i + j * Width),
                                                            new PointIndex(verts[i][j + 1], current_x + (j - 1) * Width),
                                                            new PointIndex(verts[i + 1][j], (current_x - 1) + (j - 1) * Width));
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
                                continue;
                            }
                            tempTri = new Triangle(new PointIndex(verts[i][j], i + j * Width),
                                                   new PointIndex(verts[i][j + 1], (current_x - 1) + (j - 1) * Width),
                                                   new PointIndex(verts[i + 1][j], (i - 1) + j * Width));
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
        }

        List<Vector3> calculatedPeaks = new List<Vector3>();

        // for each x + k
        int searchRange = 10;
        for (int i = 0; i < potentialPeaks.Count; ++i)
        {
            Vector2 highestIndex = new Vector2();
            float highestPoint = 0;
            for (int k = -searchRange; k < searchRange; ++k)
            {
                for (int l = -searchRange; l < searchRange; ++l)
                {
                    if ((potentialPeaks[i].x + k < Width) && (potentialPeaks[i].x + k > 0) && (potentialPeaks[i].y + l < Width) && (potentialPeaks[i].y + l > 0))
                    {
                        if (verts[(int)potentialPeaks[i].x + k][(int)potentialPeaks[i].y + l].y > highestPoint)
                        {
                            highestPoint = verts[(int)potentialPeaks[i].x + k][(int)potentialPeaks[i].y + l].y;
                            highestIndex.x = (int)potentialPeaks[i].x + k;
                            highestIndex.y = (int)potentialPeaks[i].y + l;
                        }
                    }
                }
            }

            calculatedPeaks.Add(verts[(int)highestIndex.x][(int)highestIndex.y]);
        }
        calculatedPeaks.Sort((a, b) => a.y.CompareTo(b.y));
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
        numberOfPeaks = calculatedPeaks.Count;
    }

    float GetHeight(float x, float z)
    {
        int leftGeneSum = 0;
        int rightGeneSum = 0;
        for (int i = 0; i < Genes.Count; ++i)
        {
            if (i < (Genes.Count / 2))
            {
                leftGeneSum += Genes[i];
            }
            else
            {
                rightGeneSum += Genes[i];
            }
        }
        float returnVal = waveLength * Mathf.PerlinNoise(xFreq * (x + leftGeneSum), zFreq * (z + rightGeneSum));
        for (int i = 1; i < octaves; i++)
        {
            returnVal += (1 / Mathf.Pow(2, i)) * waveLength * Mathf.PerlinNoise(i * Mathf.Sin(xFreq) * (x - leftGeneSum), (i * Mathf.Sin(zFreq)) * (z - rightGeneSum));
        }
        return returnVal;
    }

    // Update is called once per frame
    void Update()
    {
        CalculateFitness();
        fitValue = Fitness;
        //GenerateMesh();
    }

    public void CalculateFitness()
    {
        float temp = TotalVertices;
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
        Fitness = GAFunctions.Average(mountFitness, grassFitness, underwaterFitness, peakFitness);
    }

    public List<IChromosome> CrossOver(IChromosome partner)
    {
        List<IChromosome> children = new List<IChromosome>();
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
        return children;
    }

    public List<IChromosome> Crossover<Map>(Map a)
    {
        return new List<IChromosome>();
    } 

    public List<IChromosome> CrossOver(Map partner)
    {
        List<IChromosome> children = new List<IChromosome>();
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
        return children;
    }

    public List<IChromosome> CrossOver(Settlement a) { return null; }


    

    public void Mutate(float mutationRate)
    {
        if(Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[0].DataSet[0] = GAFunctions.FlipACoin();
        }
        if(Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[1].DataSet[0] = GAFunctions.FlipACoin();
        }
        for (int i = 0; i < 10; ++i)
        {
            if (Random.Range(0, 1f) <= mutationRate)
            {
                GeneData[2].DataSet[i] = Random.Range(10, 80);
            }
        }
        if (Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[3].DataSet[0] = Random.Range(15f, 40f);
        }
        if (Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[4].DataSet[0] = Random.Range(0.0f, 0.4f);
        }
        if (Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[5].DataSet[0] = Random.Range(0.005f, 0.04f);
        }
        if (Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[6].DataSet[0] = Random.Range(0.005f, 0.04f);
        }
        if(Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[7].DataSet[0] = Random.Range(1.3f, 5f);
        }
        if (Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[8].DataSet[0] = Random.Range(1f, 4);
        }
        if (Random.Range(0, 1f) <= mutationRate)
        {
            GeneData[9].DataSet[0] = Random.Range(0.65f, 0.95f);
        }
    }

    

    public void SetWater()
    {
        MapParameters.MP.Water.transform.position = new Vector3(transform.position.x - 76, waterHeight, transform.position.z - 76);
    }

    void OnDestroy()
    {
        settleViableArea.Clear();
        settlementTris.Clear();
        verts.Clear();
        tris.Clear();
        uvs.Clear();
        if(ret != null)
            if(ret.name != "null")
                ret.Clear();
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public List<float> GetFitnessVariables()
    {
        return new List<float>
        {
            MountainousVertices,
            UnderwaterVertices,
            GrasslandVertices, 
            numberOfPeaks
        };
    }

    public List<float> GetExpectedFitnesses()
    {
        return new List<float>
        {
            MapParameters.MP.Mountainous,
            MapParameters.MP.Underwater,
            MapParameters.MP.Grassland,
            MapParameters.MP.TargetPeaks
        };
    }

    public void Initialise(string name)
    {
        Name = name;
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