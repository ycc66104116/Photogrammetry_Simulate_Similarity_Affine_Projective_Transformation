using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageProcessing;
using System.IO;
using Microsoft.Win32;

namespace WpfApp1
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
        LOCImage OImage = null;
        LOCImage ProcessedImage = null;
        String Filename;
        float sx;
        float sy;
        float alpha;
        float beta;
        float x0;
        float y0;

        float ome;
        float ph;
        float kap;

        float tx;
        float ty;
        float tz;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.ShowDialog();
            Filename = OFD.FileName;
            Original.Source = new BitmapImage(new Uri(Filename));
            OImage = new LOCImage(Filename, Int32Rect.Empty);
        }



        private void Transform_Click(object sender, RoutedEventArgs e)
        {
            double[,] A = new double[2, 2] { { sx * Math.Cos(alpha * Math.PI / 180), -sy * Math.Sin((alpha + beta) * Math.PI / 180) }, { sx * Math.Sin(alpha * Math.PI / 180), sy * Math.Cos((alpha + beta) * Math.PI / 180) } };
            double[,] a = new double[2, 1] { { x0 }, { y0 } };

            //正算
            AffineTransform Affine = new AffineTransform(3);            
                Affine.Coeffs = new float[6];
                Affine.Coeffs[0] = (float)A[0,0];//a1
                Affine.Coeffs[1] = (float)A[0,1];//a2
                Affine.Coeffs[2] = (float)a[0,0];//a0
                Affine.Coeffs[3] = (float)A[1,0];//b1
                Affine.Coeffs[4] = (float)A[1,1];//b2
                Affine.Coeffs[5] = (float)a[1,0];//b0
                forwardcofficients.Text = "forwardcoefficients\n" + "a0=" + Affine.Coeffs[2].ToString() + "\na1=" + Affine.Coeffs[0].ToString() + "\na2=" + Affine.Coeffs[1].ToString() + "\nb0=" + Affine.Coeffs[5].ToString() + "\nb1=" + Affine.Coeffs[3].ToString() + "\nb2=" + Affine.Coeffs[4].ToString();

                float[] xc = new float[4];
                float[] yc = new float[4];

                Affine.Transform(0 - OImage.Width / 2, -0+ OImage.Height / 2);               
                xc[0] = Affine.TransformPt[0];
                yc[0] = Affine.TransformPt[1];
                Affine.Transform(OImage.Width - OImage.Width / 2, -0 + OImage.Height / 2);
                xc[1] = Affine.TransformPt[0];
                yc[1] = Affine.TransformPt[1];
                Affine.Transform(0 - OImage.Width / 2, -OImage.Height + OImage.Height / 2);
                xc[2] = Affine.TransformPt[0];
                yc[2] = Affine.TransformPt[1];
                Affine.Transform(OImage.Width - OImage.Width / 2, -OImage.Height + OImage.Height / 2);
                xc[3] = Affine.TransformPt[0];
                yc[3] = Affine.TransformPt[1];
                float xmax = xc.Max();
                float ymax = yc.Max();
                float xmin = xc.Min();
                float ymin= yc.Min();
            int newWidth = (int)(xmax - xmin);
            int newHeight = (int)(ymax - ymin);


            //反算
            //求逆矩陣
            double det = A[0, 0] * A[1, 1] - A[0, 1] * A[1, 0];
            double[,] InverseA = new double[2, 2] { { A[1, 1]/ det, -A[0, 1] / det }, { -A[1, 0] / det, A[0, 0] / det } };
            double[,] NEWA = InverseA;
            double[,] NEWa = new double[2, 1] { { NEWA[0,0]*-a[0,0]+ NEWA[0, 1] * -a[1, 0] }, { NEWA[1, 0] * -a[0, 0] + NEWA[1, 1] * -a[1, 0] } };
         
            AffineTransform Affine2 = new AffineTransform(3);
            Affine2.Coeffs = new float[6];
            Affine2.Coeffs[0] = (float)NEWA[0, 0];
            Affine2.Coeffs[1] = (float)NEWA[0, 1];
            Affine2.Coeffs[2] = (float)NEWa[0, 0];
            Affine2.Coeffs[3] = (float)NEWA[1, 0];
            Affine2.Coeffs[4] = (float)NEWA[1, 1]; 
            Affine2.Coeffs[5] = (float)NEWa[1, 0];
            backwardcofficients.Text = "backwardcoefficients\n" + "a0=" + Affine2.Coeffs[2].ToString() + "\na1=" + Affine2.Coeffs[0].ToString() + "\na2=" + Affine2.Coeffs[1].ToString() + "\nb0=" + Affine2.Coeffs[5].ToString() + "\nb1=" + Affine2.Coeffs[3].ToString() + "\nb2=" + Affine2.Coeffs[4].ToString();
            //設定新影像大小
            ProcessedImage = new LOCImage(newWidth, newHeight, 96, 96, PixelFormats.Bgr24, null);
            for (int i = 0; i < newWidth; i++)
                {
                    int Index = 0;
                    for (int j = 0; j < newHeight; j++)
                    {
                        Index = (j * newWidth + i) * 3;
                        Affine2.Transform(i - newWidth / 2, -j + newHeight / 2);
                        for (int k = 0; k < ProcessedImage.NumberOfBands; k++)
                        {
                            ProcessedImage.ByteData[Index + k] = (byte)Interpolation.Bilinear(OImage.ByteData, OImage.Width, OImage.Height, 3, 1, Affine2.TransformPt[0] +OImage.Width / 2, -Affine2.TransformPt[1] + OImage.Height / 2, k);
                        }
                    }


                }
                string ProcessedFile = "C:\\hw2\\transformed.tif";
                ProcessedImage.Save(ProcessedFile, ImageFormat.Tiff);
                using (var stream = new FileStream(ProcessedFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Process.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
        }

        public void X_scale_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            
            bool valid = float.TryParse(objTextBox.Text, out sx);
        }

        public void Y_scale_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            
            bool valid = float.TryParse(objTextBox.Text, out sy);
        }

        public void Rotation_angle_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
           
            bool valid = float.TryParse(objTextBox.Text, out alpha);
        }

        public void Shearing_angle_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            
            bool valid = float.TryParse(objTextBox.Text, out beta);
        }

        public void X_translation_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            
            bool valid = float.TryParse(objTextBox.Text, out x0);
        }

        public void Y_translation_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            
            bool valid = float.TryParse(objTextBox.Text, out y0);
        }

        private void Omega_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;

            bool valid = float.TryParse(objTextBox.Text, out ome);
        }

        private void Phi_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;

            bool valid = float.TryParse(objTextBox.Text, out ph);
        }

        private void Kappa_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;

            bool valid = float.TryParse(objTextBox.Text, out kap);
        }

        private void Ty_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;

            bool valid = float.TryParse(objTextBox.Text, out ty);
        }

        private void Tx_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;

            bool valid = float.TryParse(objTextBox.Text, out tx);
        }

        private void Tz_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;

            bool valid = float.TryParse(objTextBox.Text, out tz);
        }

        private void Mx_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;

            bool valid = float.TryParse(objTextBox.Text, out sx);
        }

        private void My_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;

            bool valid = float.TryParse(objTextBox.Text, out sy);
        }

      

        private void Projective_transform_Click(object sender, RoutedEventArgs e)
        {
            //正算
            double r11 = Math.Cos(ph* Math.PI / 180) * Math.Cos(kap* Math.PI / 180);
            double r12 =-Math.Cos(ph * Math.PI / 180) * Math.Sin(kap * Math.PI / 180);
            double r13 = Math.Sin(ph * Math.PI / 180);
            double r21 = Math.Sin(ome * Math.PI / 180) * Math.Sin(ph * Math.PI / 180) * Math.Cos(kap * Math.PI / 180) + Math.Cos(ome * Math.PI / 180) * Math.Sin(kap * Math.PI / 180);
            double r22 =-Math.Sin(ome * Math.PI / 180) * Math.Sin(ph * Math.PI / 180) * Math.Sin(kap * Math.PI / 180) + Math.Cos(ome * Math.PI / 180) * Math.Cos(kap * Math.PI / 180);
            double r23 =-Math.Sin(ome * Math.PI / 180) * Math.Cos(ph * Math.PI / 180);
            double r31 =-Math.Cos(ome * Math.PI / 180) * Math.Sin(ph * Math.PI / 180) * Math.Cos(kap * Math.PI / 180) + Math.Sin(ome * Math.PI / 180) * Math.Sin(kap * Math.PI / 180); 
            double r32 = Math.Cos(ome * Math.PI / 180) * Math.Sin(ph * Math.PI / 180) * Math.Sin(kap * Math.PI / 180) + Math.Sin(ome * Math.PI / 180) * Math.Cos(kap * Math.PI / 180); 
            double r33 = Math.Cos(ome * Math.PI / 180) * Math.Cos(ph * Math.PI / 180);
            

            ProjectiveTransform projective = new ProjectiveTransform(2.5f);
            projective.Coeffs = new float[8];
            float a0, a1, a2, b0, b1, b2, c1, c2;
            a1=projective.Coeffs[0] = (float)((sx * r11) / (r33 + tz));//a1
            a2=projective.Coeffs[1] = (float)((sx * r12) / (r33 + tz));//a2
            a0=projective.Coeffs[2] = (float)((sx * r13 + tx) / (r33 + tz));//a0
            b1=projective.Coeffs[3] = (float)((sy * r21) / (r33 + tz));//b1
            b2=projective.Coeffs[4] = (float)((sy * r22) / (r33 + tz));//b2
            b0=projective.Coeffs[5] = (float)((sy * r23 + ty) / (r33 + tz));//b0
            c1=projective.Coeffs[6] = (float)((r31) / (r33 + tz));//c1
            c2=projective.Coeffs[7] = (float)((r32) / (r33 + tz));//c2
            forwardcofficients.Text = "forwardcoefficients\n" + "a0=" + projective.Coeffs[2].ToString() + "\na1=" + projective.Coeffs[0].ToString() + "\na2=" + projective.Coeffs[1].ToString() + "\nb0=" + projective.Coeffs[5].ToString() + "\nb1=" + projective.Coeffs[3].ToString() + "\nb2=" + projective.Coeffs[4].ToString() + "\nc1=" + projective.Coeffs[6].ToString() + "\nc2=" + projective.Coeffs[7].ToString();

            float[] xc = new float[4];
            float[] yc = new float[4];

            projective.Transform(0 - OImage.Width / 2, -0 + OImage.Height / 2);
            xc[0] = projective.TransformPt[0];
            yc[0] = projective.TransformPt[1];
            projective.Transform(OImage.Width - OImage.Width / 2, -0 + OImage.Height / 2);
            xc[1] = projective.TransformPt[0];
            yc[1] = projective.TransformPt[1];
            projective.Transform(0 - OImage.Width / 2, -OImage.Height + OImage.Height / 2);
            xc[2] = projective.TransformPt[0];
            yc[2] = projective.TransformPt[1];
            projective.Transform(OImage.Width - OImage.Width / 2, -OImage.Height + OImage.Height / 2);
            xc[3] = projective.TransformPt[0];
            yc[3] = projective.TransformPt[1];
            float xmax = xc.Max();
            float ymax = yc.Max();
            float xmin = xc.Min();
            float ymin = yc.Min();
            int newWidth = (int)(xmax - xmin);
            int newHeight = (int)(ymax - ymin);
            //int newWidth = OImage.Width;
            //int newHeight = OImage.Height;
            //int OffsetWidth = 0;
            //int OffsetHeight = 0;

            //int newWidth = 3000;
            //int newHeight = 3000;
            int OffsetWidth = (int)(xmax + xmin) / 2;
           int OffsetHeight = (int)(ymax + ymin) / 2;

            //反算
            //求逆矩陣


            ProjectiveTransform projective2 = new ProjectiveTransform(2.5f);
            projective2.Coeffs = new float[8];
            projective2.Coeffs[0] = (float)((b2 - b0 * c2) / (a1 * b2 - a2 * b1));//a1
            projective2.Coeffs[1] = (float)((a0 * c2 - a2) / (a1 * b2 - a2 * b1));//a2
            projective2.Coeffs[2] = (float)((a2 * b0 - a0 * b2) / (a1 * b2 - a2 * b1));//a0
            projective2.Coeffs[3] = (float)((b0 * c1 - b1) / (a1 * b2 - a2 * b1));//b1
            projective2.Coeffs[4] = (float)((a1 - a0 * c1) / (a1 * b2 - a2 * b1));//b2
            projective2.Coeffs[5] = (float)((a0 * b1 - a1 * b0) / (a1 * b2 - a2 * b1));//b0
            projective2.Coeffs[6] = (float)((b1 * c2 - b2 * c1) / (a1 * b2 - a2 * b1));//c1
            projective2.Coeffs[7] = (float)((a2 * c1 - a1 * c2) / (a1 * b2 - a2 * b1));//c2
            backwardcofficients.Text = "backwardcoefficients\n" + "a0=" + projective2.Coeffs[2].ToString() + "\na1=" + projective2.Coeffs[0].ToString() + "\na2=" + projective2.Coeffs[1].ToString() + "\nb0=" + projective2.Coeffs[5].ToString() + "\nb1=" + projective2.Coeffs[3].ToString() + "\nb2=" + projective2.Coeffs[4].ToString() + "\nc1=" + projective.Coeffs[6].ToString() + "\nc2=" + projective.Coeffs[7].ToString();
            //設定新影像大小
            ProcessedImage = new LOCImage(newWidth, newHeight, 96, 96, PixelFormats.Bgr24, null);
            for (int i = 0; i < newWidth; i++)
            {
                int Index = 0;
                for (int j = 0; j < newHeight; j++)
                {
                    Index = (j * newWidth + i) * 3;
                    projective2.Transform(i - newWidth / 2+OffsetWidth , -j + newHeight / 2 +OffsetHeight);
                    for (int k = 0; k < ProcessedImage.NumberOfBands; k++)
                    {
                        ProcessedImage.ByteData[Index + k] = (byte)Interpolation.Bilinear(OImage.ByteData, OImage.Width, OImage.Height, 3, 1, projective2.TransformPt[0] + OImage.Width / 2, -projective2.TransformPt[1] + OImage.Height / 2, k);
                    }
                }


            }
            string ProcessedFile = "C:\\hw2\\transformed2.tif";
            ProcessedImage.Save(ProcessedFile, ImageFormat.Tiff);
            using (var stream = new FileStream(ProcessedFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Process.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }
    }
}
