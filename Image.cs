using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace ImageProcessing
{

    public class LOCImage : IDisposable
    {
        public string FileName;
        public int Width, Height, NumberOfBytes, NumberOfBands, OffsetWidth, OffsetHeight, ByteBands, Stride;
        public byte[] ByteData;
        public byte[,,] ByteData2D;
        public double DpiX, DpiY;
        public PixelFormat PixelFormat;
        public BitmapMetadata Metadata;
        public BitmapSource Source;

        private bool Disposed = false;


        public LOCImage()
        { }
        public LOCImage(int Width, int Height, double DpiX, double DpiY, PixelFormat PixelFormat, BitmapMetadata Metadata)
        {
            this.Width = Width;
            this.Height = Height;
            NumberOfBands = PixelFormat.Masks.Count;
            NumberOfBytes = (PixelFormat.BitsPerPixel / NumberOfBands) / 8;
            ByteBands = NumberOfBands * NumberOfBytes;
            this.DpiX = DpiX;
            this.DpiY = DpiY;
            this.PixelFormat = PixelFormat;
            this.Metadata = Metadata;
            ByteData = new byte[Width * Height * NumberOfBytes * NumberOfBands];
        }
        public LOCImage(int Width, int Height, double DpiX, double DpiY, int NumberOfBands, byte[] ByteData)
        {
            this.Width = Width;
            this.Height = Height;
            this.NumberOfBands = NumberOfBands;
            NumberOfBytes = 1;
            ByteBands = NumberOfBands * NumberOfBytes;
            this.DpiX = DpiX;
            this.DpiY = DpiY;
            this.ByteData = ByteData;
        }


        public LOCImage(LOCImage RefImage)
        {
            Width = RefImage.Width;
            Height = RefImage.Height;

            CopyImageMetaData(RefImage);
            NumberOfBands = PixelFormat.Masks.Count;
            NumberOfBytes = (PixelFormat.BitsPerPixel / NumberOfBands) / 8;
            ByteBands = NumberOfBands * NumberOfBytes;
            Stride = Width * ByteBands;
            ByteData = new byte[Width * Height * NumberOfBytes * NumberOfBands];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        public LOCImage(string FileName, Int32Rect Rect)
        {
            this.FileName = FileName;
            Open(FileName, Rect);
        }

        public LOCImage(BitmapSource Source, Int32Rect Rect)
        {
            GetImageInfo(Source, Rect);
            GC.Collect();
        }

        public LOCImage(string FileName, Int32Rect Rect, int band)
        {
            Open(FileName, Rect);
            byte[] Convertbyte = new byte[Width * Height];
            int Index = 0;
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Index = j * Width + i;
                    if (band == 4)

                    {
                        Convertbyte[Index] = (byte)((ByteData[Index * NumberOfBands + 0] + ByteData[Index * NumberOfBands + 1] + ByteData[Index * NumberOfBands + 2]) / 3);
                    }
                    else
                    {
                        Convertbyte[Index] = ByteData[Index * NumberOfBands + band];
                    }

                }
            }
            ByteData = Convertbyte;
            NumberOfBands = 1;
            ByteBands = NumberOfBytes * NumberOfBands;
            GC.Collect();
        }


        public LOCImage(BitmapSource Source, Int32Rect Rect, int band)
        {
            GetImageInfo(Source, Rect);
            byte[] Convertbyte = new byte[Width * Height];
            int Index = 0;
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Index = j * Width + i;
                    if (band == 4)

                    {
                        Convertbyte[Index] = (byte)((ByteData[Index * NumberOfBands + 0] + ByteData[Index * NumberOfBands + 1] + ByteData[Index * NumberOfBands + 2]) / 3);
                    }
                    else
                    {
                        Convertbyte[Index] = ByteData[Index * NumberOfBands + band];
                    }

                }
            }
            ByteData = Convertbyte;
            NumberOfBands = 1;
            ByteBands = NumberOfBytes * NumberOfBands;
            GC.Collect();
        }

        private void Open(string FileName, Int32Rect Rect)
        {
            Source = BitmapDecoder.Create(new Uri(FileName), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
            GetImageInfo(Source, Rect);

            GC.Collect();
        }



        public void Save(string FileName, ImageFormat ImageFormat)
        {
            int Stride = Width * NumberOfBands * NumberOfBytes;
            BitmapSource BitmapSource = BitmapSource.Create(Width, Height, DpiX, DpiY, PixelFormat, null, ByteData, Stride);

            using (var Stream = File.Create(FileName))
            {
                switch (ImageFormat)
                {
                    case ImageFormat.Jpeg:
                        JpegBitmapEncoder JpgImage = new JpegBitmapEncoder();
                        JpgImage.Frames.Add(BitmapFrame.Create(BitmapSource, null, Metadata, null));
                        JpgImage.QualityLevel = 90;
                        JpgImage.Save(Stream);
                        JpgImage = null;
                        break;

                    case ImageFormat.Tiff:
                        TiffBitmapEncoder TiffImage = new TiffBitmapEncoder();
                        TiffImage.Frames.Add(BitmapFrame.Create(BitmapSource, null, Metadata, null));
                        TiffImage.Compression = TiffCompressOption.None;
                        TiffImage.Save(Stream);
                        TiffImage = null;
                        break;

                    case ImageFormat.Bmp:
                        BmpBitmapEncoder BmpImage = new BmpBitmapEncoder();
                        BmpImage.Frames.Add(BitmapFrame.Create(BitmapSource, null, Metadata, null));
                        BmpImage.Save(Stream);
                        BmpImage = null;
                        break;

                    case ImageFormat.Png:
                        PngBitmapEncoder PngImage = new PngBitmapEncoder();
                        PngImage.Frames.Add(BitmapFrame.Create(BitmapSource, null, Metadata, null));
                        PngImage.Save(Stream);
                        PngImage.Interlace = PngInterlaceOption.On;
                        PngImage = null;
                        break;

                    case ImageFormat.Wmp:
                        WmpBitmapEncoder WmpImage = new WmpBitmapEncoder();
                        WmpImage.Frames.Add(BitmapFrame.Create(BitmapSource, null, Metadata, null));
                        WmpImage.Save(Stream);
                        WmpImage.Lossless = false;
                        WmpImage = null;
                        break;
                }

                BitmapSource = null;
            }

            GC.Collect();
        }

        public void Save(string FileName, Int32Rect SourceRect, ImageFormat ImageFormat)
        {
            int Stride = Width * NumberOfBands * NumberOfBytes;

            BitmapSource BitmapSource = BitmapSource.Create(Width, Height, DpiX, DpiY, PixelFormat, null, ByteData, Stride);
            BitmapSource.CopyPixels(SourceRect, ByteData, Stride, 0);
            BitmapSource = BitmapSource.Create(SourceRect.Width, SourceRect.Height, DpiX, DpiY, PixelFormat, null, ByteData, Stride);

            using (var Stream = File.Create(FileName))
            {
                switch (ImageFormat)
                {
                    case ImageFormat.Jpeg:
                        JpegBitmapEncoder JpgImage = new JpegBitmapEncoder();
                        JpgImage.Frames.Add(BitmapFrame.Create(BitmapSource, null, Metadata, null));
                        JpgImage.QualityLevel = 90;
                        JpgImage.Save(Stream);
                        JpgImage = null;
                        break;

                    case ImageFormat.Tiff:
                        TiffBitmapEncoder TiffImage = new TiffBitmapEncoder();
                        TiffImage.Frames.Add(BitmapFrame.Create(BitmapSource, null, Metadata, null));
                        TiffImage.Compression = TiffCompressOption.None;
                        TiffImage.Save(Stream);
                        TiffImage = null;
                        break;

                    case ImageFormat.Bmp:
                        BmpBitmapEncoder BmpImage = new BmpBitmapEncoder();
                        BmpImage.Frames.Add(BitmapFrame.Create(BitmapSource, null, Metadata, null));
                        BmpImage.Save(Stream);
                        BmpImage = null;
                        break;

                    case ImageFormat.Png:
                        PngBitmapEncoder PngImage = new PngBitmapEncoder();
                        PngImage.Frames.Add(BitmapFrame.Create(BitmapSource, null, Metadata, null));
                        PngImage.Save(Stream);
                        PngImage.Interlace = PngInterlaceOption.On;
                        PngImage = null;
                        break;

                    case ImageFormat.Wmp:
                        WmpBitmapEncoder WmpImage = new WmpBitmapEncoder();
                        WmpImage.Frames.Add(BitmapFrame.Create(BitmapSource, null, Metadata, null));
                        WmpImage.Save(Stream);
                        WmpImage.Lossless = false;
                        WmpImage = null;
                        break;
                }

                BitmapSource = null;
            }

            GC.Collect();
        }

        public void CopyImageMetaData(LOCImage TarImage)
        {
            DpiX = TarImage.DpiX;
            DpiY = TarImage.DpiY;
            Metadata = TarImage.Metadata;
            PixelFormat = TarImage.PixelFormat;
        }

        private void GetImageInfo(BitmapSource Source, Int32Rect Rect)
        {
            NumberOfBands = Source.Format.Masks.Count;
            NumberOfBytes = (Source.Format.BitsPerPixel / NumberOfBands) / 8;
            ByteBands = NumberOfBands * NumberOfBytes;
            DpiX = Source.DpiX;
            DpiY = Source.DpiY;
            PixelFormat = Source.Format;
            Metadata = (BitmapMetadata)Source.Metadata;

            if (Rect.IsEmpty == false)
            {
                Width = Rect.Width;
                Height = Rect.Height;
                OffsetWidth = Rect.X;
                OffsetHeight = Rect.Y;
                Stride = Width * ByteBands;
                ByteData = new byte[Stride * Height];
                Source.CopyPixels(Rect, ByteData, Stride, 0);
            }
            else
            {
                Width = Source.PixelWidth;
                Height = Source.PixelHeight;
                OffsetWidth = OffsetHeight = 0;
                Stride = Width * ByteBands;
                ByteData = new byte[Stride * Height];
                Source.CopyPixels(ByteData, Stride, 0);
            }

            Source = null;
            GC.Collect();
        }


        private void GetImageInfo2D(BitmapSource Source)
        {
            Stride = 0;
            Width = Source.PixelWidth;
            Height = Source.PixelHeight;
            OffsetWidth = OffsetHeight = 0;
            DpiX = Source.DpiX;
            DpiY = Source.DpiY;
            PixelFormat = Source.Format;
            Metadata = (BitmapMetadata)Source.Metadata;

            NumberOfBands = Source.Format.Masks.Count;
            NumberOfBytes = (Source.Format.BitsPerPixel / NumberOfBands) / 8;
            ByteBands = NumberOfBands * NumberOfBytes;
            Stride = Width * NumberOfBytes * NumberOfBands;
            ByteData = new byte[Stride * Height];
            ByteData2D = new byte[Width, Height, NumberOfBands];
            Source.CopyPixels(ByteData, Stride, 0);

            int Index = 0;
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Index = (j * Width + i) * NumberOfBands;
                    ByteData2D[i, j, 0] = ByteData[Index + 0];
                    ByteData2D[i, j, 1] = ByteData[Index + 1];
                    ByteData2D[i, j, 2] = ByteData[Index + 2];
                }
            }


            Source = null;
            GC.Collect();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                ByteData = null;
                Metadata = null;
                Source = null;
                Width = Height = 0;
                DpiX = DpiY = 0;
                NumberOfBytes = NumberOfBands = 0;
                PixelFormat = PixelFormats.Default;
                GC.Collect();
            }
            Disposed = true;
        }

        ~LOCImage()
        {
            Dispose(false);
        }
       
    }
    public struct Interpolation
    {

        public static byte[] Bilinear(LOCImage Image, float x, float y)
        {
            int A, B, C, D, Row, Column, Index;
            int Value = 0;
            float dx, dy;

            byte[] DN = new byte[Image.NumberOfBands];

            if (x < 0 || y < 0 || x >= Image.Width - 1 || y >= Image.Height - 1)
            {
                return DN;
            }

            Row = (int)x;
            Column = (int)y;
            dx = x - Row;
            dy = y - Column;

            for (int i = 0; i < Image.NumberOfBands; i++)
            {
                Index = (Column * Image.Width + Row) * Image.NumberOfBands;

                A = Image.ByteData[Index + i];
                B = Image.ByteData[Index + Image.NumberOfBands + i] - A;
                C = Image.ByteData[Index + Image.Width * Image.NumberOfBands + i] - A;
                D = -A - B - C + Image.ByteData[Index + Image.Width * Image.NumberOfBands + Image.NumberOfBands + i];

                Value = (int)(A + B * dx + C * dy + D * dx * dy);

                Value = (Value > 255) ? 255 : Value;
                Value = (Value < 0) ? 0 : Value;

                DN[i] = (byte)Value;
            }
            return DN;
        }


        public static int Bilinear(byte[] ByteData, int Width, int Height, int ByteBands, int Bytes, float x, float y, int k)
        {
            int A = 0, B = 0, C = 0, D = 0, Row = 0, Column = 0, Value = 0, Index1 = 0, Index2 = 0, Index3 = 0, Index4 = 0;
            float dx = 0, dy = 0;

            if (x < 0 || y < 0 || x >= Width - 1 || y >= Height - 1)
            {
                return 0;
            }

            Row = (int)x;
            Column = (int)y;
            dx = x - Row;
            dy = y - Column;

            Index1 = (Column * Width + Row) * ByteBands;
            Index2 = Index1 + ByteBands;
            Index3 = Index1 + Width * ByteBands;
            Index4 = Index1 + Width * ByteBands + ByteBands;

            if (Bytes == 1)
            {
                A = ByteData[Index1 + k];
                B = ByteData[Index2 + k] - A;
                C = ByteData[Index3 + k] - A;
                D = -A - B - C + ByteData[Index4];

                Value = (int)(A + B * dx + C * dy + D * dx * dy);
                Value = (Value > 255) ? 255 : Value;
                Value = (Value < 0) ? 0 : Value;

            }
            else if (Bytes == 2)
            {
                A = ByteData[Index1 + k * Bytes] + ByteData[Index1 + k * Bytes + 1] * 256;
                B = ByteData[Index2 + k * Bytes] + ByteData[Index2 + k * Bytes + 1] * 256 - A;
                C = ByteData[Index3 + k * Bytes] + ByteData[Index3 + k * Bytes + 1] * 256 - A;
                D = -A - B - C + ByteData[Index4 + k * Bytes] + ByteData[Index4 + k * Bytes + 1] * 256;

                Value = (int)(A + B * dx + C * dy + D * dx * dy);
                Value = (Value > 65535) ? 65535 : Value;
                Value = (Value < 0) ? 0 : Value;

            }

            return Value;
        }


        public static void Bilinear(ref byte[] TarImage, LOCImage OriImage, int TarIndex, float x, float y)
        {
            int A = 0, B = 0, C = 0, D = 0, Row, Column, Value = 0;
            float dx, dy;
            int Index1 = 0, Index2 = 0, Index3 = 0, Index4 = 0;
            int Bytes, Bands, Width, ByteBands;
            Bytes = OriImage.NumberOfBytes;
            Bands = OriImage.NumberOfBands;
            Width = OriImage.Width;
            ByteBands = OriImage.ByteBands;

            if (x < 0 || y < 0 || x >= OriImage.Width - 1 || y >= OriImage.Height - 1)
            {

                for (int i = 0; i < Bands; i++)
                {
                    if (Bytes == 1)
                    {
                        TarImage[TarIndex + i * Bytes] = 0;
                    }
                    else if (Bytes == 2)
                    {
                        TarImage[TarIndex + i * Bytes] = 0;
                        TarImage[TarIndex + i * Bytes + 1] = 0;
                    }
                }

                return;
            }

            Row = (int)x;
            Column = (int)y;
            dx = x - Row;
            dy = y - Column;

            Index1 = (Column * Width + Row) * ByteBands;
            Index2 = Index1 + ByteBands;
            Index3 = Index1 + Width * ByteBands;
            Index4 = Index1 + Width * ByteBands + ByteBands;

            for (int i = 0; i < Bands; i++)
            {
                if (Bytes == 1)
                {
                    A = OriImage.ByteData[Index1 + i];
                    B = OriImage.ByteData[Index2 + i] - A;
                    C = OriImage.ByteData[Index3 + i] - A;
                    D = -A - B - C + OriImage.ByteData[Index4 + i];

                    Value = (int)(A + B * dx + C * dy + D * dx * dy);
                    Value = (Value > 255) ? 255 : Value;
                    Value = (Value < 0) ? 0 : Value;
                    TarImage[TarIndex + i] = (byte)Value;
                }
                else if (Bytes == 2)
                {
                    int a, b;
                    A = OriImage.ByteData[Index1 + i * Bytes] + OriImage.ByteData[Index1 + i * Bytes + 1] * 256;
                    B = OriImage.ByteData[Index2 + i * Bytes] + OriImage.ByteData[Index2 + i * Bytes + 1] * 256 - A;
                    C = OriImage.ByteData[Index3 + i * Bytes] + OriImage.ByteData[Index3 + i * Bytes + 1] * 256 - A;
                    D = -A - B - C + OriImage.ByteData[Index4 + i * Bytes] + OriImage.ByteData[Index4 + i * Bytes + 1] * 256;

                    Value = (int)(A + B * dx + C * dy + D * dx * dy);
                    Value = (Value > 65535) ? 65535 : Value;
                    Value = (Value < 0) ? 0 : Value;

                    a = Value / 256;
                    b = Value % 256;

                    TarImage[TarIndex + i * Bytes] = (byte)b;
                    TarImage[TarIndex + i * Bytes + 1] = (byte)a;
                }
            }

        }

    }

}
