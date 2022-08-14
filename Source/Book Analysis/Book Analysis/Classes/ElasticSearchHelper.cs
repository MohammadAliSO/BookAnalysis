using Book_Analysis.Models;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Concurrent;

namespace Book_Analysis.Classes
{
    public static class ElasticSearchHelper
    {
        public static ElasticClient CreateConnection()
        {
            var node = new Uri(Config.All.ElasticSearch.Address);
            var settings = new ConnectionSettings(node);
            return new ElasticClient(settings);
        }



        public static Dictionary<string, BookInfoModel> SearchAlltt(string indexname)
        {

            ConcurrentDictionary<string, BookInfoModel> result = new ConcurrentDictionary<string, BookInfoModel>();


            Time processTimePerScroll = "20s";
            int numberOfSlices = Environment.ProcessorCount;
            var elasticClient = CreateConnection();

            var scrollAllObservable = elasticClient.ScrollAll<BookInfoModel>(processTimePerScroll, numberOfSlices, sc => sc
                .MaxDegreeOfParallelism(numberOfSlices)
                .Search(s => s.Index(indexname)
                    .Query(q => q
                        .MatchAll()
                    )
                )
            );

            var waitHandle = new ManualResetEvent(false);
            Exception exception = null;

            var scrollAllObserver = new ScrollAllObserver<BookInfoModel>(
                onNext: response =>
                {
                    // do something with the documents
                    var documents = response.SearchResponse.Documents;
                    foreach (var hit in response.SearchResponse.Hits)
                    {
                        result.TryAdd(hit.Id, new BookInfoModel
                        {
                            bookname = hit.Source.bookname,
                            //header = hit.Source.header,
                            publishdate = hit.Source.publishdate,
                            content = hit.Source.content,
                            topic = hit.Source.topic,
                            //header_topic = hit.Source.header_topic,
                            eventdate = hit.Source.eventdate
                        });
                    }
                },
                onError: e =>
                {
                    exception = e;
                    waitHandle.Set();
                },
                onCompleted: () => waitHandle.Set()
            );


            scrollAllObservable.Subscribe(scrollAllObserver);

            waitHandle.WaitOne();

            if (exception != null)
            {
                throw exception;
            }
            

            return result.ToDictionary(a=>a.Key,a=>a.Value);
        }
        public static Dictionary<string, BookInfoModel> SearchAll(string indexname)
        {

            //string url_req = "http://localhost:9200/book/_search";

            //string s = "{\"query\":{\"match_all\":{}}}";
            //var data = new StringContent(s, Encoding.UTF8, "application/json");
            //using (var client = new HttpClient())
            //{
            //   var dd= client.PostAsync(url_req, data);
            //    var rr =dd.Result.Content;
            //    var dfd =rr.ReadAsStringAsync();
            //}
            var elasticClient = CreateConnection();
            var response = elasticClient.Search<BookInfoModel>(s => s.Index(indexname)
            .Query(q => q.MatchAll()));

            Dictionary<string, BookInfoModel> result = new Dictionary<string, BookInfoModel>();

            foreach (var hit in response.Hits)
            {
                result.Add(hit.Id, new BookInfoModel
                {
                    bookname = hit.Source.bookname,
                    //header = hit.Source.header,
                    publishdate = hit.Source.publishdate,
                    content = hit.Source.content,
                    topic = hit.Source.topic,
                    //header_topic = hit.Source.header_topic,
                    eventdate = hit.Source.eventdate
                });
            }

            return result;
        }

