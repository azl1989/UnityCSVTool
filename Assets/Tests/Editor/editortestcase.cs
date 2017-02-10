using UnityEngine;
using UnityEditor;
using System.Collections;
using NUnit.Framework;

public class CSVTests
{
    [Test]
    public void SimpleTests()
    {
        var content = CSV.Parser.Run(System.IO.File.ReadAllText("Assets/Tests/test.csv.txt"), ',');

        Assert.AreEqual(content.Length, 4);
        Assert.AreEqual(content[0][0].Value, 1);
        Assert.AreEqual(content[0][1].Value, "Joy");
        Assert.AreEqual(content[0][4].Value, CSV.Parser.CollectionType.Data);
        Assert.AreEqual(content[0]["dynamic"].To<DynamicObjectItem>().a, "aaa");
        Assert.AreEqual(content[0]["dynamic"].To<DynamicObjectItem>().b, 999);

        Assert.AreEqual(content[0]["id"].Value, 1);
        Assert.AreEqual(content[0]["name"].Value, "Joy");
        Assert.AreEqual(content[0]["enum"].Value, CSV.Parser.CollectionType.Data);

        Assert.AreEqual(content[2]["id"].Value, 0);
        Assert.AreEqual(content[3]["enum"].Value, CSV.Parser.CollectionType.Header);
    }
}