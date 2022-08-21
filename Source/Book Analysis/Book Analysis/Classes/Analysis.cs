using Book_Analysis.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace Book_Analysis.Classes
{
    public class Analysis
    {
        public string[] KeyWordCalculate(string text , string DocSpliter="\n")
        {
            var contents = text.Split(DocSpliter);

            List<BookInfoModel> listdocs = new List<BookInfoModel>();
            Analysis analysis = new Analysis();
            foreach (string content in contents)
            {

                var content1 = analysis.PersianToEnglishNumber(content);
                content1 = analysis.RemovePunctuations(content1);
                content1 = analysis.RemoveNumber(content1);
                content1 = analysis.RemoveEnterLine(content1);

                if (content1 == "") continue;
                listdocs.Add(new BookInfoModel
                {
                    content = content1
                });
            }

            var ids = ElasticSearchHelper.InsertData(listdocs.ToArray(), "keywordanalysis");

            var ctt = analysis.PersianToEnglishNumber(text);
            ctt = analysis.RemovePunctuations(ctt);
            ctt = analysis.RemoveNumber(ctt);
            ctt = analysis.RemoveEnterLine(ctt);
            listdocs.Clear();
            listdocs.Add(new BookInfoModel
                {
                    content = ctt
                });
            var idOrg = ElasticSearchHelper.InsertData(listdocs.ToArray(), "keywordanalysis");

            Dictionary<string, double> keywords = new();
            foreach (var id in idOrg)
            {
                var keyp = ElasticSearchHelper.TermVectors(contents.Count(), id, true, true);
                foreach (var k in keyp)
                {
                    if (keywords.ContainsKey(k.Key))
                        keywords[k.Key] += k.Value;
                    else keywords[k.Key] = k.Value;

                }

            }
            var keys = keywords.OrderByDescending(a => a.Value).Select(a => a.Key).Take(10).ToArray();
            ElasticSearchHelper.DeleteAll("keywordanalysis");
            return keys;

        }
        public List<List<bool>> Validation(string indexName)
        {
            var Data = ElasticSearchHelper.SearchAll(indexName);
            var countData = Data.Count;
            var splitData = Data.Chunk((countData / 5) + 1).ToList();
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
                    var result = ElasticSearchHelper.MoreLikeThisQuery(testD.Value.content);

                    if (result != null)
                    {
                        result.RemoveAt(result.FindIndex(a => splitTest.ContainsKey(a.id)));
                        var resOrd = result.OrderByDescending(a => a.score).ToList();
                        foreach (var res in resOrd)
                        {
                            if (splitTest.ContainsKey(res.id)) continue;

                            if (res.topic == testD.Value.topic) valList.Add(true);
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

        public string PersianToEnglishNumber(string persianStr)
        {
            Dictionary<char, char> LettersDictionary = new Dictionary<char, char>
            {
                ['۰'] = '0',
                ['۱'] = '1',
                ['۲'] = '2',
                ['۳'] = '3',
                ['۴'] = '4',
                ['۵'] = '5',
                ['۶'] = '6',
                ['۷'] = '7',
                ['۸'] = '8',
                ['۹'] = '9'
            };
            foreach (var item in LettersDictionary)
            {
                persianStr = persianStr.Replace(item.Key, item.Value);
            }
            return persianStr;
        }

        public string RemoveNumber(string str)
        {
            return Regex.Replace(str, @"[\d-]", string.Empty); ;
        }

        public string RemoveEnterLine(string str)
        {
            return str.Replace("\n", "");
            ;
        }
    }
}

