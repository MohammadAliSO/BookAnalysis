using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Analysis.Models
{

    public class Aggregations
    {
        public Query query { get; set; }
    }

    public class Aggregations2
    {
        public Aggs aggs { get; set; }
    }

    public class Aggs
    {
        public int doc_count_error_upper_bound { get; set; }
        public int sum_other_doc_count { get; set; }
        public List<Bucket> buckets { get; set; }
    }
    public class Bucket
    {
        public string key { get; set; }
        public int doc_count { get; set; }
    }

    public class Hits
    {
        public Total total { get; set; }
        public object max_score { get; set; }
        public List<object> hits { get; set; }
    }

    public class Query
    {
        public int doc_count_error_upper_bound { get; set; }
        public int sum_other_doc_count { get; set; }
        public List<Bucket> buckets { get; set; }
    }

    public class GroupAllFieldValue
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public Shards _shards { get; set; }
        public Hits hits { get; set; }
        public Aggregations aggregations { get; set; }
    }

    public class Shards
    {
        public int total { get; set; }
        public int successful { get; set; }
        public int skipped { get; set; }
        public int failed { get; set; }
    }

    public class Total
    {
        public int value { get; set; }
        public string relation { get; set; }
    }

    public class MoreLikeThisQueryResponse
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public Shards _shards { get; set; }
        public Hit hits { get; set; }
    }
    public class Hit
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public double _score { get; set; }
        public Source _source { get; set; }
        public Total total { get; set; }
        public double? max_score { get; set; }
        public List<Hit>? hits { get; set; }
    }





    public class Source
    {
        public string bookname { get; set; }
        //public string header { get; set; }
        public string topic { get; set; }
        //public string header_topic { get; set; }
        public string content { get; set; }
        public DateTime publishdate { get; set; }
        public DateTime eventdate { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    
   

    public class GroupAllFieldByFilterValue
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public Shards _shards { get; set; }
        public Hits hits { get; set; }
        public Aggregations2 aggregations { get; set; }
    }




}
