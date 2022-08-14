using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Book_Analysis.Classes
{
    public class Analysis
    {
        public List<List<bool>> Validation(string indexName)
        {
            var Data= ElasticSearchHelper.SearchAlltt(indexName);
            var countData = Data.Count;
            var splitData= Data.Chunk((countData/5)+1).ToList();
            List<List<bool>> CrossVal = new List<List<bool>>();
            //List<> Data = new List<>();
            //List<> testData = new List<>();
            //List<> result = new List<>();

            foreach (var split in splitData)
            {
                List<bool> valList = new List<bool>();

                var splitTest = split.ToDictionary(a => a.Key, a => a.Value);
                foreach (var testD in splitTest)
                {
                    var result=ElasticSearchHelper.MoreLikeThisQuery(testD.Value.content);
                    
                    if (result != null)
                    {
                        result.RemoveAt(result.FindIndex(a => splitTest.ContainsKey(a.id)));
                         var resOrd=result.OrderByDescending(a => a.score).ToList();
                        foreach (var res in resOrd)
                        {
                            if (splitTest.ContainsKey(res.id)) continue;

                            if(res.topic == testD.Value.topic) valList.Add(true);
                            else valList.Add(false);
                            break;
                        }

                    }
                }

                CrossVal.Add(valList);
            }
            

            return CrossVal;
        }
        public string RemovePunctuations(string text)
        {
            var sb = new StringBuilder();
            string cleanedText = Regex.Replace(text, @"(http|https):[^\s]+", "");
            foreach (char c in cleanedText)
            {
                if (!char.IsPunctuation(c))
                    sb.Append(c);
                else
                {
                    if (c is '.') sb.Append('\n');
                    else sb.Append(' ');
                }
            }
            return sb.ToString();
        }


    }
}