        public static List<BookLikeInfoModel> MoreLikeThisQuery(string text)
        {
            Analysis analysis = new Analysis();
            text = analysis.RemovePunctuations(text);
            text = text.Replace("\n","");
            //var elasticClient = CreateConnection();
            //var response = elasticClient.Search<BookInfoModel>(s => s
            //.Index(Global.IndexElastic)
            //.Query(q => q.MoreLikeThis(sn => sn
            ////.Name("named_query")
            ////.Boost(3.1)
            //.BoostTerms(3)
            //.Like(l => l
            //.Text(text))
            //.Analyzer("persian")
            //.BoostTerms(2.0)
            //.Include()
            //.MaxDocumentFrequency(100)
            //.MaxQueryTerms(60)
            ////.MaxWordLength(300)
            ////.MinDocumentFrequency(1)
            //.MinTermFrequency(1)
            ////.MinWordLength(10)
            ////.StopWords("and", "the")
            ////.MinimumShouldMatch(1)

            //.Fields(f => f.Field(p => p.header_topic,2.5)))));
            ////.Unlike(l => l.Text("not like this text")


            var client = new RestClient($"{Config.All.ElasticSearch.Address}/{Global.IndexElastic}/_search");

            var request = new RestRequest("", Method.Get);
            request.AddHeader("Content-Type", "application/json");

            string body = "{\r\n    \"query\": {\r\n        \"dis_max\": {\r\n            \"queries\": [{\r\n                \"more_like_this\": {\r\n                    \"fields\": [\r\n                        \"topic\"\r\n                    ],\r\n                    \"minimum_should_match\": 1,\r\n                    \"min_term_freq\": 1,\r\n                    \"max_query_terms\": 5000,\r\n                    \"min_doc_freq\": 1,\r\n                    \"boost\": 2.0,\r\n                    \"analyzer\": \"persian\",\r\n                    \"include\": true,\r\n                    \"like\": \"@text\"\r\n                }},{\r\n                \"more_like_this\": {\r\n                    \"fields\": [\r\n                        \"content\"\r\n                    ],\r\n                    \"minimum_should_match\": 1,\r\n                    \"min_term_freq\": 1,\r\n                    \"max_query_terms\": 5000,\r\n                    \"min_doc_freq\": 1,\r\n                    \"boost\": 1.0,\r\n                    \"boost_terms\": 2.0,\r\n                    \"analyzer\": \"persian\",\r\n                    \"include\": true,\r\n                    \"like\": \"@text\"\r\n                }}\r\n            ]\r\n        }\r\n    }\r\n}";
            //string body = "{\r\n    \"query\": {\r\n        \"dis_max\": {\r\n            \"queries\": [{\r\n                \"more_like_this\": {\r\n                    \"fields\": [\r\n                        \"header_topic\"\r\n                    ],\r\n                    \"minimum_should_match\": 1,\r\n                    \"min_term_freq\": 1,\r\n                    \"max_query_terms\": 200,\r\n                    \"min_doc_freq\": 1,\r\n                    \"boost\": 1.0,\r\n                    \"boost_terms\": 3.0,\r\n                    \"analyzer\": \"persian\",\r\n                    \"include\": true,\r\n                    \"like\": \"@text\"\r\n                }},{\r\n                \"more_like_this\": {\r\n                    \"fields\": [\r\n                        \"content\"\r\n                    ],\r\n                    \"minimum_should_match\": 1,\r\n                    \"min_term_freq\": 1,\r\n                    \"max_query_terms\": 200,\r\n                    \"min_doc_freq\": 1,\r\n                    \"boost\": 2.0,\r\n                    \"analyzer\": \"persian\",\r\n                    \"include\": true,\r\n                    \"like\": \"@text\"\r\n                }}\r\n            ]\r\n        }\r\n    }\r\n}";
            //string body = "{\r\n    \"query\":{\r\n       \"more_like_this\":{\r\n          \"fields\":[\r\n             \"content\"\r\n             ],\r\n          \"like\":\"@text\",\r\n\r\n          \"min_term_freq\":1,\r\n          \"max_query_terms\":100\r\n       }\r\n    }\r\n  }";
            body = body.Replace("@text", text);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            if (!response.IsSuccessful)
                return null;
            MoreLikeThisQueryResponse res = JsonConvert.DeserializeObject<MoreLikeThisQueryResponse>(response.Content);

            List<BookLikeInfoModel> result = new List<BookLikeInfoModel>();
            foreach (var hit in res.hits.hits)
            {
                result.Add(new BookLikeInfoModel
                {
                    bookname = hit._source.bookname,
                    //header = hit._source.header,
                    publishdate = hit._source.publishdate,
                    content = hit._source.content,
                    topic = hit._source.topic,
                    //header_topic = hit._source.header_topic,
                    eventdate = hit._source.eventdate,
                    score = hit._score,
                    id = hit._id
                });

            }
            return result;

        }

