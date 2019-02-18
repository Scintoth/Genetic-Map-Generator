using UnityEngine;
using System.Collections;

public class MapParameters : MonoBehaviour {
    public static MapParameters MP;


    public float Underwater = 0.33f;
    public float Mountainous = 0.33f;
    public float Grassland = 0.33f;
    [Range(3, 20)]
    public int TargetPeaks = 3;

    public GameObject Water;

    void Awake()
    {
        MP = this;
    }

    void Update()
    {
        float total = Grassland + Mountainous + Underwater;
        Underwater /=  total;
        Mountainous /= total;
        Grassland /= total; 
    }
}
