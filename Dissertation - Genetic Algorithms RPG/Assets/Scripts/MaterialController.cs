using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialController : MonoBehaviour
{
    public static MaterialController MC;

    public List<Material> Materials = new List<Material>();

    public void Awake()
    {
        MC = this;
    }
}
