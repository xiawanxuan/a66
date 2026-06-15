using UnityEngine;

[CreateAssetMenu(fileName = "SimConfigAsset", menuName = "ArcSim/SimConfig")]
public class SimConfigAsset : ScriptableObject
{
    public SimConfig config = new SimConfig();

    public static SimConfigAsset LoadFromRuntime()
    {
        var asset = CreateInstance<SimConfigAsset>();
        TextAsset jsonFile = Resources.Load<TextAsset>("Config/SimConfig");
        if (jsonFile != null)
        {
            JsonUtility.FromJsonOverwrite(jsonFile.text, asset.config);
        }
        return asset;
    }

    public void SaveToJSON(string path)
    {
        string json = JsonUtility.ToJson(config, true);
        System.IO.File.WriteAllText(path, json);
    }
}
