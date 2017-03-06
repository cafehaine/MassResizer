using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Drawing.RotateFlipType;

namespace MassResizer
{
    class ResizerThread
    {
        public enum ResizeType : int
        {
            Fit = 0,
            Fill = 1,
            FitNoBorders = 2
        }

        public int Completed = 0;
        public object CompletedLock = new object();
        public Thread Thread;

        private int startIndex;
        private int endIndex;
        private List<string> toConvert;
        private string outputPath;
        private Size surface;
        private ResizeType type;

        public ResizerThread(List<string> files, string outputPath, int start,
            int end, Size surfaceSize, ResizeType resizeType)
        {
            startIndex = start;
            endIndex = end;
            toConvert = files;
            this.outputPath = outputPath;
            surface = surfaceSize;
            type = resizeType;
            ThreadStart threadStart = new ThreadStart(Convert);
            Thread = new Thread(threadStart);
        }

        public void Convert()
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                string file = toConvert[i];
                Bitmap input = new Bitmap(file);
                if (Array.IndexOf(input.PropertyIdList,274) > -1)
                {
                    byte dir = input.GetPropertyItem(274).Value[0];
                    switch (dir)
                    {
                        case 2:
                            input.RotateFlip(RotateNoneFlipX);
                            break;
                        case 3:
                            input.RotateFlip(Rotate180FlipNone);
                            break;
                        case 4:
                            input.RotateFlip(Rotate180FlipX);
                            break;
                        case 5:
                            input.RotateFlip(Rotate90FlipX);
                            break;
                        case 6:
                            input.RotateFlip(Rotate90FlipNone);
                            break;
                        case 7:
                            input.RotateFlip(Rotate270FlipX);
                            break;
                        case 8:
                            input.RotateFlip(Rotate270FlipNone);
                            break;
                    }
                }
                Bitmap output;
                switch (type)
                {
                    case ResizeType.Fill:
                        output = Conversion.ScaleToFill(input, surface);
                        break;
                    case ResizeType.Fit:
                        output = Conversion.ScaleToFit(input, surface);
                        break;
                    default:
                        output = Conversion.ScaleToFitNoBrdrs(input, surface);
                        break;
                }
                input.Dispose();

                string outputFile = outputPath +
                    Path.DirectorySeparatorChar + Path.GetFileName(file);
                output.Save(outputFile);
                output.Dispose();
                Completed++;
            }
        }
    }
}
