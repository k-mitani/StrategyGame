using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Country
{
    
}

public class Force
{
    public Cell Location { get; set; }
    public Character Commander { get; set; }
    public Country Country => Commander.country;
    public MarchAction.MarchActionTarget Target { get; set; }
}
