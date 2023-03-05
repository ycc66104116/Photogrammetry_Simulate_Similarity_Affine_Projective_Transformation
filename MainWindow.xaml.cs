using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using Microsoft.Win32;


namespace ImageProcessing
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow()

        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            string OriginalFile= "D:\\LOC_Project\\ImageProcessing\\bin\\Debug\\Papa.jpg";

            //OpenFileDialog OFD = new OpenFileDialog();
            //OFD.ShowDialog();
            //OriginalFile = OFD.FileName;
           
            LOCImage OriginalImage = new LOCImage(OriginalFile, Int32Rect.Empty);
            LOCImage ProcessedImage = new LOCImage(OriginalFile, Int32Rect.Empty);
            ProcessedImage.ByteData = new byte[OriginalImage.Width * OriginalImage.Height * 3];
            //****************//


            AffineTransform Affine = new AffineTransform(3);
            Affine.Coeffs = new float[6];
            Affine.Coeffs[0] = 0.866025404f;
            Affine.Coeffs[1] = 0.5f;
            Affine.Coeffs[2] = 0;
            Affine.Coeffs[3] = -0.5f;
            Affine.Coeffs[4] = 0.866025404f;
            Affine.Coeffs[5] = 0;


            


            //UI//
            Reference.Source = new BitmapImage(new Uri(OriginalFile));
            
            for (int i = 0; i < OriginalImage.Width; i++)
            {
                int Index = 0, Index1=0;
                for (int j = 0; j < OriginalImage.Height; j++)
                {
                    Index = (j * OriginalImage.Width + i) * 3;

                    Affine.Transform(i, j);
                    for (int k = 0; k < 3; k++)
                    {

                      
                        // ProcessedImage.ByteData[Index+k] = (byte)(255 - OriginalImage.ByteData[(int)(Index)]);
                         ProcessedImage.ByteData[Index + k] = (byte)Interpolation.Bilinear(OriginalImage.ByteData, OriginalImage.Width, OriginalImage.Height, 3, 1, Affine.TransformPt[0], Affine.TransformPt[1], k);

                        //if (Affine.TransformPt[0] >= 0 && Affine.TransformPt[0] < 960 && Affine.TransformPt[1] >= 0 && Affine.TransformPt[1] < 960)
                        //{
                        //    Index1 =(int) (Affine.TransformPt[1] * OriginalImage.Width + Affine.TransformPt[1])*3 ;
                        //    ProcessedImage.ByteData[Index +k] = OriginalImage.ByteData[Index1+k ];
                        //}
                       

                    }
                }
            }

            string ProcessedFile = "D:\\LOC_Project\\ImageProcessing\\bin\\Debug\\Papa_Processed.tif";
            ProcessedImage.Save(ProcessedFile, ImageFormat.Tiff);

            using (var stream = new FileStream(ProcessedFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Target.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }

            

        }


    }
}
