namespace Book_Analysis.Models;

public class ConfigModels
{
    public ElasticSearchConfigModel ElasticSearch { get; set; }
}

public class ElasticSearchConfigModel
{
    public string Address { get; set; }
    //public string Index { get; set; }
}