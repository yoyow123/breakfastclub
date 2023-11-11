using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;

[Serializable]
public class TagsConfigs
{
    public string name;
    public Dictionary<string, string> Tags;

    public TagsConfigs(SerializableTagsConfig sc) {
        name = sc.name;
        Tags = List2Dict(sc.Tags);
    }
    private Dictionary<string, string> List2Dict(NamedConfigStrValue[] list)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        list.ToList().ForEach(x => dict.Add(x.field, x.value));
        return dict;
    }
}

[Serializable]
//json structure
public struct SerializableTagsConfig {
    public string name;
    public NamedConfigStrValue[] Tags;
}

[Serializable]
public struct NamedConfigStrValue
{
    public string field;
    public string value;
    public NamedConfigStrValue(string field, string value)
    {
        this.field = field;
        this.value = value;
    }
}
