using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;

namespace TextRecognition
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox2.Text = "1";
        }

        TesseractEngine ocr = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractAndCube);
        int resizeMultiplier = 1;
        Bitmap currentImage;
        bool imgSelected = false;

        private void button1_Click(object sender, EventArgs e)
        {
            imgSelected = true;
            if (currentImage != null)
                currentImage.Dispose();
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Bitmap selectedImage = Get24bppRgb(new Bitmap(dlg.FileName));
                    currentImage = ResizeImage(selectedImage, selectedImage.Width * resizeMultiplier, selectedImage.Height * resizeMultiplier);
                    pictureBox1.Image = new Bitmap(currentImage);
                    button2.Enabled = true;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (Page imgToText = ocr.Process(currentImage, PageSegMode.AutoOsd))
            {
                textBox1.AppendText(imgToText.GetText() + Environment.NewLine); 
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private static Bitmap Get24bppRgb(Image image)
        {
            var bitmap = new Bitmap(image);
            var bitmap24 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (var gr = Graphics.FromImage(bitmap24))
            {
                gr.DrawImage(bitmap, new Rectangle(0, 0, bitmap24.Width, bitmap24.Height));
            }
            return bitmap24;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                resizeMultiplier = Int32.Parse(textBox2.Text);
                if (imgSelected)
                    currentImage = ResizeImage(currentImage, currentImage.Width * resizeMultiplier, currentImage.Height * resizeMultiplier);
                
            }
            catch (FormatException)
            {
                if (textBox2.Text == "") { }
                else
                {
                    textBox2.Text = "1";
                    string currString = textBox2.Text, newString = "";
                    foreach (char c in currString)
                    {
                        if (Char.IsDigit(c))
                            newString += c;
                    }
                    textBox2.Text = newString;
                    MessageBox.Show("Please only input positive integers", "Error");
                }
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                textBox2.Text = "1";
            }
        }
    }
}
