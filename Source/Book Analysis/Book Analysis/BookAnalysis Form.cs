using Book_Analysis.Classes;
using Book_Analysis.Models;

namespace Book_Analysis;

public partial class Book_Analysis : Form
{
    static string[]? BookDocs;
    static int offsetDoc = 0;
    public Book_Analysis()
    {
        InitializeComponent();
    }
    private async void Book_Analysis_Load(object sender, EventArgs e)
    {
        cbCategoryLearn.Items.Clear();
        cbCategoryValidation.Items.Clear();
        cbCategoryLearn.Items.AddRange(await ElasticSearchHelper.GetIndex());
        cbCategoryValidation.Items.AddRange(await ElasticSearchHelper.GetIndex());


    }

    private void btnBrowsBookFile_Click(object sender, EventArgs e)
    {
        //var fileContent = string.Empty;
        var filePath = string.Empty;

        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
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


            tbContentLearn.Text = GetDoc();
            btnLearn.Enabled = true;
            //tbBookLearn.Text = filename.Substring(0, filename.IndexOf("."));

        }

    }

    private void btnLearn_Click(object sender, EventArgs e)
    {
        //var filename = Path.GetFileName(tbBookFile.Text);
        var topic = cbTopicLearn.Text;
        var header = cbHeaderLearn.Text;
        ElasticSearchHelper.InsertData(new[] { new BookInfoModel
        {
            bookname=tbBookLearn.Text,
            //header= header.Contains("(") ? header.Substring(0,header.IndexOf("(")):header,
            content= tbContentLearn.Text,
            publishdate= dtpPublishDateLearn.Value,
            topic =topic.Contains("(") ? topic.Substring(0,topic.IndexOf("(")):topic,
            eventdate = DateTime.Now ,
            //header_topic=(header.Contains("(") ? header.Substring(0,header.IndexOf("(")):header)+"_"+(topic.Contains("(") ? topic.Substring(0,topic.IndexOf("(")):topic)
        } });

        if (offsetDoc == BookDocs.Length) { btnLearn.Enabled = false; btnReadBook.Enabled = true; return; }
        tbContentLearn.Text = GetDoc();


    }

    public string GetDoc()
    {
        var doc = "";
        do
        {

            doc = BookDocs[offsetDoc];
            var result = ElasticSearchHelper.MoreLikeThisQuery(doc);

            var disRes = result.Select(a => new { a.topic, a.score }).Distinct().ToList();

            var minRes = disRes.GroupBy(a => a.topic).Select(a => new { tp=a.Key, Avg = a.Sum(s => s.score) }).ToList();

            cbHeaderLearn.Items.Clear(); cbHeaderLearn.Text = "";
            cbTopicLearn.Items.Clear(); cbTopicLearn.Text = "";

            if (result != null && result.Count > 0)
            {
                //cbHeaderLearn.Items.AddRange(result.OrderByDescending(a=>a.score).Take(10).Select((a,b)=> a.header+"("+a.score+")").ToArray());
                //cbHeaderLearn.Items.AddRange(result.OrderByDescending(a => a.score).Take(10).Select((a, b) => a.header_topic.Substring(0, a.header_topic.IndexOf("_")) + "(" + a.score + ")").ToArray());
                //cbHeaderLearn.SelectedIndex = 0;
                //cbTopicLearn.Items.AddRange(result.OrderByDescending(a => a.score).Take(10).Select((a, b) => a.topic + "(" + a.score + ")").ToArray());
                //cbTopicLearn.Items.AddRange(result.OrderByDescending(a => a.score).Take(10).Select((a, b) => a.header_topic.Substring(a.header_topic.IndexOf("_") + 1) + "(" + a.score + ")").ToArray());
                cbTopicLearn.Items.AddRange(minRes.OrderByDescending(a => a.Avg).Take(10).Select(a => a.tp + "(" + a.Avg + ")").ToArray());
                cbTopicLearn.SelectedIndex = 0;

            }

            offsetDoc++;
        } while (doc == "" || doc == "\n" || doc == "\r\n");

        return doc;
    }



    private void btnIgnore_Click(object sender, EventArgs e)
    {
        tbContentLearn.Text = GetDoc();

    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        cbHeaderLearn.Items.Clear(); cbHeaderLearn.Text = "";
        cbTopicLearn.Items.Clear(); cbTopicLearn.Text = "";

        var result = ElasticSearchHelper.MoreLikeThisQuery(tbContentLearn.Text);

        var disRes = result.Select(a => new { a.topic, a.score }).Distinct().ToList();
        var minRes = disRes.GroupBy(a => a.topic).Select(a => new { tp = a.Key, Avg = a.Sum(s => s.score) }).ToList();

        if (result != null && disRes.Count > 0)
        {
            //cbHeaderLearn.Items.AddRange(result.OrderByDescending(a=>a.score).Take(10).Select((a,b)=> a.header+"("+a.score+")").ToArray());
            //cbHeaderLearn.Items.AddRange(disRes.OrderByDescending(a => a.score).Take(10).Select(a => a.header_topic.Substring(0, a.header_topic.IndexOf("_")) + "(" + a.score + ")").ToArray());
            //cbHeaderLearn.SelectedIndex = 0;
            //cbTopicLearn.Items.AddRange(result.OrderByDescending(a => a.score).Take(10).Select((a, b) => a.topic + "(" + a.score + ")").ToArray());
            //cbTopicLearn.Items.AddRange(disRes.OrderByDescending(a => a.score).Take(10).Select(a => a.header_topic.Substring(a.header_topic.IndexOf("_") + 1) + "(" + a.score + ")").ToArray());
            cbTopicLearn.Items.AddRange(minRes.OrderByDescending(a => a.Avg).Take(10).Select(a => a.tp + "(" + a.Avg + ")").ToArray());
            cbTopicLearn.SelectedIndex = 0;

        }


    }

    private void btnAddCategoryLearn_Click(object sender, EventArgs e)
    {
        Global.IndexElastic = $"bookanalysis_{cbCategoryLearn.Text.ToLower()}";
        if (cbCategoryLearn.Text is not null)
        {
            ElasticSearchHelper.CreateIndex(cbCategoryLearn.Text.ToLower());
        }
    }

    private void cbCategoryLearn_SelectedIndexChanged(object sender, EventArgs e)
    {
        Global.IndexElastic = $"bookanalysis_{cbCategoryLearn.Text.ToLower()}";
    }


    private void btnStartValidation_Click(object sender, EventArgs e)
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

        List<List<bool>> res = Validation(Global.IndexElastic);

        tbPerformance1.Text = ((float)(res[0].Where(a => a == true).ToArray().Length) / res[0].Count) * 100 + "%";
        tbPerformance2.Text = ((float)(res[1].Where(a => a == true).ToArray().Length) / res[1].Count) * 100 + "%";
        tbPerformance3.Text = ((float)(res[2].Where(a => a == true).ToArray().Length) / res[2].Count) * 100 + "%";
        tbPerformance4.Text = ((float)(res[3].Where(a => a == true).ToArray().Length) / res[3].Count) * 100 + "%";
        tbPerformance5.Text = ((float)(res[4].Where(a => a == true).ToArray().Length) / res[4].Count) * 100 + "%";


        tbPerformanceTotal.Text = ((float)(res.Sum(l => l.Where(a => a == true).ToArray().Length)) / res.Sum(l => l.Count)) * 100 + "%";
    }

    private async void tabControl_SelectedIndexChanged(object sender, EventArgs e)
    {

        cbCategoryLearn.Items.Clear();
        cbCategoryValidation.Items.Clear();
        cbCategoryLearn.Items.AddRange(await ElasticSearchHelper.GetIndex());
        cbCategoryValidation.Items.AddRange(await ElasticSearchHelper.GetIndex());

    }

    private void cbCategoryValidation_SelectedIndexChanged(object sender, EventArgs e)
    {
        Global.IndexElastic = $"bookanalysis_{cbCategoryValidation.Text.ToLower()}";
    }



    public List<List<bool>> Validation(string indexName)
    {
        var Data = ElasticSearchHelper.SearchAlltt(indexName);
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
                    if(selfindex is not -1) result.RemoveAt(selfindex);
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
}