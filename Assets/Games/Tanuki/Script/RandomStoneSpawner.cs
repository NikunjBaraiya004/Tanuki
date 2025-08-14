using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace nostra.booboogames.slapcastle
{
    
    public class RandomStoneSpawner : MonoBehaviour
    {

        [SerializeField] List<GameObject> stonelist;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            int randomstonenumber = Random.Range(0, stonelist.Count);

            for (int i = 0; i < stonelist.Count; i++)
            {
                stonelist[i].gameObject.SetActive(false);

                if (i == randomstonenumber)
                {
                    stonelist[i].gameObject.SetActive(true);
                }
            }
        }
    
        // Update is called once per frame
        void Update()
        {
            
        }
    }
    
}