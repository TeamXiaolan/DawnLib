using System.Collections.Generic;

namespace CodeRebirthLib.Data;

public class ThunderstoreManifest()
{
    public string author_name { get; set; }
    public string name { get; set; }
    public string version_number { get; set; }
    public string description { get; set; }
    public string website_url { get; set; }
    public List<string> dependencies { get; set; }
}