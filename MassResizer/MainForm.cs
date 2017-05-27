using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using static System.Environment;
using static System.Text.RegularExpressions.RegexOptions;

namespace MassResizer
{
    public partial class FormMain : Form
    {
        private static Regex resolutionRegex = new Regex("[0-9]+x[0-9]+",
            Compiled);
        private static Regex pictureRegex = new Regex(
            @".*\.(jpe?g|bmp|png|gif)", IgnoreCase | Compiled);

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
                Queue<string> queue = new Queue<string>(files);
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
                for (int i = 0; i < ProcessorCount; i++)
                    resizers.Add(new ResizerThread(queue, textBoxOutput.Text,
                        size, type));

                resizers.ForEach(r => r.Thread.Start());

                while (!resizers.TrueForAll(r =>
                    r.Thread.ThreadState == ThreadState.Stopped))
                {
                    lock (queue)
                    {
                        progressBar1.Value = files.Count - queue.Count;
                    }
                    Application.DoEvents();
                    Thread.Sleep(50);
                }
                progressBar1.Value = 0;
                MessageBox.Show("Done !");
            }
            catch (Exception x)
            {
                MessageBox.Show("A problem occured:\n" + x.Message);
            }
        }

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            AboutBox dialog = new AboutBox();
            dialog.ShowDialog(this);
        }

        private void comboBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((string)comboBoxType.SelectedItem)
            {
                case "Fit":
                    pictureBoxPreview.Image = Properties.Resources.fit;
                    break;
                case "FitNoBorders":
                    pictureBoxPreview.Image = Properties.Resources.fitnoborders;
                    break;
                case "Fill":
                    pictureBoxPreview.Image = Properties.Resources.fill;
                    break;
            }
        }
    }
}
