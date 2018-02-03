using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Settlement settlement; 
    public float Area;
    public float Prosperity;
    public List<Citizen> Citizens;
    public Citizen CitizenTemplate;

    public void Initialise(Vector3 scale, Settlement belongsTo)
    {
        settlement = belongsTo;
        Area = scale.x * scale.z;   
        settlement.unusedArea -= Area;
        Prosperity = Mathf.Sqrt(Area);
        settlement.UnusedProsperity -= Prosperity;
        transform.localScale = scale;
        int numberOfCitizens = Random.Range(0, 5);
        if (settlement.TargetNumInhabitants > 0)
        {
            for (int i = 0; i < numberOfCitizens; ++i)
            {
                Citizens.Add(Instantiate(CitizenTemplate, transform.position, new Quaternion()));
                Citizens[i].transform.SetParent(transform, true);
                settlement.UnusedInhabitants--;
            }
        }
    }

	// Use this for initialization
	void Start ()
    {
        
    }

    public void FindFloor()
    {
        transform.position += new Vector3(0, 30, 0);
        RaycastHit info;
        Ray downRay = new Ray(transform.position, -Vector3.up * 100);
        Ray upRay = new Ray(transform.position, Vector3.up * 100);
        if (Physics.Raycast(downRay, out info))
        {
            int safety = 1000;
            while(info.collider.gameObject.name != "Map" && safety > 0)
            {
                transform.Translate(Random.insideUnitSphere * Random.Range(2f,4f));
                Physics.Raycast(downRay, out info);
                //Debug.Log(gameObject.name + " hit point = " + info.point + " on " + info.collider.gameObject.name);
                safety--;
            }
            transform.position = info.point;
        }

    }

    void OnDestroy()
    {
        for(int i = 0; i < Citizens.Count; ++i)
        {
            Destroy(Citizens[i].gameObject);
        }
        Citizens.Clear();
    }
}
