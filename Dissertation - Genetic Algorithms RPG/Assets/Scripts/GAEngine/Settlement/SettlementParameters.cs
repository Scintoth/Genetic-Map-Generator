using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlementParameters : MonoBehaviour {
    public static SettlementParameters SP;

    [Range(3, 30)]
    public float TargetArea;
    [Range(1, 20)]
    public int TargetNumBuildings;
    [Range(5, 40)]
    public int TargetInhabitants;
    [Range(3, 10)]
    public int TargetProsperity;
    

    public Building BuildingTemplate;

	// Use this for initialization
	void Awake ()
    {
        SP = this;
	}
}
