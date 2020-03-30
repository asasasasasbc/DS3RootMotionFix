//DSRootMotionFixByFS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DSRootMotionFixByFS
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
           // Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run(new Form1());
            ConvertToString();
        }

        static string[] settingString = { "X", "Y", "Z", "1" };
        static int frameDelay = 0;

        static void ConvertToString()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML FILES|*.xml|HKX FILES|*.hkx";
            openFileDialog.Multiselect = true;
            bool flag = openFileDialog.ShowDialog() == DialogResult.OK;
            if (!flag) { return; }
            if (flag)
            {
                string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string convertStr = File.ReadAllText(assemblyPath + "\\Settings.ini");
                string[] tempSettings = convertStr.Split(',');
                settingString[0] = tempSettings[0];
                settingString[1] = tempSettings[1];
                settingString[2] = tempSettings[2];
                settingString[3] = tempSettings[3];
                frameDelay = int.Parse(tempSettings[4]);
                string masterName = tempSettings[5];
                string coordinatePrefix = tempSettings[6];

                string[] fileNames = openFileDialog.FileNames;
                for (int i = 0; i < fileNames.Length; i++)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    string str = fileNames[i];
                    try
                    {
                        using (FileStream fileStream = new FileStream(openFileDialog.FileNames[i], FileMode.Open, FileAccess.Read))
                        {
                            XmlReader xmlReader = XmlReader.Create(fileStream);
                            while (xmlReader.Read())
                            {
                                bool flag2 = xmlReader.GetAttribute("name") == "referenceFrameSamples";
                                if (flag2)
                                {
                                    stringBuilder.AppendLine(xmlReader.ReadInnerXml().Replace(")", ")" + Environment.NewLine));
                                }
                            }
                            //  stringBuilder.AppendLine("");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ERROR loading " + str + Environment.NewLine + ex.ToString());
                    }

                    //Divide the numbers.
                    char[] delimiterChars = { ' ', ',', '(', ')', '\t', '\n', '\r' };
                    string orgStr = stringBuilder.ToString();
                    string[] arrays = orgStr.Split(delimiterChars);
                    List<string> xArray = new List<string>();
                    List<string> yArray = new List<string>();
                    List<string> zArray = new List<string>();
                    stringBuilder.Clear();
                    int currentPos = 0; //0 is x, 1 is y, 2 is z, 3 is 4th


                    foreach (var e in arrays)
                    {
                        if (e.Length > 1)
                        {
                            if (currentPos == 0)
                            {
                                //   stringBuilder.AppendLine("X:[" + e + "]");
                                xArray.Add(e);
                                currentPos++;
                            }
                            else if (currentPos == 1)
                            {
                                //  stringBuilder.AppendLine("Y:[" + e + "]");
                                yArray.Add(e);
                                currentPos++;
                            }
                            else if (currentPos == 2)
                            {
                                //stringBuilder.AppendLine("Z:[" + e + "]");
                                zArray.Add(e);
                                currentPos++;
                            }
                            else
                            {
                                currentPos = 0;
                            }


                        }
                    }
                    //Read the Settings.ini:


                    stringBuilder.AppendLine("objName = \"" + masterName + "\"");
                    stringBuilder.AppendLine("curobject = execute (\"$'\"+objName + \"'\")");
                    stringBuilder.AppendLine("");
                    stringBuilder.AppendLine("max tool animmode");
                    stringBuilder.AppendLine("set animate on");






                    for (int j = 0; j < xArray.Count; j++)
                    {
                        stringBuilder.AppendLine("at time " + (j + frameDelay) + "			in coordsys " + coordinatePrefix + " curobject.pos = [" + OutputPosition(xArray[j], yArray[j], zArray[j]) + "]");

                    }
                    stringBuilder.AppendLine("max tool animmode");
                    stringBuilder.AppendLine("set animate off");



                    WriteToFile(stringBuilder.ToString(), str + "RM.ms");
                }
                //   this.sourceBox.Text = stringBuilder.ToString();

            }




        }

        static string OutputPosition(string a, string b, string c)
        {
            string ans = "";
            float x = 0;
            float y = 0;
            float z = 0;
            float scale = float.Parse(settingString[3]);
            string cs = settingString[0];
            switch (cs)
            {
                case "X":
                    x = float.Parse(a) * scale;
                    break;
                case "Y":
                    x = float.Parse(b) * scale;
                    break;
                case "Z":
                    x = float.Parse(c) * scale;
                    break;
                case "-X":
                    x = float.Parse(a) * scale * -1;
                    break;
                case "-Y":
                    x = float.Parse(b) * scale * -1;
                    break;
                case "-Z":
                    x = float.Parse(c) * scale * -1;
                    break;
            }
            cs = settingString[1];
            switch (cs)
            {
                case "X":
                    y = float.Parse(a) * scale;
                    break;
                case "Y":
                    y = float.Parse(b) * scale;
                    break;
                case "Z":
                    y = float.Parse(c) * scale;
                    break;
                case "-X":
                    y = float.Parse(a) * scale * -1;
                    break;
                case "-Y":
                    y = float.Parse(b) * scale * -1;
                    break;
                case "-Z":
                    y = float.Parse(c) * scale * -1;
                    break;
            }

            cs = settingString[2];
            switch (cs)
            {
                case "X":
                    z = float.Parse(a) * scale;
                    break;
                case "Y":
                    z = float.Parse(b) * scale;
                    break;
                case "Z":
                    z = float.Parse(c) * scale;
                    break;
                case "-X":
                    z = float.Parse(a) * scale * -1;
                    break;
                case "-Y":
                    z = float.Parse(b) * scale * -1;
                    break;
                case "-Z":
                    z = float.Parse(c) * scale * -1;
                    break;
            }

            return x + ", " + y + ", " + z;

        }

        static void WriteToFile(string content, string path)
        {
            File.WriteAllText(path, content);

        }

    }
}
