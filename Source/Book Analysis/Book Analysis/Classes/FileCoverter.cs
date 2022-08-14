using Aspose.Pdf;
using Aspose.Pdf.Devices;
using Aspose.Pdf.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Book_Analysis.Classes
{
    public static class FileConverter
    {
        public static string PDFtoTXT(string pathfile)
        {
            // Open document
            Document pdfDocument = new Document(pathfile);
            StringBuilder builder = new StringBuilder();
            // String to hold extracted text
            string extractedText = "";

            foreach (Aspose.Pdf.Page pdfPage in pdfDocument.Pages)
            {
                using (MemoryStream textStream = new MemoryStream())
                {
                    // Create text device
                    TextDevice textDevice = new TextDevice();

                    // Set different options
                    TextExtractionOptions options = new
                    TextExtractionOptions(TextExtractionOptions.TextFormattingMode.Pure);
                    textDevice.ExtractionOptions = options;

                    // Convert the page and save text to the stream
                    textDevice.Process(pdfPage, textStream);

                    // Close memory stream
                    textStream.Close();

                    // Get text from memory stream
                    extractedText = Encoding.Unicode.GetString(textStream.ToArray());
                }
                builder.Append(extractedText);
            }

            var txtfile = pathfile.Replace(".pdf",".txt");
            // Save the text file
            File.WriteAllText(txtfile, builder.ToString());

            return txtfile;
        }
    }
}
