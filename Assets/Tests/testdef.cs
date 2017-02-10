public class DynamicObjectItem
{
    public string a;
    public int b;

    public DynamicObjectItem(string text)
    {
        var texts = text.Split(',');
        a = texts[0];
        b = int.Parse(texts[1]);
    }
}