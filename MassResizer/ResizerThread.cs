using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
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
        public Thread Thread;

        private Queue<string> toConvert;
        private string outputPath;
        private Size surface;
        private ResizeType type;

        public ResizerThread(Queue<string> files, string outputPath,
            Size surfaceSize, ResizeType resizeType)
        {
            toConvert = files;
            this.outputPath = outputPath;
            surface = surfaceSize;
            type = resizeType;
            ThreadStart threadStart = new ThreadStart(Convert);
            Thread = new Thread(threadStart);
        }

        public void Convert()
        {
            while (true)
            {
                string file;
                lock (toConvert)
                {
                    if (toConvert.Count == 0)
                        break;
                    file = toConvert.Dequeue();
                }
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
            }
        }
    }
}
