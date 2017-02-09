using UnityEngine;
using System.Collections;

public class csvtests : MonoBehaviour 
{
    private void Start()
    {
        var content = CSV.Parser.Run(System.IO.File.ReadAllText("Assets/Tests/test.csv.txt"), ',');

        for (var i = 0; i < content.Length; ++i)
        {
            var text = "";

            for (var j = 0; j < content[i].Length; ++j)
            {
                text += content[i][j].ToString() + ((j == content[i].Length - 1) ? "" : ",");
            }

            UnityEngine.Debug.Log(text);
        }
    }
}