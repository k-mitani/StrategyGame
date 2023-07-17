using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class TODO
{
    public static TODO Instance { get; private set; }

    internal void StartBattle(Character actor, Character target)
    {
        throw new NotImplementedException();
    }
}