using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageProcessing
{


    public struct SimilarityTransform
    {
        float BlunderOrder;
        public bool Convergent;
        public float RMSE;
        public int MatchPoints;
        public float[] Coeffs;
        public float[] TransformPt;

        public SimilarityTransform(float BlunderOrder)
        {
            this.BlunderOrder = BlunderOrder;
            Convergent = false;
            Coeffs = new float[4];
            TransformPt = new float[2];
            RMSE = 0;
            MatchPoints = 0;
        }

   
        public void Transform(float OriX, float OriY, ref float TarX, ref float TarY)
        {
            TarX = (Coeffs[0] * OriX - Coeffs[1] * OriY + Coeffs[2]);
            TarY = (Coeffs[1] * OriX + Coeffs[0] * OriY + Coeffs[3]);
        }
       
    }

    public struct AffineTransform
    {
        float BlunderOrder;
        public bool Convergent;
        public float RMSE;
        public int MatchPoints;
        public float[] Coeffs;
        public float[] TransformPt;

        public AffineTransform(float BlunderOrder)
        {
            this.BlunderOrder = BlunderOrder;
            Convergent = false;
            Coeffs = new float[6];
            TransformPt = new float[2];
            RMSE = 0;
            MatchPoints = 0;
        }


   

        public float[] Transform(float OriX, float OriY)
        {
            TransformPt[0] = (Coeffs[0] * OriX + Coeffs[1] * OriY + Coeffs[2]);
            TransformPt[1] = (Coeffs[3] * OriX + Coeffs[4] * OriY + Coeffs[5]);
            return TransformPt;
        }

       

    }

    public struct ProjectiveTransform
    {
        float BlunderOrder;
        public bool Convergent;
        public float RMSE;
        public int MatchPoints;
        public float[] Coeffs;
        public float[] TransformPt;
        public int NumOfCoef;
        public ProjectiveTransform(float BlunderOrder)
        {
            this.BlunderOrder = BlunderOrder;
            Convergent = false;
            Coeffs = new float[8];
            TransformPt = new float[2];
            RMSE = 0;
            MatchPoints = 0;
            NumOfCoef = 8;
        }

     

      


      
      


        public void Transform(float TarX, float TarY, ref float RefX, ref float RefY)
        {
            float A = (Coeffs[0] * TarX + Coeffs[1] * TarY + Coeffs[2]);
            float B = (Coeffs[3] * TarX + Coeffs[4] * TarY + Coeffs[5]);
            float C = (Coeffs[6] * TarX + Coeffs[7] * TarY + 1);
            if (Coeffs == null)
            {
                return;
            }
            if (Coeffs.GetLength(0) == 8)
            {
                RefX = A / C;
                RefY = B / C;
            }
            else
            {
                float r2 = TarX * TarX + TarY * TarY, r4 = r2 * r2, r6 = r2 * r4;
                float K1 = Coeffs[8], K2 = Coeffs[9], K3 = Coeffs[10], P1 = Coeffs[11], P2 = Coeffs[12];

                RefX = A / C + ((K1 * r2 + K2 * r4 + K3 * r6) * TarX + P1 * (r2 + 2 * TarX * TarX) + 2 * P2 * TarX * TarY);
                RefY = B / C + ((K1 * r2 + K2 * r4 + K3 * r6) * TarY + P2 * (r2 + 2 * TarY * TarY) + 2 * P1 * TarX * TarY);
            }

        }

        public void Transform(float TarX, float TarY)
        {
            float A = (Coeffs[0] * TarX + Coeffs[1] * TarY + Coeffs[2]);
            float B = (Coeffs[3] * TarX + Coeffs[4] * TarY + Coeffs[5]);
            float C = (Coeffs[6] * TarX + Coeffs[7] * TarY + 1);

            if (NumOfCoef == 8)
            {
                TransformPt[0] = A / C;
                TransformPt[1] = B / C;
            }
            else
            {
                float X2 = TarX * TarX, Y2 = TarY * TarY, XY = 2 * TarX * TarY, r2 = X2 + Y2, r4 = r2 * r2, r6 = r2 * r4;
                float K1 = Coeffs[8], K2 = Coeffs[9], K3 = Coeffs[10], P1 = Coeffs[11], P2 = Coeffs[12];

                TransformPt[0] = A / C + ((K1 * r2 + K2 * r4 + K3 * r6) * TarX + P1 * (r2 + 2 * X2) + P2 * XY);
                TransformPt[1] = B / C + ((K1 * r2 + K2 * r4 + K3 * r6) * TarY + P2 * (r2 + 2 * Y2) + P1 * XY);
            }

        }

 


    }

    struct BlunderRemoval
    {
     

    }



}
    
