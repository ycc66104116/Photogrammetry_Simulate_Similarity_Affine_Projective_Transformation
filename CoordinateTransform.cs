using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
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

        public void Adjustment(List<ImagePoints.Coordinate> Tar, List<ImagePoints.Coordinate> Ref, LeastSquare LSA)
        {

            do
            {
                int Points = Tar.Count;
                int NumberOfEquation = 2;
                Matrix<float> A = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 4, 1); //建立A矩陣，預設值為1                     
                Matrix<float> L = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 1, 1);

                for (int i = 0; i < Points; i++)
                {
                    int Index_X = 0 + i * NumberOfEquation;
                    int Index_Y = Index_X + 1;

                    A[Index_X, 0] = A[Index_Y, 1] = Tar[i].X;
                    A[Index_X, 1] = -Tar[i].Y;
                    A[Index_X, 2] = A[Index_Y, 3] = 1;
                    A[Index_X, 3] = A[Index_Y, 2] = 0;
                    A[Index_Y, 0] = Tar[i].Y;


                    L[Index_X, 0] = Ref[i].X;
                    L[Index_Y, 0] = Ref[i].Y;
                }

                LSA.Adjustment(A, L);


            } while (BlunderRemoval.RemoveBlunder(ref Tar, ref Ref, LSA, BlunderOrder) == true);


            Convergent = (LSA.RMSE > 5) ? false : true;

            Coeffs[0] = LSA.X[0, 0];
            Coeffs[1] = LSA.X[1, 0];
            Coeffs[2] = LSA.X[2, 0];
            Coeffs[3] = LSA.X[3, 0];

            RMSE = LSA.RMSE;
            MatchPoints = Ref.Count;
        }

        public void Transform(float OriX, float OriY, ref float TarX, ref float TarY)
        {
            TarX = (Coeffs[0] * OriX - Coeffs[1] * OriY + Coeffs[2]);
            TarY = (Coeffs[1] * OriX + Coeffs[0] * OriY + Coeffs[3]);
        }
        public List<ImagePoints.Coordinate> Transform(List<ImagePoints.Coordinate> OriginalCoordinateList)
        {
            List<ImagePoints.Coordinate> TransformedCoordinateList = new List<ImagePoints.Coordinate>();

            foreach (ImagePoints.Coordinate OriginalCoordinate in OriginalCoordinateList)
            {
                float X = 0, Y = 0;
                Transform(OriginalCoordinate.X, OriginalCoordinate.Y, ref X, ref Y);

                TransformedCoordinateList.Add(new ImagePoints.Coordinate(X, Y, CoordinateFormat.mm));
            }

            return TransformedCoordinateList;
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


        public void Adjustment(List<ImagePoints.Coordinate> Tar, List<ImagePoints.Coordinate> Ref, LeastSquare LSA)
        {

            if (Ref.Count < 4)
            {
                Convergent = false;
                return;
            }

            do
            {
                int Points = Tar.Count;
                const int NumberOfEquation = 2;
                Matrix<float> A = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 6, 1); //建立A矩陣，預設值為1                                                                                           
                Matrix<float> L = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 1, 1);

                Parallel.For(0, Points, i =>
                {
                    int Index_X = 0 + i * NumberOfEquation;
                    int Index_Y = Index_X + 1;

                    A[Index_X, 0] = A[Index_Y, 3] = Tar[i].X;
                    A[Index_X, 1] = A[Index_Y, 4] = Tar[i].Y;
                    A[Index_X, 2] = A[Index_Y, 5] = 1;
                    A[Index_X, 3] = A[0 + i * 2, 4] = A[0 + i * 2, 5] = 0;
                    A[Index_Y, 0] = A[1 + i * 2, 1] = A[1 + i * 2, 2] = 0;


                    L[Index_X, 0] = Ref[i].X;
                    L[Index_Y, 0] = Ref[i].Y;
                });

                LSA.Adjustment(A, L);

            } while (BlunderRemoval.RemoveBlunder(ref Tar, ref Ref, LSA, BlunderOrder) == true);

            Convergent = (LSA.RMSE > 5) ? false : true;

            Coeffs[0] = LSA.X[0, 0];
            Coeffs[1] = LSA.X[1, 0];
            Coeffs[2] = LSA.X[2, 0];
            Coeffs[3] = LSA.X[3, 0];
            Coeffs[4] = LSA.X[4, 0];
            Coeffs[5] = LSA.X[5, 0];
            RMSE = LSA.RMSE;
            MatchPoints = Ref.Count;
        }

        public float[] Transform(float OriX, float OriY)
        {
            TransformPt[0] = (Coeffs[0] * OriX + Coeffs[1] * OriY + Coeffs[2]);
            TransformPt[1] = (Coeffs[3] * OriX + Coeffs[4] * OriY + Coeffs[5]);
            return TransformPt;
        }

        public List<ImagePoints.Coordinate> Transform(List<ImagePoints.Coordinate> OriginalCoordinateList)
        {
            List<ImagePoints.Coordinate> TransformedCoordinateList = new List<ImagePoints.Coordinate>();

            foreach (ImagePoints.Coordinate OriginalCoordinate in OriginalCoordinateList)
            {
                float[] Pt = new float[2];
                Pt = Transform(OriginalCoordinate.X, OriginalCoordinate.Y);
                TransformedCoordinateList.Add(new ImagePoints.Coordinate(Pt[0], Pt[1], CoordinateFormat.mm));
            }

            return TransformedCoordinateList;
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

        public void Adjustment(List<ImagePoints.Coordinate> Tar, List<ImagePoints.Coordinate> Ref, LeastSquare LSA)
        {
            int Interation;
            float Difference, TempDifference;
            const int NumberOfEquation = 2;
            float A1 = 1f, A2 = 0, A3 = 0, B1 = 0, B2 = 1f, B3 = 0, C1 = 0, C2 = 0;
            Coeffs = new float[8];
            NumOfCoef = 8;
            if (Tar.Count < 8)
            {
                Convergent = false;
                return;
            }
            int Count = 0;
            do
            {
                Interation = 0;
                Difference = TempDifference = 1e-10f;

                do
                {
                    int Points = Tar.Count;
                    TempDifference = Difference;
                    Difference = 0;

                    Matrix<float> A = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 8, 1); //建立A矩陣，預設值為1                                                                                                  
                    Matrix<float> L = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 1, 1);

                    Parallel.For(0, Points, i =>
                    {
                        int Index_X = i * NumberOfEquation;
                        int Index_Y = Index_X + 1;
                        float S = (A1 * Tar[i].X + A2 * Tar[i].Y + A3);
                        float T = (B1 * Tar[i].X + B2 * Tar[i].Y + B3);
                        float U = (C1 * Tar[i].X + C2 * Tar[i].Y + 1);
                        float U2 = U * U, SU2 = S / U2, TU2 = T / U2;

                        A[Index_X, 0] = A[Index_Y, 3] = Tar[i].X / U;
                        A[Index_X, 1] = A[Index_Y, 4] = Tar[i].Y / U;
                        A[Index_X, 2] = A[Index_Y, 5] = 1 / U;
                        A[Index_X, 3] = A[Index_X, 4] = A[Index_X, 5] = A[Index_Y, 0] = A[1 + i * 2, 1] = A[1 + i * 2, 2] = 0;

                        A[Index_X, 6] = -Tar[i].X * SU2;
                        A[Index_X, 7] = -Tar[i].Y * SU2;
                        A[Index_Y, 6] = -Tar[i].X * TU2;
                        A[Index_Y, 7] = -Tar[i].Y * TU2;

                        L[Index_X, 0] = Ref[i].X - S / U;
                        L[Index_Y, 0] = Ref[i].Y - T / U;
                    });

                    LSA.Adjustment(A, L);

                    A1 = A1 + LSA.X[0, 0];
                    A2 = A2 + LSA.X[1, 0];
                    A3 = A3 + LSA.X[2, 0];
                    B1 = B1 + LSA.X[3, 0];
                    B2 = B2 + LSA.X[4, 0];
                    B3 = B3 + LSA.X[5, 0];
                    C1 = C1 + LSA.X[6, 0];
                    C2 = C2 + LSA.X[7, 0];

                    for (int i = 0; i < 8; i++)
                    {
                        Difference += LSA.X[i, 0] * LSA.X[i, 0];
                    }

                    Interation++;
                    if (Interation > 100)
                    {
                        Convergent = false;
                        return;
                    }

                } while (Math.Abs(Difference - TempDifference) / TempDifference > 0.9); ///TempDifference > 1e-2 );

                Count++;
            } while (BlunderRemoval.RemoveBlunder(ref Tar, ref Ref, LSA, BlunderOrder) == true);

            Convergent = (LSA.RMSE > 5) ? false : true;


            if (float.IsNaN(RMSE))
            {
                Convergent = false;
            }

            Coeffs[0] = A1;
            Coeffs[1] = A2;
            Coeffs[2] = A3;
            Coeffs[3] = B1;
            Coeffs[4] = B2;
            Coeffs[5] = B3;
            Coeffs[6] = C1;
            Coeffs[7] = C2;
            RMSE = LSA.RMSE;
            MatchPoints = Ref.Count;
        }

        public void Adjustment(List<ImagePoints.Coordinate> Tar, List<ImagePoints.Coordinate> Ref, LeastSquare LSA, float[] Coefs)
        {
            int Interation;
            float Difference, TempDifference;
            const int NumberOfEquation = 2;
            float A1 = Coefs[0], A2 = Coefs[1], A3 = Coefs[2], B1 = Coefs[3], B2 = Coefs[4], B3 = Coefs[5], C1 = Coefs[6], C2 = Coefs[7];
            Coeffs = new float[8];
            NumOfCoef = 8;
            if (Tar.Count < 8)
            {
                Convergent = false;
                return;
            }
            int Count = 0;
            do
            {
                Interation = 0;
                Difference = TempDifference = 1e-10f;

                do
                {
                    int Points = Tar.Count;
                    TempDifference = Difference;
                    Difference = 0;

                    Matrix<float> A = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 8, 1); //建立A矩陣，預設值為1                                                                                                  
                    Matrix<float> L = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 1, 1);

                    Parallel.For(0, Points, i =>
                    {
                        int Index_X = i * NumberOfEquation;
                        int Index_Y = Index_X + 1;
                        float S = (A1 * Tar[i].X + A2 * Tar[i].Y + A3);
                        float T = (B1 * Tar[i].X + B2 * Tar[i].Y + B3);
                        float U = (C1 * Tar[i].X + C2 * Tar[i].Y + 1);
                        float U2 = U * U, SU2 = S / U2, TU2 = T / U2;

                        A[Index_X, 0] = A[Index_Y, 3] = Tar[i].X / U;
                        A[Index_X, 1] = A[Index_Y, 4] = Tar[i].Y / U;
                        A[Index_X, 2] = A[Index_Y, 5] = 1 / U;
                        A[Index_X, 3] = A[Index_X, 4] = A[Index_X, 5] = A[Index_Y, 0] = A[1 + i * 2, 1] = A[1 + i * 2, 2] = 0;

                        A[Index_X, 6] = -Tar[i].X * SU2;
                        A[Index_X, 7] = -Tar[i].Y * SU2;
                        A[Index_Y, 6] = -Tar[i].X * TU2;
                        A[Index_Y, 7] = -Tar[i].Y * TU2;

                        L[Index_X, 0] = Ref[i].X - S / U;
                        L[Index_Y, 0] = Ref[i].Y - T / U;
                    });

                    LSA.Adjustment(A, L);

                    A1 = A1 + LSA.X[0, 0];
                    A2 = A2 + LSA.X[1, 0];
                    A3 = A3 + LSA.X[2, 0];
                    B1 = B1 + LSA.X[3, 0];
                    B2 = B2 + LSA.X[4, 0];
                    B3 = B3 + LSA.X[5, 0];
                    C1 = C1 + LSA.X[6, 0];
                    C2 = C2 + LSA.X[7, 0];

                    for (int i = 0; i < 8; i++)
                    {
                        Difference += LSA.X[i, 0] * LSA.X[i, 0];
                    }

                    Interation++;
                    if (Interation > 100)
                    {
                        Convergent = false;
                        return;
                    }

                } while (Math.Abs(Difference - TempDifference) / TempDifference > 0.9); ///TempDifference > 1e-2 );

                Count++;
            } while (BlunderRemoval.RemoveBlunder(ref Tar, ref Ref, LSA, BlunderOrder) == true);

            Convergent = (LSA.RMSE > 5) ? false : true;


            if (float.IsNaN(RMSE))
            {
                Convergent = false;
            }

            Coeffs[0] = A1;
            Coeffs[1] = A2;
            Coeffs[2] = A3;
            Coeffs[3] = B1;
            Coeffs[4] = B2;
            Coeffs[5] = B3;
            Coeffs[6] = C1;
            Coeffs[7] = C2;
            RMSE = LSA.RMSE;
            MatchPoints = Ref.Count;
        }


        public void Adjustment2(List<ImagePoints.Coordinate> Tar, List<ImagePoints.Coordinate> Ref, LeastSquare LSA)
        {
            const int NumberOfEquation = 2;
            Coeffs = new float[8];
            NumOfCoef = 8;
            if (Tar.Count < 8)
            {
                Convergent = false;
                return;
            }

            do
            {

                int Points = Tar.Count;

                Matrix<float> A = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 8, 1); //建立A矩陣，預設值為1                                                                                                  
                Matrix<float> L = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 1, 1);

                Parallel.For(0, Points, i =>
                {
                    int Index_X = i * NumberOfEquation;
                    int Index_Y = Index_X + 1;

                    A[Index_X, 0] = A[Index_Y, 3] = Tar[i].X;
                    A[Index_X, 1] = A[Index_Y, 4] = Tar[i].Y;
                    A[Index_X, 2] = A[Index_Y, 5] = 1;
                    A[Index_X, 3] = A[Index_X, 4] = A[Index_X, 5] = A[Index_Y, 0] = A[Index_Y, 1] = A[Index_Y, 2] = 0;

                    A[Index_X, 6] = -Tar[i].X * Ref[i].X;
                    A[Index_X, 7] = -Tar[i].Y * Ref[i].X;
                    A[Index_Y, 6] = -Tar[i].X * Ref[i].Y;
                    A[Index_Y, 7] = -Tar[i].Y * Ref[i].Y;

                    L[Index_X, 0] = Ref[i].X;
                    L[Index_Y, 0] = Ref[i].Y;
                });

                LSA.Adjustment(A, L);

            } while (BlunderRemoval.RemoveBlunder(ref Tar, ref Ref, LSA, BlunderOrder) == true);

            Convergent = (LSA.RMSE > 5) ? false : true;


            if (float.IsNaN(RMSE))
            {
                Convergent = false;
            }

            Coeffs[0] = LSA.X[0, 0];
            Coeffs[1] = LSA.X[1, 0];
            Coeffs[2] = LSA.X[2, 0];
            Coeffs[3] = LSA.X[3, 0];
            Coeffs[4] = LSA.X[4, 0];
            Coeffs[5] = LSA.X[5, 0];
            Coeffs[6] = LSA.X[6, 0];
            Coeffs[7] = LSA.X[7, 0];

            RMSE = LSA.RMSE;
            MatchPoints = Ref.Count;
        }

        public void AdjustmentExtended(List<ImagePoints.Coordinate> Tar, List<ImagePoints.Coordinate> Ref, LeastSquare LSA)
        {
            int Interation;
            float Difference, TempDifference;
            const int NumberOfEquation = 2;
            float A1 = 1f, A2 = 0, A3 = 0, B1 = 0, B2 = 1f, B3 = 0, C1 = 0, C2 = 0;
            Coeffs = new float[13];
            NumOfCoef = 13;
            if (Tar.Count < 13)
            {
                Convergent = false;
                return;
            }

            do
            {
                Interation = 0;
                Difference = TempDifference = 1e-10f;
                System.Diagnostics.Stopwatch Sw = new System.Diagnostics.Stopwatch();

                Sw.Start();
                do
                {
                    int Points = Tar.Count;
                    TempDifference = Difference;
                    Difference = 0;

                    Matrix<float> A = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 13, 1); //建立A矩陣，預設值為1                                                                                                  
                    Matrix<float> L = DenseMatrix.CreateDiagonal(Points * NumberOfEquation, 1, 1);

                    Parallel.For(0, Points, i =>
                    {
                        int Index_X = i * NumberOfEquation;
                        int Index_Y = Index_X + 1;
                        float S = (A1 * Tar[i].X + A2 * Tar[i].Y + A3);
                        float T = (B1 * Tar[i].X + B2 * Tar[i].Y + B3);
                        float U = (C1 * Tar[i].X + C2 * Tar[i].Y + 1);
                        float U2 = U * U, SU2 = S / U2, TU2 = T / U2;
                        float X2 = Tar[i].X * Tar[i].X, Y2 = Tar[i].Y * Tar[i].Y, XY = 2 * Tar[i].X * Tar[i].Y;
                        float r2 = X2 + Y2, r4 = r2 * r2, r6 = r2 * r4;

                        A[Index_X, 0] = A[Index_Y, 3] = Tar[i].X / U;
                        A[Index_X, 1] = A[Index_Y, 4] = Tar[i].Y / U;
                        A[Index_X, 2] = A[Index_Y, 5] = 1 / U;
                        A[Index_X, 3] = A[Index_X, 4] = A[Index_X, 5] = A[Index_Y, 0] = A[Index_Y, 1] = A[Index_Y, 2] = 0;

                        A[Index_X, 6] = -Tar[i].X * SU2;
                        A[Index_X, 7] = -Tar[i].Y * SU2;
                        A[Index_Y, 6] = -Tar[i].X * TU2;
                        A[Index_Y, 7] = -Tar[i].Y * TU2;

                        A[Index_X, 8] = Tar[i].X * r2;
                        A[Index_X, 9] = Tar[i].X * r4;
                        A[Index_X, 10] = Tar[i].X * r6;
                        A[Index_X, 11] = r2 + 2 * X2;
                        A[Index_X, 12] = XY;

                        A[Index_Y, 8] = Tar[i].Y * r2;
                        A[Index_Y, 9] = Tar[i].Y * r4;
                        A[Index_Y, 10] = Tar[i].Y * r6;
                        A[Index_Y, 11] = XY;
                        A[Index_Y, 12] = r2 + 2 * Y2;

                        L[Index_X, 0] = Ref[i].X - S / U;
                        L[Index_Y, 0] = Ref[i].Y - T / U;
                    });

                    LSA.Adjustment(A, L);

                    A1 = A1 + LSA.X[0, 0];
                    A2 = A2 + LSA.X[1, 0];
                    A3 = A3 + LSA.X[2, 0];
                    B1 = B1 + LSA.X[3, 0];
                    B2 = B2 + LSA.X[4, 0];
                    B3 = B3 + LSA.X[5, 0];
                    C1 = C1 + LSA.X[6, 0];
                    C2 = C2 + LSA.X[7, 0];

                    for (int i = 0; i < 8; i++)
                    {
                        Difference += LSA.X[i, 0] * LSA.X[i, 0];
                    }

                    Interation++;
                    if (Interation > 1000)
                    {
                        Convergent = false;
                        return;
                    }

                } while (Math.Abs(Difference - TempDifference) / TempDifference > 0.9);
            } while (BlunderRemoval.RemoveBlunder(ref Tar, ref Ref, LSA, BlunderOrder) == true);

            Convergent = (LSA.RMSE > 5) ? false : true;

            Coeffs[0] = A1;
            Coeffs[1] = A2;
            Coeffs[2] = A3;
            Coeffs[3] = B1;
            Coeffs[4] = B2;
            Coeffs[5] = B3;
            Coeffs[6] = C1;
            Coeffs[7] = C2;
            Coeffs[8] = LSA.X[8, 0];
            Coeffs[9] = LSA.X[9, 0];
            Coeffs[10] = LSA.X[10, 0];
            Coeffs[11] = LSA.X[11, 0];
            Coeffs[12] = LSA.X[12, 0];
            RMSE = LSA.RMSE;
            MatchPoints = Ref.Count;
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

        public List<ImagePoints.Coordinate> Transform(List<ImagePoints.Coordinate> OriginalCoordinateList)
        {
            List<ImagePoints.Coordinate> TransformedCoordinateList = new List<ImagePoints.Coordinate>();

            foreach (ImagePoints.Coordinate OriginalCoordinate in OriginalCoordinateList)
            {
                float X = 0, Y = 0;
                Transform(OriginalCoordinate.X, OriginalCoordinate.Y, ref X, ref Y);

                TransformedCoordinateList.Add(new ImagePoints.Coordinate(X, Y, CoordinateFormat.mm));
            }

            return TransformedCoordinateList;
        }



    }

    struct BlunderRemoval
    {
        public static bool RemoveBlunder(ref List<ImagePoints.Coordinate> Original, ref List<ImagePoints.Coordinate> Transform, LeastSquare LSA, float BlunderOrder)
        {
            int Points = Original.Count;
            int Size = 0;
            const int Equation = 2;
            bool IsBlunders = false;

            float Upbound = LSA.Mean + BlunderOrder * LSA.RMSE;
            float Lowbound = LSA.Mean - BlunderOrder * LSA.RMSE;
            Size = Points * Equation - 1;
            int RemoveCounts = 0;

            for (int i = Size; i >= 0; i = i - Equation)
            {
                if (LSA.V[i, 0] > Upbound || LSA.V[i, 0] < Lowbound || LSA.V[i - 1, 0] > Upbound || LSA.V[i - 1, 0] < Lowbound)
                {
                    Original.RemoveAt(i / Equation);
                    Transform.RemoveAt(i / Equation);
                    IsBlunders = true;
                    RemoveCounts++;
                }
            }
            return IsBlunders;
        }

    }



}
    
