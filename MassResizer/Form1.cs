using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using static System.Environment;
using System.Threading;

namespace MassResizer
{
    public partial class FormMain : Form
    {
        private static Regex resolutionRegex = new Regex("[0-9]+x[0-9]+");
        private static Regex pictureRegex = new Regex(@".*\.(jpe?g|bmp|png|gif)", RegexOptions.IgnoreCase);
        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonInput_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxInput.Text = folderBrowserDialog.SelectedPath;
        }

        private void buttonOutput_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxOutput.Text = folderBrowserDialog.SelectedPath;
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (!comboBoxType.Items.Contains(comboBoxType.Text))
            {
                MessageBox.Show("Invalid scaling mode.");
                return;
            }
            if (!Directory.Exists(textBoxInput.Text) ||
                !Directory.Exists(textBoxOutput.Text))
            {
                MessageBox.Show("Invalid input/output path.");
                return;
            }
            if (!resolutionRegex.IsMatch(comboBoxResolution.Text))
            {
                MessageBox.Show("Invalid resolution.");
                return;
            }

            try
            {
                List<string> files =
                    new List<string>(Directory.GetFiles(textBoxInput.Text));
                // remove non-picture files
                files.RemoveAll(x => !pictureRegex.IsMatch(x));
                progressBar1.Maximum = files.Count;
                string[] res = comboBoxResolution.Text.Split('x');
                int width = int.Parse(res[0]);
                int height = int.Parse(res[1]);
                Size size = new Size(width, height);
                ResizerThread.ResizeType type;

                switch (comboBoxType.Text)
                {
                    case "Fill":
                        type = ResizerThread.ResizeType.Fill;
                        break;
                    case "FitNoBorders":
                        type = ResizerThread.ResizeType.FitNoBorders;
                        break;
                    default:
                        type = ResizerThread.ResizeType.Fit;
                        break;
                }

                List<ResizerThread> resizers =
                    new List<ResizerThread>(ProcessorCount);
                int startIndex = 0;
                int endIndex = files.Count / ProcessorCount;
                for (int i = 0; i < ProcessorCount - 1; i++)
                {
                    resizers.Add(new ResizerThread(files, textBoxOutput.Text,
                        startIndex, endIndex, size, type));
                    startIndex += files.Count / ProcessorCount;
                    endIndex += files.Count / ProcessorCount;
                }
                resizers.Add(new ResizerThread(files, textBoxOutput.Text,
                        startIndex, files.Count, size, type));

                resizers.ForEach(r => r.Thread.Start());

                while (!resizers.TrueForAll(r =>
                    r.Thread.ThreadState == ThreadState.Stopped))
                {
                    foreach (ResizerThread resizer in resizers)
                        if (resizer.Completed != 0)
                            lock (resizer.CompletedLock)
                            {
                                progressBar1.Value += resizer.Completed;
                                resizer.Completed = 0;
                            }
                    Application.DoEvents();
                }
                progressBar1.Value = 0;
            }
            catch (Exception x)
            {
                MessageBox.Show("A problem occured:\n" + x.Message);
            }
        }
    }
}
