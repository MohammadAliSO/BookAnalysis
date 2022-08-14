using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Analysis.Models
{
    public class BookInfoModel
    {
        public string bookname { get; set; }
       // public string header { get; set; }
        public string topic { get; set; }
        //public string header_topic { get; set; }
        public string content { get; set; }
        public DateTime publishdate { get; set; }
        public DateTime eventdate { get; set; }

    }

    public class BookLikeInfoModel:BookInfoModel
    {
        public string id { get; set; }
        public double? score { get; set; }
    }
}
