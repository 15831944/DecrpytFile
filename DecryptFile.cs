using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace DecodeFile
{
    public partial class DecryptFile : Form
    {
        public DecryptFile()
        {
            InitializeComponent();
            DragEnter += new DragEventHandler(Form1_DragEnter);
            DragDrop += new DragEventHandler(Form1_DragDrop);
        }
        public DecryptFile(string[] args)
        {
            InitializeComponent();
            textBox1.Text = args[0];
            button2_Click(null, new EventArgs());
            Close();
        }
        public string Decrypt(string toDecrypt, string key)
        {
            StringBuilder sb = new StringBuilder(key);
            while (16 > key.Length)
            {
                sb.Append("_");
            }
            key = sb.ToString();

            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string filePath in files)
                {
                    Console.WriteLine(filePath);
                    textBox1.Text = filePath;
                }
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text)) return;

            string decrypt = Decrypt(StreamReadFile(textBox1.Text, false), "EncryptKey");
            dynamic obj = JsonConvert.DeserializeObject(decrypt);

            string oriFileName = Path.GetFileNameWithoutExtension(textBox1.Text);
            string newFilePath = textBox1.Text.Replace(oriFileName, oriFileName + "_decrypt");

            File.Delete(newFilePath);
            File.WriteAllText(newFilePath, JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
        private string StreamReadFile(string filePath, bool splitLine)
        {
            string jsonText = "";
            string line = "";
            using (StreamReader fileJson = new StreamReader(filePath))
            {
                StringBuilder sb = new StringBuilder(jsonText);
                while ((line = fileJson.ReadLine()) != null)
                {
                    if (splitLine)
                        sb.AppendLine(line);
                    else
                        sb.Append(line);
                }
                jsonText = sb.ToString();
            }
            return jsonText;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "選擇檔案";
            //ofd.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }
    }
}
