using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace ImageProcessing
{
   
        public class LeastSquare
        {
            public Matrix<float> X;
            Matrix<float> N;
            public Matrix<float> V;
            public float Mean, RMSE;
            public void Adjustment(Matrix<float> A, Matrix<float> L)
            {
                N = A.Transpose().Multiply(A);
                X = N.Inverse().Multiply(A.Transpose()).Multiply(L);
                V = A.Multiply(X).Subtract(L);
                RMSE = 0;
                for (int i = 0; i < V.RowCount; i++)
                {
                    RMSE += V[i, 0] * V[i, 0];
                }

                RMSE = (float)Math.Sqrt(RMSE / (V.RowCount-X.RowCount));
            //    BasicStatstic(V);
            }
            public void Adjustment(Matrix<float> A, Matrix<float> P, Matrix<float> L)
            {
                Matrix<float> N = A.Transpose().Multiply(P).Multiply(A);
                X = N.Inverse().Multiply(A.Transpose()).Multiply(P).Multiply(L);
                V = A.Multiply(X).Subtract(L);

                for (int i = 0; i < V.RowCount; i++)
                {
                    RMSE += V[i, 0] * V[i, 0];
                }
                RMSE = (float)Math.Sqrt(RMSE / (V.RowCount - X.RowCount));
              //  BasicStatstic(V);
            }
            private void BasicStatstic(Matrix<float> Component)
            {
                for (int i = 0; i < Component.RowCount; i++)
                {
                    Mean += Component[i, 0];
                }
                Mean = Mean / Component.RowCount;

              

            }

        }
    }
