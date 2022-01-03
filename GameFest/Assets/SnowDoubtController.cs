using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowDoubtController : GenericController
{
    public TopDownMovement[] PlayerMovements;

    public static SnowDoubtController Instance;

    private void Start()
    {
        Instance = this;
    }
}
