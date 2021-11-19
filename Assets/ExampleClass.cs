using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Docs("This is an example class descriptor")]
public class ExampleClass : MonoBehaviour
{
    [Docs("Something important about this")]
    public int number = 10;
    public string words;

    private int invisible;

    [Docs("This is the documentation of a method")]
    public void PubMethod()
    {
        print("AYEE");
    }

    private void CantSeeMe()
    {
        print("SUS");
    }
}
