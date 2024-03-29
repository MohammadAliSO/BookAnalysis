﻿using Book_Analysis.Classes;
using Book_Analysis.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Book_Analysis;

public partial class Book_Analysis : Form
{

    static string[]? BookDocs;
    static List<BookInfoModel>? BookDocsEdit;
    static List<BookLikeInfoModel>? DataTermDocs = null;

    static int offsetDoc = 0;
    static int offsetDocEdit = 0;
    static int offsetDocTerm = 0;

    static string[]? NewBookIds=null;
    static string[]? TopicTermDocs = null;

    public Book_Analysis()
    {
        InitializeComponent();

    }
    public List<List<bool>> Validation(string indexName)
    {
        var Data = ElasticSearchHelper.SearchAll(indexName);
        pbValidation.Value += 15;

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
                    int selfindex = result.FindIndex(a => splitTest.ContainsKey(a.id));
                    if (selfindex is not -1) result.RemoveAt(selfindex);
                    var resOrd = result.OrderByDescending(a => a.score).ToList();

                    for (int i = 0; i < resOrd.Count; i++)
                    {
                        if (splitTest.ContainsKey(resOrd[i].id)) continue;

                        if (resOrd[i].topic == testD.Value.topic) valList.Add(true);
                        else
                        {
                            if (i + 1 != resOrd.Count && resOrd[i].score == resOrd[i + 1].score) continue;

                            valList.Add(false);
                        }
                        break;
                    }
                    //foreach (var res in resOrd)
                    //{
                    //   // if (splitTest.ContainsKey(res.id)) continue;
                    //    if()
                    //    if (res.header_topic == testD.Value.header_topic) valList.Add(true);
                    //    else 
                    //        valList.Add(false);
                    //    break;
                    //}

                }
            }

            CrossVal.Add(valList);
            var ss = valList.Where(a => a == false).ToList();
            pbValidation.Value += 20;
        }


        return CrossVal;
    }

    public List<List<bool>> Validation2(string indexName)
    {
        var Data = ElasticSearchHelper.SearchAll(indexName);
        pbValidation.Value += 15;

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
                List<BookLikeInfoModel> resultFiltered = new List<BookLikeInfoModel>();

                foreach (var r in result) if (!splitTest.ContainsKey(r.id)) resultFiltered.Add(r);

                if (result.Count != 0)
                {

                    var disRes = resultFiltered.Select(a => new { a.topic, a.score }).Distinct().ToList();
                    var sumRes = disRes.GroupBy(a => a.topic).Select(a => new { tp = a.Key, sum = a.Sum(s => s.score) }).ToList();
                    var resOrd = sumRes.OrderByDescending(a => a.sum).ToList();


                    if (resOrd.First().tp == testD.Value.topic)
                        valList.Add(true);

                    else
                        valList.Add(false);


                    //foreach (var res in resOrd)
                    //{
                    //   // if (splitTest.ContainsKey(res.id)) continue;
                    //    if()
                    //    if (res.header_topic == testD.Value.header_topic) valList.Add(true);
                    //    else 
                    //        valList.Add(false);
                    //    break;
                    //}

                }
            }

            CrossVal.Add(valList);
            var ss = valList.Where(a => a == false).ToList();
            pbValidation.Value += 20;
        }


        return CrossVal;
    }
    public void AppendText(RichTextBox box, string text, Color color)
    {
        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;

        //box.SelectionColor = color;
        box.SelectionBackColor = color;
        box.AppendText(text);
        //box.SelectionColor = box.ForeColor;
    }

    public string GetDoc()
    {
        var doc = "";
        do
        {

            doc = BookDocs[offsetDoc];
            var result = ElasticSearchHelper.MoreLikeThisQuery(doc);

            var disRes = result.Select(a => new { a.topic, a.score }).Distinct().ToList();

            var minRes = disRes.GroupBy(a => a.topic).Select(a => new { tp = a.Key, sum = a.Sum(s => s.score) }).ToList();

            lbToicsLearn.Items.Clear(); tbTopicLearn.Text = "";
            //lbToicsLearn.DroppedDown = true;

            if (result != null && result.Count > 0)
            {
                //cbHeaderLearn.Items.AddRange(result.OrderByDescending(a=>a.score).Take(10).Select((a,b)=> a.header+"("+a.score+")").ToArray());
                //cbHeaderLearn.Items.AddRange(result.OrderByDescending(a => a.score).Take(10).Select((a, b) => a.header_topic.Substring(0, a.header_topic.IndexOf("_")) + "(" + a.score + ")").ToArray());
                //cbHeaderLearn.SelectedIndex = 0;
                //lbToicsLearn.Items.AddRange(result.OrderByDescending(a => a.score).Take(10).Select((a, b) => a.topic + "(" + a.score + ")").ToArray());
                //lbToicsLearn.Items.AddRange(result.OrderByDescending(a => a.score).Take(10).Select((a, b) => a.header_topic.Substring(a.header_topic.IndexOf("_") + 1) + "(" + a.score + ")").ToArray());
                var listtopic = minRes.OrderByDescending(a => a.sum).Take(10).Select(a => a.tp + "(" + a.sum + ")").ToArray();
                lbToicsLearn.Items.AddRange(listtopic);

                tbTopicLearn.Text = listtopic.First();

            }

            offsetDoc++;
        } while (doc == "" || doc == "\n" || doc == "\r\n");

        return doc;
    }

   

    private async void Book_Analysis_Load(object sender, EventArgs e)
    {


        cbCategoryLearn.Items.Clear();

        cbCategoryLearn.Items.AddRange(await ElasticSearchHelper.GetIndex());


    }

    private void btnBrowsBookFile_Click(object sender, EventArgs e)
    {
        //var fileContent = string.Empty;
        var filePath = string.Empty;

        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "Choose Book file";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                //Read the contents of the file into a stream
                //var fileStream = openFileDialog.OpenFile();

                //using (StreamReader reader = new StreamReader(fileStream))
                //{
                //    fileContent = reader.ReadToEnd();
                //}
            }
        }

        tbBookFile.Text = filePath;
    }

    private void tabLearn_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.Text))
            e.Effect = DragDropEffects.Copy;
        else
            e.Effect = DragDropEffects.None;
    }

    private void tabLearn_DragDrop(object sender, DragEventArgs e)
    {
        tbBookFile.Text = e.Data.GetData(DataFormats.Text).ToString();
    }

    private void btnReadBook_Click(object sender, EventArgs e)
    {
        offsetDoc = 0;
        var pathfile = tbBookFile.Text;
        var filename = Path.GetFileName(tbBookFile.Text);

        //convert all format(pdf,word) to txt file


        //Read the contents of the file into a stream
        using (StreamReader reader = new StreamReader(tbBookFile.Text))
        {
            var BookContent = reader.ReadToEnd();
            //remove extra text and just paragraph and chapter and ...
            //split paragraph 

            BookDocs = BookContent.Split("\r\n");


            tbContentLearn.Text = BookDocs[0];
            btnLearn.Enabled = true;
            //tbBookLearn.Text = filename.Substring(0, filename.IndexOf("."));

        }

    }

    private void btnLearn_Click(object sender, EventArgs e)
    {
        btnLearn.BackColor = Color.LightGray;

        if (rbReadBookLearn.Checked == true)
        {


            //var filename = Path.GetFileName(tbBookFile.Text);
            var topic = tbTopicLearn.Text;
            //var header = cbHeaderLearn.Text;

            var contents = tbContentLearn.Text.Split("@@@@@");

            List<BookInfoModel> listdocs = new List<BookInfoModel>();
            foreach (string content in contents)
            {

                Analysis analysis = new Analysis();
                var content1 = analysis.PersianToEnglishNumber(content);
                content1 = analysis.RemovePunctuations(content1);
                content1 = analysis.RemoveNumber(content1);
                content1 = analysis.RemoveEnterLine(content1);
                listdocs.Add(new BookInfoModel
                {
                    bookname = tbBookLearn.Text,
                    //header= header.Contains("(") ? header.Substring(0,header.IndexOf("(")):header,
                    content = content1,
                    publishdate = dtpPublishDateLearn.Value,
                    topic = topic.Contains("(") ? topic.Substring(0, topic.IndexOf("(")) : topic,
                    eventdate = DateTime.Now,
                    //header_topic=(header.Contains("(") ? header.Substring(0,header.IndexOf("(")):header)+"_"+(topic.Contains("(") ? topic.Substring(0,topic.IndexOf("(")):topic)
                });

            }

            var ids = ElasticSearchHelper.InsertData(listdocs.ToArray(), Global.IndexElastic);


            if (offsetDoc == BookDocs.Length) { btnLearn.Enabled = false; btnReadBook.Enabled = true; return; }
            //tbContentLearn.Text = GetDoc();
            btnLearn.BackColor = Color.LightGreen;

        }

        if (rbImportLearn.Checked == true)
        {
            var docs = Global.FileToBookInfoModel(tbPathImportLearn.Text);

            ElasticSearchHelper.InsertData(docs.ToArray(), Global.IndexElastic);
            btnLearn.BackColor = Color.LightGreen;
        }


    }



    private void btnRefresh_Click(object sender, EventArgs e)
    {
        lbToicsLearn.Items.Clear(); tbTopicLearn.Text = "";
        lbKeywords.Items.Clear(); lbKeywords.Text = "";
        var result = ElasticSearchHelper.MoreLikeThisQuery(tbContentLearn.Text);

        var disRes = result.Select(a => new { a.topic, a.score }).Distinct().ToList();
        var minRes = disRes.GroupBy(a => a.topic).Select(a => new { tp = a.Key, sum = a.Sum(s => s.score) }).ToList();

        if (result != null && disRes.Count > 0)
        {
            //cbHeaderLearn.Items.AddRange(result.OrderByDescending(a=>a.score).Take(10).Select((a,b)=> a.header+"("+a.score+")").ToArray());
            //cbHeaderLearn.Items.AddRange(disRes.OrderByDescending(a => a.score).Take(10).Select(a => a.header_topic.Substring(0, a.header_topic.IndexOf("_")) + "(" + a.score + ")").ToArray());
            //cbHeaderLearn.SelectedIndex = 0;
            //lbToicsLearn.Items.AddRange(result.OrderByDescending(a => a.score).Take(10).Select((a, b) => a.topic + "(" + a.score + ")").ToArray());
            //lbToicsLearn.Items.AddRange(disRes.OrderByDescending(a => a.score).Take(10).Select(a => a.header_topic.Substring(a.header_topic.IndexOf("_") + 1) + "(" + a.score + ")").ToArray());
            var listtopic = minRes.OrderByDescending(a => a.sum).Take(10).Select(a => a.tp + "(" + a.sum + ")").ToArray();
            lbToicsLearn.Items.AddRange(listtopic);
            var topic=listtopic.First();
            tbTopicLearn.Text = topic.Contains("(") ? topic.Substring(0, topic.IndexOf("(")) : topic;
            if(listtopic.Count() > 5 || (minRes.OrderByDescending(a => a.sum).First().sum < 1000))
            {
                tbTopicLearn.Text = "unknown";
            }

        }
        else
        {
            tbTopicLearn.Text = "unknown";

        }




        Analysis analysis = new Analysis();

        lbKeywords.Items.AddRange(analysis.KeyWordCalculate(tbContentLearn.Text.Replace("@@@@@", ""), "\n"));
        //lbToicsLearn.DroppedDown = true;



    }

    private async void btnAddCategoryLearn_Click(object sender, EventArgs e)
    {
        Global.IndexElastic = $"bookanalysis_{cbCategoryLearn.Text.ToLower()}";
        if (cbCategoryLearn.Text is not null)
        {
            ElasticSearchHelper.CreateIndex(cbCategoryLearn.Text.ToLower());
        }
        cbCategoryLearn.Items.Clear();
        cbCategoryLearn.Items.AddRange(await ElasticSearchHelper.GetIndex());
    }

    private void cbCategoryLearn_SelectedIndexChanged(object sender, EventArgs e)
    {
        Global.IndexElastic = $"bookanalysis_{cbCategoryLearn.Text.ToLower()}";
        lbAllTopicsLearn.Items.AddRange(ElasticSearchHelper.GetGroupAllFields(Global.IndexElastic,"topic.keyword"));

    }

    private void btnStartValidation_Click(object sender, EventArgs e)
    {
        if (rbKfold.Checked == true)
        {
            tbPerformance1.Text = "";
            tbPerformance2.Text = "";
            tbPerformance3.Text = "";
            tbPerformance4.Text = "";
            tbPerformance5.Text = "";
            tbPerformanceTotal.Text = "";

            pbValidation.Maximum = 120;
            pbValidation.Step = 1;
            pbValidation.Value = 5;

            List<List<bool>> res = Validation2(Global.IndexElastic);

            tbPerformance1.Text = ((float)(res[0].Where(a => a == true).ToArray().Length) / res[0].Count) * 100 + "%";
            tbPerformance2.Text = ((float)(res[1].Where(a => a == true).ToArray().Length) / res[1].Count) * 100 + "%";
            tbPerformance3.Text = ((float)(res[2].Where(a => a == true).ToArray().Length) / res[2].Count) * 100 + "%";
            tbPerformance4.Text = ((float)(res[3].Where(a => a == true).ToArray().Length) / res[3].Count) * 100 + "%";
            tbPerformance5.Text = ((float)(res[4].Where(a => a == true).ToArray().Length) / res[4].Count) * 100 + "%";


            tbPerformanceTotal.Text = ((float)(res.Sum(l => l.Where(a => a == true).ToArray().Length)) / res.Sum(l => l.Count)) * 100 + "%";
        }
        if (rbKappa.Checked == true)
        {

        }
        if (rbTestVal.Checked == true)//test val
        {

            lbTopicsTestval.Items.Clear(); lbTopicsTestval.Text = "";
            lbKeywordVal.Items.Clear(); lbKeywordVal.Text = "";

            var result = ElasticSearchHelper.MoreLikeThisQuery(tbContentTestVal.Text);

            var disRes = result.Select(a => new { a.topic, a.score }).Distinct().ToList();
            var minRes = disRes.GroupBy(a => a.topic).Select(a => new { tp = a.Key, sum = a.Sum(s => s.score) }).ToList();

            if (result != null && disRes.Count > 0)
            {
                lbTopicsTestval.Items.AddRange(minRes.OrderByDescending(a => a.sum).Take(10).Select(a => a.tp + "(" + a.sum + ")").ToArray());
            }



            Analysis analysis = new Analysis();

            lbKeywordVal.Items.AddRange(analysis.KeyWordCalculate(tbContentTestVal.Text, "\n"));

        }


    }

    private async void tabControl_SelectedIndexChanged(object sender, EventArgs e)
    {

        cbCategoryLearn.Items.Clear();
        cbCategoryValidation.Items.Clear();
        cbCategoryEdit.Items.Clear();
        cbCategoryAnal.Items.Clear();
        cbCategoryTermAnal.Items.Clear();
        var items = await ElasticSearchHelper.GetIndex();
        cbCategoryLearn.Items.AddRange(items);
        cbCategoryValidation.Items.AddRange(items);
        cbCategoryEdit.Items.AddRange(items);
        cbCategoryAnal.Items.AddRange(items);
        cbCategoryTermAnal.Items.AddRange(items);

    }

    private void cbCategoryValidation_SelectedIndexChanged(object sender, EventArgs e)
    {
        Global.IndexElastic = $"bookanalysis_{cbCategoryValidation.Text.ToLower()}";
    }

    private void btnStartAnalysis_Click(object sender, EventArgs e)
    {
        pbAnalysis.Visible = true;
        lbKeywordAnal.Items.Clear();
        lbTopicsAnal.Items.Clear();
        lbBookAnal.Items.Clear();
        tbmainTopicAnal.Text = "";
        tbsimBookAnal.Text = "";


        List<BookLikeInfoModel> ScorsList = new List<BookLikeInfoModel>();
        Dictionary<string,int> TopicPredictCount = new Dictionary<string, int>();
        List<BookInfoModel> listdocs = new List<BookInfoModel>();

        var topicsAll = ElasticSearchHelper.GetGroupAllFields(Global.IndexElastic, "topic.keyword");
        foreach (var topic in topicsAll) { TopicPredictCount[topic] = 0; }
        TopicPredictCount.Add("unknown",0);
        //Read the contents of the file into a stream
        using (StreamReader reader = new StreamReader(tbPathFileAnal.Text))
        {
            Analysis analysis = new Analysis();

            var BookContent = reader.ReadToEnd();
            //remove extra text and just paragraph and chapter and ...
            //split paragraph 

            var BookAnalysisDocs = BookContent.Split("\r\n");

            //set pb
            pbAnalysis.Maximum = BookAnalysisDocs.Length +10;
            pbAnalysis.Step = 1;
            pbAnalysis.Value = 5;


            foreach (var text in BookAnalysisDocs)
            {
                pbAnalysis.Value += 1;

                var text2 = analysis.PersianToEnglishNumber(text);
                text2 = analysis.RemovePunctuations(text2);
                text2 = analysis.RemoveNumber(text2);
                text2 = analysis.RemoveEnterLine(text2);
                if (text2 == "") 
                    continue;




                var result = ElasticSearchHelper.MoreLikeThisQuery(text2);
                if (result == null || result.Count == 0) 
                    continue;
                var disRes = result.Select(a => new { a.topic, a.score }).Distinct().ToList();
                var minRes = disRes.GroupBy(a => a.topic).Select(a => new { tp = a.Key, sum = a.Sum(s => s.score) }).ToList();

                if (result != null && disRes.Count > 0)
                {
                    var listtopic = minRes.OrderByDescending(a => a.sum).Take(10).Select(a => a.tp ).ToArray();

                    var topic = listtopic.First();
                    if (listtopic.Count() > 5 || (minRes.OrderByDescending(a => a.sum).First().sum < 1000))
                        TopicPredictCount["unknown"]++;
                    else TopicPredictCount[topic]++;


                    listdocs.Add(new BookInfoModel
                    {
                        bookname = tbBookNameAnal.Text,
                        content = text2,
                        publishdate = dtpPublishDateLearn.Value,
                        topic = topic,
                        eventdate = DateTime.Now,
                    });

                }
                else
                {
                    TopicPredictCount["unknown"]++;
                }



                ScorsList.AddRange(result);


            }

            var textk = analysis.PersianToEnglishNumber(BookContent);
            textk = analysis.RemovePunctuations(textk);
            textk = analysis.RemoveNumber(textk);
            lbKeywordAnal.Items.AddRange(analysis.KeyWordCalculate(textk, "\n"));
            //tbBookLearn.Text = filename.Substring(0, filename.IndexOf("."));

        }


      
        //topics
        var sumDocs = TopicPredictCount.Values.Sum();
        foreach (var tc in TopicPredictCount.OrderByDescending(a=>a.Value))
        {
            lbTopicsAnal.Items.Add("["+tc.Key+"]    ("+ (int)(((float)tc.Value/ sumDocs)*100)+"%)    "+tc.Value);
        }

        var books = ElasticSearchHelper.GetGroupAllFields(Global.IndexElastic, "bookname.keyword");

        Dictionary<string, string[]?> bibib = new();


        TopicPredictCount.Remove("unknown");
        var TopicOfNewBook = TopicPredictCount.Where(a => (int)(((float)a.Value / sumDocs) * 100) > 5).Select(a => a.Key).ToList();
        Dictionary<string, int> bookeSimilarPercent = new Dictionary<string, int>();
        foreach (var book in books)
        {
            var topicCount_dic = ElasticSearchHelper.GetGroupAllFieldsByFilter(Global.IndexElastic, "topic.keyword", "bookname.keyword", book);
            var alldoc = topicCount_dic.Sum(a=>a.Value);
            int containDocsCount = 0;
            foreach (var topic in topicCount_dic)
            {

                if (!TopicOfNewBook.Contains(topic.Key) || ((int)(((float)topic.Value / alldoc) * 100))<5) continue;

                containDocsCount += topic.Value;
            }
            bookeSimilarPercent[book] = (int)(((float)containDocsCount / alldoc) * 100);
            lbBookAnal.Items.Add("[" + book + "]    (" + (int)(((float)containDocsCount / alldoc) * 100) + "%)    " + containDocsCount);
        }

        //add to db new book
        NewBookIds = ElasticSearchHelper.InsertData(listdocs.ToArray(), Global.IndexElastic);


       

        pbAnalysis.Value = pbAnalysis.Maximum;
        tbmainTopicAnal.Text = TopicPredictCount.OrderByDescending(a => a.Value).First().Key;
        tbsimBookAnal.Text = bookeSimilarPercent.OrderByDescending(a => a.Value).First().Key;
        pbAnalysis.Visible = false ;

    }

    private void rbKfold_Click(object sender, EventArgs e)
    {

        gbKFold.Visible = true;
        gbTestFile.Visible = false;
        gbTestVal.Visible = false;

    }

    private void rbKappa_Click(object sender, EventArgs e)
    {
        gbTestFile.Visible = true;
        gbKFold.Visible = false;
        gbTestVal.Visible = false;

    }


    private void rbTestVal_Click(object sender, EventArgs e)
    {
        gbTestFile.Visible = false;
        gbKFold.Visible = false;
        gbTestVal.Visible = true;
    }

    private void btnNextLearn_Click(object sender, EventArgs e)
    {
        if (offsetDoc == BookDocs.Count() - 1)
        {
            MessageBox.Show("End Of Book!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        lbToicsLearn.Items.Clear(); tbTopicLearn.Text = "";
        lbKeywords.Items.Clear(); lbKeywords.Text = "";
        offsetDoc++;
        tbContentLearn.Text = BookDocs[offsetDoc];

        btnLearn.BackColor = Color.LightGray;

    }

    private void btnPrevLearn_Click(object sender, EventArgs e)
    {
        if (offsetDoc == 0)
        {
            MessageBox.Show("Start Of Book!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        lbToicsLearn.Items.Clear(); tbTopicLearn.Text = "";
        lbKeywords.Items.Clear(); lbKeywords.Text = "";
        offsetDoc--;
        tbContentLearn.Text = BookDocs[offsetDoc];


    }

    private void btnAddNextLearn_Click(object sender, EventArgs e)
    {
        if (offsetDoc == BookDocs.Count() - 1)
        {
            MessageBox.Show("End Of Book!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        lbToicsLearn.Items.Clear(); tbTopicLearn.Text = "";
        lbKeywords.Items.Clear(); lbKeywords.Text = "";
        offsetDoc++;
        tbContentLearn.Text += "\n@@@@@\n";
        tbContentLearn.Text += BookDocs[offsetDoc];
    }

    private void btnNext10Learn_Click(object sender, EventArgs e)
    {
        if (offsetDoc >= BookDocs.Count() - 10)
        {
            MessageBox.Show("End Of Book!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        lbToicsLearn.Items.Clear(); tbTopicLearn.Text = "";
        lbKeywords.Items.Clear(); lbKeywords.Text = "";
        offsetDoc += 10;
        tbContentLearn.Text = BookDocs[offsetDoc];

        btnLearn.BackColor = Color.LightGray;
    }

    private void btnPrev10Learn_Click(object sender, EventArgs e)
    {
        if (offsetDoc <= 10)
        {
            MessageBox.Show("Start Of Book!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        lbToicsLearn.Items.Clear(); tbTopicLearn.Text = "";
        lbKeywords.Items.Clear(); lbKeywords.Text = "";
        offsetDoc -= 10;
        tbContentLearn.Text = BookDocs[offsetDoc];
        btnLearn.BackColor = Color.LightGray;

    }

    private void rbReadBookLearn_Click(object sender, EventArgs e)
    {
        gbAnalysisLearn.Visible = true;
        gbbookFileLearn.Visible = true;
        gbInfoLearn.Visible = true;
        btnNextLearn.Visible = true;
        btnNext10Learn.Visible = true;
        btnAddNextLearn.Visible = true;
        btnPredict.Visible = true;
        btnPrev10Learn.Visible = true;
        btnPrevLearn.Visible = true;
        gbBulkLearn.Visible = false;
    }

    private void rbImportLearn_CheckedChanged(object sender, EventArgs e)
    {
        gbAnalysisLearn.Visible = false;
        gbbookFileLearn.Visible = false;
        gbInfoLearn.Visible = false;
        btnNextLearn.Visible = false;
        btnNext10Learn.Visible = false;
        btnAddNextLearn.Visible = false;
        btnPredict.Visible = false;
        btnPrev10Learn.Visible = false;
        btnPrevLearn.Visible = false;
        gbBulkLearn.Visible = true;
    }

    private void btnBrowsImport_Click(object sender, EventArgs e)
    {
        var filePath = string.Empty;

        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "Choose Import file";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                //Read the contents of the file into a stream
                //var fileStream = openFileDialog.OpenFile();

                //using (StreamReader reader = new StreamReader(fileStream))
                //{
                //    fileContent = reader.ReadToEnd();
                //}
            }
        }

        tbPathImportLearn.Text = filePath;
    }

    private void tbContentLearn_TextChanged(object sender, EventArgs e)
    {
        btnLearn.BackColor = Color.LightGray;

    }


    private void cbBooknameEdit_Click(object sender, EventArgs e)
    {
        cbBooknameEditSearch.Items.Clear(); cbBooknameEditSearch.Items.AddRange(ElasticSearchHelper.GetGroupAllFields(Global.IndexElastic, "bookname.keyword"));
    }

    private void cbTopicEdit_Click(object sender, EventArgs e)
    {
        cbTopicEditSearch.Items.Clear(); cbTopicEditSearch.Items.AddRange(ElasticSearchHelper.GetGroupAllFields(Global.IndexElastic, "topic.keyword"));
    }

    private void cbCategoryEdit_SelectedIndexChanged(object sender, EventArgs e)
    {
        Global.IndexElastic = $"bookanalysis_{cbCategoryEdit.Text.ToLower()}";

    }

    private void btnSearchEdit_Click(object sender, EventArgs e)
    {
        //BookDocsEdit = ElasticSearchHelper.SearchFields(Global.IndexElastic, cbBooknameEditSearch.Text, cbTopicEditSearch.Text, tbContentEditSearch.Text);

        //if (BookDocsEdit.Count == 0) return;
        //tbBookNameEditRes.Text = BookDocsEdit[0].bookname;
        //tbTopicEditRes.Text = BookDocsEdit[0].topic;
        //dpPublisDateEditRes.Value = BookDocsEdit[0].publishdate;
        //dpEventDateEditRes.Value = BookDocsEdit[0].eventdate;
        //tbContentEditReult.Text = BookDocsEdit[0].content;


        gbResultSearch.Enabled = true;
    }

    private void btnDashboard_Click(object sender, EventArgs e)
    {
        Global.openBrowse($"http://localhost:3000/d/wECO4Qi4z/bookanalysis_{cbCategoryAnal.Text}?var-topic=All&var-book="+tbBookNameAnal.Text.Replace(" ","%20"));
    }

    private void lbToicsLearn_SelectedIndexChanged(object sender, EventArgs e)
    {
        var topic = lbToicsLearn.SelectedItem.ToString();
        tbTopicLearn.Text = topic.Contains("(") ? topic.Substring(0, topic.IndexOf("(")) : topic;
    }

    private void lbAllTopicsLearn_SelectedIndexChanged(object sender, EventArgs e)
    {

        var topic = lbAllTopicsLearn.SelectedItem.ToString();
        tbTopicLearn.Text = topic.Contains("(") ? topic.Substring(0, topic.IndexOf("(")) : topic;
    }

    private void cbCategoryAnal_SelectedIndexChanged(object sender, EventArgs e)
    {
        Global.IndexElastic = $"bookanalysis_{cbCategoryAnal.Text.ToLower()}";

    }

    private void btnBrowseAnal_Click(object sender, EventArgs e)
    {
        //var fileContent = string.Empty;
        var filePath = string.Empty;

        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "Choose Book file";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                //Read the contents of the file into a stream
                //var fileStream = openFileDialog.OpenFile();

                //using (StreamReader reader = new StreamReader(fileStream))
                //{
                //    fileContent = reader.ReadToEnd();
                //}
            }
        }

        tbPathFileAnal.Text = filePath;
    }

    private void btnAcceptAnal_Click(object sender, EventArgs e)
    {
        // add and learn this book
        if (NewBookIds is null || NewBookIds.Length == 0)
            MessageBox.Show("Not analysis new Book", "Accept Book", MessageBoxButtons.OK, MessageBoxIcon.Error);
        else
        {
            NewBookIds = null;
                MessageBox.Show("Book accepted", "Accept Book", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }

    private void btnRejectAnal_Click(object sender, EventArgs e)
    {
        if (NewBookIds is null || NewBookIds.Length == 0)
            MessageBox.Show("Not analysis new Book" , "Reject Book" ,MessageBoxButtons.OK ,MessageBoxIcon.Error);
        else
        {

        foreach (var id in NewBookIds)
        {
            ElasticSearchHelper.DeleteDoc(Global.IndexElastic ,id);
        }
        NewBookIds = null;
            MessageBox.Show("Book rejected", "Reject Book", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }

    private void cbBooknameTermAnal_Click(object sender, EventArgs e)
    {
        cbBooknameTermAnal.Items.Clear(); cbBooknameTermAnal.Items.AddRange(ElasticSearchHelper.GetGroupAllFields(Global.IndexElastic, "bookname.keyword"));

    }

    private void cbCategoryTermAnal_SelectedIndexChanged(object sender, EventArgs e)
    {
        Global.IndexElastic = $"bookanalysis_{cbCategoryTermAnal.Text.ToLower()}";

    }

    private void btnStartTermAnal_Click(object sender, EventArgs e)
    {
        tbContentTermAnal.Text = "";

        lbTopicsTermAnal.Items.Clear();
        if ( DataTermDocs is not null) DataTermDocs.Clear();
        var data = ElasticSearchHelper.SearchTermByBookName(Global.IndexElastic, cbBooknameTermAnal.Text, tbSearchTermAnal.Text);



        var topicCounts = data.Select(a => new { tp=a.topic,count=1 }).GroupBy(a => a.tp).Select(a => new { tp = a.Key, count = a.Sum(s => s.count) }).ToList();

        foreach (var item in topicCounts)
        {
            lbTopicsTermAnal.Items.Add("[" + item.tp + "]    (" + (int)(((float)item.count / data.Count) * 100) + "%)    " + item.count);

        }

        DataTermDocs = data;
    }

    private void lbTopicsTermAnal_SelectedIndexChanged(object sender, EventArgs e)
    {
        tbContentTermAnal.Text = "";

        offsetDocTerm = 0;
        TopicTermDocs = null;
        var selecitem = lbTopicsTermAnal.SelectedItem.ToString();
        var topic = selecitem.Substring(selecitem.IndexOf("[")+1, selecitem.LastIndexOf("]")-1);

        TopicTermDocs = DataTermDocs.Where(a => a.topic == topic).Select(a => a.content).ToArray();

        var texts = TopicTermDocs[0].Split(tbSearchTermAnal.Text.TrimEnd().TrimStart());
        for(int i=0;i<texts.Length;i++)
        {
            AppendText(tbContentTermAnal, texts[i], Color.White);
            if (i != texts.Length - 1) AppendText(tbContentTermAnal, tbSearchTermAnal.Text, Color.Yellow);
        }

    }

    private void btnNextTermAnal_Click(object sender, EventArgs e)
    {
        if (offsetDocTerm == TopicTermDocs.Count() - 1)
        {
            MessageBox.Show("End Of Docs!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        tbContentTermAnal.Text ="";
        offsetDocTerm++;
        var texts = TopicTermDocs[offsetDocTerm].Split(tbSearchTermAnal.Text.TrimEnd().TrimStart());
        for (int i = 0; i < texts.Length; i++)
        {
            AppendText(tbContentTermAnal, texts[i], Color.White);
            if (i != texts.Length - 1) AppendText(tbContentTermAnal, tbSearchTermAnal.Text, Color.Yellow);
        }
    }

    private void btnPrevTermAnal_Click(object sender, EventArgs e)
    {
        if (offsetDocTerm == 0)
        {
            MessageBox.Show("Start Of Docs!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        tbContentTermAnal.Text = "";
        offsetDocTerm--;
        var texts = TopicTermDocs[offsetDocTerm].Split(tbSearchTermAnal.Text.TrimEnd().TrimStart());
        for (int i = 0; i < texts.Length; i++)
        {
            AppendText(tbContentTermAnal, texts[i], Color.White);
            if (i != texts.Length - 1) AppendText(tbContentTermAnal, tbSearchTermAnal.Text, Color.Yellow);
        }
    }
}