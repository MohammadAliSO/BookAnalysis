using Book_Analysis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Analysis.Classes
{
    public static class Global
    {
        public static string IndexElastic = "";
        private static Random rng = new Random();


        public static List<BookInfoModel> FileToBookInfoModel(string PathFile)
        {
            Analysis analysis= new Analysis();
            List<string> file = File.ReadAllLines(PathFile).ToList();
            List<BookInfoModel> datas = new List<BookInfoModel>();
            //bookname,topic,publishdate,content
            foreach (var row in file)
            {
                if (row == "bookname,topic,publishdate,content") continue;
                var fields = row.Split(",");
                fields[3] = analysis.PersianToEnglishNumber(fields[3]);
                fields[3] = analysis.RemovePunctuations(fields[3]);
                fields[3] = analysis.RemoveNumber(fields[3]);
                fields[3] = analysis.RemoveEnterLine(fields[3]);
                if (fields[3] == "") continue;
                datas.Add(new BookInfoModel
                {
                    bookname= fields[0],
                    content= fields[3],
                    eventdate= DateTime.Now,
                    publishdate=Convert.ToDateTime( fields[2]),
                    topic= fields[1],
                });
            }

            return datas;
        }
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


        public static void openBrowse(string url)
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
    }
}
