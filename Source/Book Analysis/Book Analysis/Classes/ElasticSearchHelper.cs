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

        public static long GetCount(string IndexName, string BookName)
        {
            using (var client = new RestClient($"{Config.All.ElasticSearch.Address}/{IndexName}/_count"))
            {

                var request = new RestRequest($"{Config.All.ElasticSearch.Address}/{IndexName}/_count", Method.Post);
                request.AddHeader("Content-Type", "application/json");

                var body = "{\r\n  \"query\" : {\r\n    \"term\" : { \"bookname.keyword\" : \"@bookname\" }\r\n  }\r\n}";
                body = body.Replace("@bookname", BookName);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                var res = client.Execute(request);

                MoreLikeThisQueryResponse result = JsonConvert.DeserializeObject<MoreLikeThisQueryResponse>(res.Content);


                CountResponse? count = JsonConvert.DeserializeObject <CountResponse?> (res.Content);

            return count.Count;
            }
        }
        public static long GetCount(string Index)
        {
            var elasticClient = CreateConnection();

            var count = elasticClient.Count<BookInfoModel>(a => a.Index(Index));


            return count.Count;
        }
        public static Dictionary<string, double> TermVectors(long docCount, string id , bool ttfMode = true ,bool thisTextMode = false)
        {
            var elasticClient = CreateConnection();

            var termVectorResponse = elasticClient.TermVectors<BookInfoModel>(t => t.Index(thisTextMode ? "keywordanalysis" : Global.IndexElastic)
    //.Document(myDocument)
    .Id(new Id(id)) //you can specify document by id as well
    .TermStatistics()
    .Fields(f => f.content));



            Dictionary<string, double> keywords = new();
            foreach (var item in termVectorResponse.TermVectors)
            {

                //var topTerms = item.Value.Terms.OrderByDescending(x => x.Value.TotalTermFrequency).Take(10).ToList();

                foreach (var term in item.Value.Terms)
                {
                    var tf = ttfMode? term.Value.TotalTermFrequency  : term.Value.TermFrequency;

                    var tfidf = tf * (Math.Log(((double)(docCount )/(term.Value.DocumentFrequency))+1));
                    keywords[term.Key] = tfidf;
                }


            }


            return keywords;
        }

        public static Dictionary<string, BookInfoModel> SearchAll(string indexname)
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


            return result.ToDictionary(a => a.Key, a => a.Value);
        }
        public static Dictionary<string, BookInfoModel> SearchAllmax10(string indexname)
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
            text = text.Replace("\n", "");
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


            string body = "{\r\n    \"query\": {\r\n        \"dis_max\": {\r\n            \"queries\": [{\r\n                \"more_like_this\": {\r\n                    \"fields\": [\r\n                        \"topic\"\r\n                    ],\r\n                    \"minimum_should_match\": 1,\r\n                    \"min_term_freq\": 1,\r\n                    \"max_query_terms\": 1000,\r\n                    \"min_doc_freq\": 1,\r\n                    \"boost\": 2.0,\r\n                    \"analyzer\": \"persian\",\r\n                    \"include\": true,\r\n                    \"like\": \"@text\"\r\n                }},{\r\n                \"more_like_this\": {\r\n                    \"fields\": [\r\n                        \"content\"\r\n                    ],\r\n                    \"minimum_should_match\": 1,\r\n                    \"min_term_freq\": 1,\r\n                    \"max_query_terms\": 1000,\r\n                    \"min_doc_freq\": 1,\r\n                    \"boost\": 1.0,\r\n                    \"boost_terms\": 2.0,\r\n                    \"analyzer\": \"persian\",\r\n                    \"include\": true,\r\n                    \"like\": \"@text\"\r\n                }}\r\n            ]\r\n        }\r\n    }\r\n}";
            //string body = "{\r\n    \"query\": {\r\n        \"dis_max\": {\r\n            \"queries\": [{\r\n                \"more_like_this\": {\r\n                    \"fields\": [\r\n                        \"header_topic\"\r\n                    ],\r\n                    \"minimum_should_match\": 1,\r\n                    \"min_term_freq\": 1,\r\n                    \"max_query_terms\": 200,\r\n                    \"min_doc_freq\": 1,\r\n                    \"boost\": 1.0,\r\n                    \"boost_terms\": 3.0,\r\n                    \"analyzer\": \"persian\",\r\n                    \"include\": true,\r\n                    \"like\": \"@text\"\r\n                }},{\r\n                \"more_like_this\": {\r\n                    \"fields\": [\r\n                        \"content\"\r\n                    ],\r\n                    \"minimum_should_match\": 1,\r\n                    \"min_term_freq\": 1,\r\n                    \"max_query_terms\": 200,\r\n                    \"min_doc_freq\": 1,\r\n                    \"boost\": 2.0,\r\n                    \"analyzer\": \"persian\",\r\n                    \"include\": true,\r\n                    \"like\": \"@text\"\r\n                }}\r\n            ]\r\n        }\r\n    }\r\n}";
            //string body = "{\r\n    \"query\":{\r\n       \"more_like_this\":{\r\n          \"fields\":[\r\n             \"content\"\r\n             ],\r\n          \"like\":\"@text\",\r\n\r\n          \"min_term_freq\":1,\r\n          \"max_query_terms\":10000\r\n       }\r\n    }\r\n  }";
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

        public static string[] InsertData(BookInfoModel[] documents , string IndexName)
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


            List<string> ids = new List<string>();  
            var docsList = documents.Chunk(9000).ToArray();
            foreach (var docs in docsList)
            {

                var response = elasticClient.Bulk(s => s.Index(IndexName).CreateMany(docs).Refresh(Refresh.WaitFor));
                ids.AddRange(response.Items.Select(a => a.Id));
            }
            return ids.ToArray();

        }

        public static void CreateIndex(string name)
        {
            using (var client1 = new RestClient($"{Config.All.ElasticSearch.Address}/bookanalysis_{name}"))
            {

                var request1 = new RestRequest($"{Config.All.ElasticSearch.Address}/bookanalysis_{name}", Method.Put);
                request1.AddHeader("Content-Type", "application/json");
                request1.AddParameter("application/json", "{\r\n  \"settings\": {\r\n    \"index.mapping.ignore_malformed\": true,\r\n    \"analysis\": {\r\n      \"char_filter\": {\r\n        \"zero_width_spaces\": {\r\n          \"type\": \"mapping\",\r\n          \"mappings\": [\r\n            \"\\u200C=>\\u0020\",\r\n            \"٠ => 0\",\r\n            \"١ => 1\",\r\n            \"٢ => 2\",\r\n            \"٣ => 3\",\r\n            \"٤ => 4\",\r\n            \"٥ => 5\",\r\n            \"٦ => 6\",\r\n            \"٧ => 7\",\r\n            \"٨ => 8\",\r\n            \"٩ => 9\"\r\n          ]\r\n        },\r\n        \"number_filter\":{  \r\n            \"type\":\"pattern_replace\",\r\n            \"pattern\":\"\\\\d+\",\r\n            \"replacement\":\"\"\r\n         }\r\n      },\r\n      \"filter\": {\r\n        \"persian_stop\": {\r\n          \"type\": \"stop\",\r\n          \"stopwords\": \"_persian_\"\r\n        }\r\n      },\r\n      \"analyzer\": {\r\n        \"rebuilt_persian\": {\r\n          \"tokenizer\": \"standard\",\r\n          \"char_filter\": [\r\n            \"number_filter\",\r\n            \"zero_width_spaces\"\r\n          ],\r\n          \"filter\": [\r\n            \"lowercase\",\r\n            \"decimal_digit\",\r\n            \"arabic_normalization\",\r\n            \"persian_normalization\",\r\n            \"persian_stop\"\r\n          ]\r\n        }\r\n      }\r\n    },\r\n    \"index\" : {\r\n        \"number_of_shards\":3,\r\n        \"number_of_replicas\" : 1\r\n    }\r\n  }\r\n}", ParameterType.RequestBody);
                var res1 = client1.Execute(request1);

                if (res1.IsSuccessful)
                {
                    MessageBox.Show($"{name} index created", "CreateIndex", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    using (var client = new RestClient($"{Config.All.ElasticSearch.Address}/bookanalysis_{name}/_mapping"))
                    {

                        var request = new RestRequest($"{Config.All.ElasticSearch.Address}/bookanalysis_{name}/_mapping", Method.Post);
                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", "{\r\n  \"properties\": {\r\n    \"content\": {\r\n      \"type\": \"text\",\r\n      \"term_vector\": \"with_positions_offsets_payloads\",\r\n      \"store\": true,\r\n      \"analyzer\": \"persian\"\r\n    },\r\n    \"topic\": {\r\n      \"type\": \"text\",\r\n      \"analyzer\": \"persian\",\r\n      \"term_vector\": \"with_positions_offsets_payloads\",\r\n      \"store\": true,\r\n      \"fields\": {\r\n        \"keyword\": {\r\n          \"type\": \"keyword\"\r\n        }\r\n      }\r\n    },\r\n    \"publishdate\": {\r\n      \"type\": \"date\"\r\n    },\r\n    \"eventdate\": {\r\n      \"type\": \"date\"\r\n    },\r\n    \"bookname\": {\r\n      \"type\": \"text\",\r\n      \"fields\": {\r\n        \"keyword\": {\r\n          \"type\": \"keyword\"\r\n        }\r\n      }\r\n    }\r\n  }\r\n}", ParameterType.RequestBody);

                        var res = client.Execute(request);
                    }
                }
                else
                    MessageBox.Show($"{res1.ErrorMessage}", "CreateIndex", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
        public static bool DeleteAll (string IndexName)
        {
            var elasticClient = CreateConnection();
            var result = elasticClient.DeleteByQuery<BookInfoModel>(del => del.Index(IndexName)
    .Query(q => q.QueryString(qs => qs.Query("*")))
); 
            return result.IsValid;
        }
        public static bool DeleteDoc(string IndexName, string Id)
        {
            var elasticClient = CreateConnection();
            var result = elasticClient.Delete<BookInfoModel>(Id, a => a.Index(IndexName));
            return result.IsValid;
        }
        public static Dictionary<long, string> Analyser(string text)
        {

            var elasticClient = CreateConnection();
            var analyzeResponse = elasticClient.Indices.Analyze(a => a
                                .Analyzer("standard")
                                .Text(text));

            //analyzeResponse.Tokens.Select(a=> new  {a.Token ,a.Position}).
            return analyzeResponse.Tokens.GroupBy(a => a.Token).Select(a => new { token = a.Key, id = a.Max(s => s.Position) }).ToDictionary(a=>a.id,a=>a.token);


        }
        public static string[] GetGroupAllFields(string IndexName, string FieldName)
        {
            using (var client = new RestClient($"{Config.All.ElasticSearch.Address}/{IndexName}/_search"))
            {

                var request = new RestRequest($"{Config.All.ElasticSearch.Address}/{IndexName}/_search", Method.Post);
                request.AddHeader("Content-Type", "application/json");

                var body = "{\r\n  \"from\":0,\r\n  \"size\":0,\r\n  \"aggs\": {\r\n    \"query\": {\r\n      \"terms\": {\r\n        \"field\": \"@field\"\r\n      }\r\n    }\r\n  }\r\n}";
                body = body.Replace("@field", FieldName);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                var res = client.Execute(request);

                GroupAllFieldValue groupByField = JsonConvert.DeserializeObject<GroupAllFieldValue>(res.Content);

                return groupByField.aggregations.query.buckets.Select(a=>a.key).ToArray();

            }
        }
        public static Dictionary<string,int> GetGroupAllFieldsByFilter(string IndexName,  string MainField , string FilterField , string FilterValue)
        {
            using (var client = new RestClient($"{Config.All.ElasticSearch.Address}/{IndexName}/_search"))
            {

                var request = new RestRequest($"{Config.All.ElasticSearch.Address}/{IndexName}/_search", Method.Post);
                request.AddHeader("Content-Type", "application/json");

                var body = "{\r\n    \"size\": 0,\r\n    \"query\": {\r\n    \"bool\": {\r\n      \"must\": [],\r\n      \"filter\": [\r\n        {\r\n          \"bool\": {\r\n            \"should\": [\r\n              {\r\n                \"match_phrase\": {\r\n                  \"@FilterField\": \"@FilterValue\"\r\n                }\r\n              }\r\n            ],\r\n            \"minimum_should_match\": 1\r\n          }\r\n        }\r\n      ],\r\n      \"should\": [],\r\n      \"must_not\": []\r\n    }\r\n    },\r\n    \"aggs\": {\r\n\r\n            \"aggs\": {\r\n\"terms\": {\r\n                                \"field\": \"@AggsField\",\r\n                                \"size\": 20\r\n                            }\r\n            }\r\n        \r\n    }\r\n}";
                body = body.Replace("@AggsField", MainField).Replace("@FilterField", FilterField).Replace("@FilterValue", FilterValue);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                var res = client.Execute(request);

                GroupAllFieldByFilterValue groupByField = JsonConvert.DeserializeObject<GroupAllFieldByFilterValue>(res.Content);

                return groupByField.aggregations.aggs.buckets.Select(a =>  new { a.key ,a.doc_count } ).ToDictionary(a=>a.key ,a=>a.doc_count);

            }
        }

        public static List<BookInfoModel> SearchFields(string IndexName , string bookname="" , string topic = "", string content = "")
        {
            ConcurrentBag<BookInfoModel> result = new ConcurrentBag<BookInfoModel>();


            Time processTimePerScroll = "20s";
            int numberOfSlices = Environment.ProcessorCount;
            var elasticClient = CreateConnection();
       
            var scrollAllObservable = elasticClient.ScrollAll<BookLikeInfoModel>(processTimePerScroll, numberOfSlices, sc => sc
                .MaxDegreeOfParallelism(numberOfSlices)
                .Search(s => s.Index(IndexName)
                                 .Query(q => q
        .Term(p => p.bookname, bookname) || q.Term(p => p.topic, topic) || q.Term(p => p.content, content)
    )));
                            //.Search(s => s.Index(IndexName)
                            // .Query(q => q
                            //            .DisMax(dm => dm
                            //                .Queries(dq => dq
                            //                    .Match(m => m
                            //                        .Field(a => a.bookname)
                            //                        .Query(bookname)
                            //                    ), dq => dq
                            //                    .Match(m => m
                            //                        .Field(a => a.topic)
                            //                        .Query(topic)
                            //                    ), dq => dq
                            //                    .Match(m => m
                            //                        .Field(a => a.content)
                            //                        .Query(content)
                            //                    )
                            //                )
                            //            )
                            //       )));
            var waitHandle = new ManualResetEvent(false);
            Exception exception = null;

            var scrollAllObserver = new ScrollAllObserver<BookLikeInfoModel>(
                onNext: response =>
                {
                    // do something with the documents
                    var documents = response.SearchResponse.Documents;
                    foreach (var hit in response.SearchResponse.Hits)
                    {
                        result.Add( new BookLikeInfoModel
                        {
                            bookname = hit.Source.bookname,
                            id = hit.Source.id,
                            publishdate = hit.Source.publishdate,
                            content = hit.Source.content,
                            topic = hit.Source.topic,
                            score = hit.Source.score,
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



            return result.ToList();


        }



        public static List<BookLikeInfoModel> SearchTermByBookName(string IndexName , string BookName , string Content)
        {
            using (var client = new RestClient($"{Config.All.ElasticSearch.Address}/{IndexName}/_search"))
            {

                var request = new RestRequest($"{Config.All.ElasticSearch.Address}/{IndexName}/_search", Method.Post);
                request.AddHeader("Content-Type", "application/json");

                var body = "{\r\n  \"query\": {\r\n    \"bool\": {\r\n      \"must\": [],\r\n      \"filter\": [\r\n        {\r\n          \"bool\": {\r\n            \"should\": [\r\n              {\r\n                \"match_phrase\": {\r\n                  \"bookname.keyword\": \"@bookname\"\r\n                }\r\n              }\r\n            ],\r\n            \"minimum_should_match\": 1\r\n          }\r\n        },\r\n              {\r\n                \"bool\": {\r\n                  \"should\": [\r\n                    {\r\n                      \"match_phrase\": {\r\n                        \"content\": \"@content\"\r\n                      }\r\n                    }\r\n                  ],\r\n                  \"minimum_should_match\": 1\r\n                }\r\n              }\r\n      ],\r\n      \"should\": [],\r\n      \"must_not\": []\r\n    }\r\n  }\r\n}";
                body = body.Replace("@bookname", BookName).Replace("@content", Content);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                var res = client.Execute(request);

                MoreLikeThisQueryResponse result = JsonConvert.DeserializeObject<MoreLikeThisQueryResponse>(res.Content);

                List<BookLikeInfoModel> data = new List<BookLikeInfoModel>();
                foreach (var hit in result.hits.hits)
                {
                    data.Add(new BookLikeInfoModel
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
                return data;

            }
        }
    }
}
