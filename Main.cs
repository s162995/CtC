using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
    	// Instantiate map
        Layouts layouts = new Layouts();
        char[,] layout = layouts.Layout13;

        // Pass map to the board manager to create level.
        Manager.Instance.CreateBoard(layout);
        Manager.Instance.Run();
    }
}