        public static void InsertData(BookInfoModel[] documents)
        {
            //         var documents = new[]
            //{
            //     new BookInfo() { book = "updated" }
            // };
            var elasticClient = CreateConnection();
            //CreateResponse response = new();
            //if (elasticClient.Indices.Exists(Config.All.ElasticSearch.Index).Exists)
            //{
            //    response =  elasticClient.CreateDocument<BookInfoModel>(document);
            //}
            //else
            //{
            //    await elasticClient.IndexAsync(document, idx => idx.Index(Config.All.ElasticSearch.Index));
            //    response =  elasticClient.CreateDocumentAsync<BookInfoModel>(document);
            //}
            var response = elasticClient.Bulk(s => s.Index(Global.IndexElastic).CreateMany(documents).Refresh(Refresh.WaitFor));

        }

        public static void CreateIndex(string name)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Config.All.ElasticSearch.Address);
                var response1 = client.PutAsync($"bookanalysis_{name}", null).Result;
                if (response1.IsSuccessStatusCode)
                {
                    MessageBox.Show($"{name} index created", "CreateIndex", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    using (var client1 = new RestClient($"{Config.All.ElasticSearch.Address}/bookanalysis_{name}/_mapping"))
                    {

                        var request = new RestRequest($"{Config.All.ElasticSearch.Address}/bookanalysis_{name}/_mapping", Method.Post);
                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", "{\r\n  \"properties\": {\r\n    \"content\": {\r\n      \"type\": \"text\",\r\n      \"term_vector\": \"with_positions_offsets_payloads\",\r\n      \"store\": true,\r\n      \"analyzer\": \"persian\"\r\n    },\r\n    \"header\": {\r\n      \"type\": \"text\",\r\n      \"analyzer\": \"persian\",\r\n      \"term_vector\": \"with_positions_offsets_payloads\",\r\n      \"store\": true,\r\n      \"fields\": {\r\n        \"keyword\": {\r\n          \"type\": \"keyword\"\r\n        }\r\n      }\r\n    },\r\n    \"topic\": {\r\n      \"type\": \"text\",\r\n      \"analyzer\": \"persian\",\r\n      \"term_vector\": \"with_positions_offsets_payloads\",\r\n      \"store\": true,\r\n      \"fields\": {\r\n        \"keyword\": {\r\n          \"type\": \"keyword\"\r\n        }\r\n      }\r\n    },\r\n    \"header_topic\": {\r\n      \"type\": \"text\",\r\n      \"analyzer\": \"persian\",\r\n      \"term_vector\": \"with_positions_offsets_payloads\",\r\n      \"store\": true,\r\n      \"fields\": {\r\n        \"keyword\": {\r\n          \"type\": \"keyword\"\r\n        }\r\n      }\r\n    },\r\n    \"releasedate\": {\r\n      \"type\": \"date\"\r\n    },\r\n    \"eventdate\": {\r\n      \"type\": \"date\"\r\n    },\r\n    \"bookname\": {\r\n      \"type\": \"text\",\r\n      \"fields\": {\r\n        \"keyword\": {\r\n          \"type\": \"keyword\"\r\n        }\r\n      }\r\n    }\r\n  }\r\n}", ParameterType.RequestBody);

                        var res = client1.Execute(request);
                    }
                }
                else
                    MessageBox.Show($"{response1.ReasonPhrase}", "CreateIndex", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        public static async Task<string[]> GetIndex()
        {
            var elasticClient = CreateConnection();
            //var result =  await elasticClient.Indices.GetAsync(new GetIndexRequest(Indices.All)).Result;

            var response = await elasticClient.Cat.IndicesAsync(c => c.AllIndices());
            var Indexes = response.Records.Select(a => a.Index).Where(a => a.Contains("bookanalysis_")).ToList();

            for (int i = 0; i < Indexes.Count; i++) Indexes[i] = Indexes[i].Replace("bookanalysis_", "");

            return Indexes.ToArray();
        }
    }
}
