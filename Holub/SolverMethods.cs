using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLARSolver
{
    /// <summary>
    /// Provides methods for solving systems of linear algebraic equations using various iterative methods.
    /// Includes Jacobi method, Gauss-Seidel method, and Gradient Descent method.
    /// </summary>
    public class SolverMethods
    {
        /// <summary>
        /// Class for storing iteration results during the solving process
        /// </summary>
        public class IterationResult
        {
            /// <summary>
            /// Iteration number (0-based)
            /// </summary>
            public int IterationNumber { get; set; }

            /// <summary>
            /// Current solution vector
            /// </summary>
            public double[] Solution { get; set; } = Array.Empty<double>();

            /// <summary>
            /// Error estimate for the current iteration
            /// </summary>
            public double Error { get; set; }

            /// <summary>
            /// Computation time in milliseconds
            /// </summary>
            public double ComputationTime { get; set; }
        }

        /// <summary>
        /// Implements the Jacobi iterative method (simple iteration method)
        /// </summary>
        /// <param name="A">Coefficient matrix</param>
        /// <param name="b">Right-hand side vector</param>
        /// <param name="eps">Convergence tolerance (default: 1e-6)</param>
        /// <param name="maxIterations">Maximum number of iterations (default: 1000)</param>
        /// <returns>List of iteration results</returns>
        /// <exception cref="InvalidOperationException">Thrown when method does not converge</exception>
        public static List<IterationResult> JacobiMethod(double[,] A, double[] b, double eps = 1e-6, int maxIterations = 1000)
        {
            // Validate matrix for critical numerical issues
            if (!ValidateMatrixValues(A, b))
            {
                throw new InvalidOperationException("Matrix contains invalid values (NaN, Infinity) or zero diagonal elements");
            }

            int n = b.Length;
            double[] x = new double[n]; // Initial approximation (zeros)
            double[] xNew = new double[n];
            List<IterationResult> iterations = new List<IterationResult>();
            double error;
            int iteration = 0;

            // Check convergence criteria
            if (!CheckConvergence(A, "Jacobi"))
            {
                throw new InvalidOperationException("Jacobi method may not converge for the given matrix (not diagonally dominant)");
            }

            DateTime startTime = DateTime.Now;

            do
            {
                // Calculate new approximation
                for (int i = 0; i < n; i++)
                {
                    double sum = b[i];
                    for (int j = 0; j < n; j++)
                    {
                        if (i != j)
                            sum -= A[i, j] * x[j];
                    }

                    // The diagonal elements were checked in ValidateMatrixValues
                    xNew[i] = sum / A[i, i];

                    // Check for overflow or NaN during calculation
                    if (double.IsNaN(xNew[i]) || double.IsInfinity(xNew[i]))
                    {
                        throw new InvalidOperationException($"Numerical overflow detected in solution x[{i + 1}]");
                    }
                }

                // Calculate error
                error = CalculateError(x, xNew);

                // Save iteration results with time in milliseconds
                TimeSpan currentElapsed = DateTime.Now - startTime;
                iterations.Add(new IterationResult
                {
                    IterationNumber = iteration,
                    Solution = (double[])xNew.Clone(),
                    Error = error,
                    ComputationTime = currentElapsed.TotalMilliseconds
                });

                // Copy new approximation for next iteration
                Array.Copy(xNew, x, n);
                iteration++;

            } while (error > eps && iteration < maxIterations);

            // Check if we reached maximum iterations without converging
            if (iteration >= maxIterations && error > eps)
            {
                throw new InvalidOperationException($"Failed to converge after {maxIterations} iterations. Final error: {error:E10}");
            }

            return iterations;
        }

        /// <summary>
        /// Implements the Gauss-Seidel iterative method
        /// </summary>
        /// <param name="A">Coefficient matrix</param>
        /// <param name="b">Right-hand side vector</param>
        /// <param name="eps">Convergence tolerance (default: 1e-6)</param>
        /// <param name="maxIterations">Maximum number of iterations (default: 1000)</param>
        /// <returns>List of iteration results</returns>
        /// <exception cref="InvalidOperationException">Thrown when method does not converge</exception>
        public static List<IterationResult> GaussSeidelMethod(double[,] A, double[] b, double eps = 1e-6, int maxIterations = 1000)
        {
            // Validate matrix for critical numerical issues
            if (!ValidateMatrixValues(A, b))
            {
                throw new InvalidOperationException("Matrix contains invalid values (NaN, Infinity) or zero diagonal elements");
            }

            int n = b.Length;
            double[] x = new double[n]; // Initial approximation (zeros)
            double[] xNew = new double[n];
            List<IterationResult> iterations = new List<IterationResult>();
            double error;
            int iteration = 0;

            // Check convergence criteria
            if (!CheckConvergence(A, "GaussSeidel"))
            {
                throw new InvalidOperationException("Gauss-Seidel method may not converge for the given matrix (not diagonally dominant)");
            }

            DateTime startTime = DateTime.Now;

            do
            {
                // Copy current approximation
                Array.Copy(x, xNew, n);

                // Calculate new approximation (using already updated values)
                for (int i = 0; i < n; i++)
                {
                    double sum = b[i];
                    for (int j = 0; j < n; j++)
                    {
                        if (i != j)
                        {
                            if (j < i) // Use already calculated values
                                sum -= A[i, j] * xNew[j];
                            else // Use values from previous iteration
                                sum -= A[i, j] * x[j];
                        }
                    }

                    // The diagonal elements were checked in ValidateMatrixValues
                    xNew[i] = sum / A[i, i];

                    // Check for overflow or NaN during calculation
                    if (double.IsNaN(xNew[i]) || double.IsInfinity(xNew[i]))
                    {
                        throw new InvalidOperationException($"Numerical overflow detected in solution x[{i + 1}]");
                    }
                }

                // Calculate error
                error = CalculateError(x, xNew);

                // Save iteration results with time in milliseconds
                TimeSpan currentElapsed = DateTime.Now - startTime;
                iterations.Add(new IterationResult
                {
                    IterationNumber = iteration,
                    Solution = (double[])xNew.Clone(),
                    Error = error,
                    ComputationTime = currentElapsed.TotalMilliseconds
                });

                // Copy new approximation for next iteration
                Array.Copy(xNew, x, n);
                iteration++;

            } while (error > eps && iteration < maxIterations);

            // Check if we reached maximum iterations without converging
            if (iteration >= maxIterations && error > eps)
            {
                throw new InvalidOperationException($"Failed to converge after {maxIterations} iterations. Final error: {error:E10}");
            }

            return iterations;
        }

        /// <summary>
        /// Implements the Gradient Descent method (steepest descent)
        /// </summary>
        /// <param name="A">Coefficient matrix (must be symmetric positive definite)</param>
        /// <param name="b">Right-hand side vector</param>
        /// <param name="eps">Convergence tolerance (default: 1e-6)</param>
        /// <param name="maxIterations">Maximum number of iterations (default: 1000)</param>
        /// <returns>List of iteration results</returns>
        /// <exception cref="InvalidOperationException">Thrown when matrix is not symmetric positive definite</exception>
        public static List<IterationResult> GradientDescentMethod(double[,] A, double[] b, double eps = 1e-6, int maxIterations = 1000)
        {
            // Validate matrix for critical numerical issues
            if (!ValidateMatrixValues(A, b))
            {
                throw new InvalidOperationException("Matrix contains invalid values (NaN, Infinity) or zero diagonal elements");
            }

            int n = b.Length;
            double[] x = new double[n]; // Initial approximation (zeros)
            List<IterationResult> iterations = new List<IterationResult>();
            double error = 0; // Initialize error variable
            int iteration = 0;

            // Check that the matrix is symmetric and positive definite
            if (!IsSymmetricPositiveDefinite(A))
            {
                throw new InvalidOperationException("Gradient descent method is only applicable to symmetric positive definite matrices!");
            }

            DateTime startTime = DateTime.Now;

            do
            {
                // Calculate residual vector r = b - A*x
                double[] r = new double[n];
                for (int i = 0; i < n; i++)
                {
                    r[i] = b[i];
                    for (int j = 0; j < n; j++)
                    {
                        r[i] -= A[i, j] * x[j];
                    }
                }

                // Calculate scalar product (r, r)
                double rr = 0;
                for (int i = 0; i < n; i++)
                {
                    rr += r[i] * r[i];
                }

                // Check if residual is close to zero (convergence achieved)
                if (Math.Abs(rr) < 1e-14)
                {
                    // Already converged, save final iteration and break
                    TimeSpan elapsed = DateTime.Now - startTime;
                    iterations.Add(new IterationResult
                    {
                        IterationNumber = iteration,
                        Solution = (double[])x.Clone(),
                        Error = 0,
                        ComputationTime = elapsed.TotalMilliseconds
                    });
                    break;
                }

                // Calculate scalar product (A*r, r)
                double[] Ar = new double[n];
                for (int i = 0; i < n; i++)
                {
                    Ar[i] = 0;
                    for (int j = 0; j < n; j++)
                    {
                        Ar[i] += A[i, j] * r[j];
                    }
                }

                double rAr = 0;
                for (int i = 0; i < n; i++)
                {
                    rAr += r[i] * Ar[i];
                }

                // Check for division by very small value
                if (Math.Abs(rAr) < 1e-14)
                {
                    throw new InvalidOperationException("Division by very small value in Gradient Descent method");
                }

                // Calculate step size alpha
                double alpha = rr / rAr;

                // Update solution
                double[] xNew = new double[n];
                for (int i = 0; i < n; i++)
                {
                    xNew[i] = x[i] + alpha * r[i];

                    // Check for overflow or NaN
                    if (double.IsNaN(xNew[i]) || double.IsInfinity(xNew[i]))
                    {
                        throw new InvalidOperationException($"Numerical overflow detected in solution x[{i + 1}]");
                    }
                }

                // Calculate error
                error = CalculateError(x, xNew);

                // Save iteration results
                TimeSpan currentElapsed = DateTime.Now - startTime;
                iterations.Add(new IterationResult
                {
                    IterationNumber = iteration,
                    Solution = (double[])xNew.Clone(),
                    Error = error,
                    ComputationTime = currentElapsed.TotalMilliseconds
                });

                // Copy new approximation for next iteration
                Array.Copy(xNew, x, n);
                iteration++;

            } while (error > eps && iteration < maxIterations);

            // Check if we reached maximum iterations without converging
            if (iteration >= maxIterations && error > eps)
            {
                throw new InvalidOperationException($"Failed to converge after {maxIterations} iterations. Final error: {error:E10}");
            }

            return iterations;
        }

        /// <summary>
        /// Checks for critical numerical issues that would prevent calculation
        /// </summary>
        /// <param name="A">Coefficient matrix</param>
        /// <param name="b">Right-hand side vector</param>
        /// <returns>True if matrix is suitable for calculation, false otherwise</returns>
        private static bool ValidateMatrixValues(double[,] A, double[] b)
        {
            int n = b.Length;

            // Check for NaN or Infinity
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (double.IsNaN(A[i, j]) || double.IsInfinity(A[i, j]))
                        return false;
                }

                if (double.IsNaN(b[i]) || double.IsInfinity(b[i]))
                    return false;
            }

            // Check for zero diagonal elements (which would cause division by zero)
            for (int i = 0; i < n; i++)
            {
                if (Math.Abs(A[i, i]) < 1e-14) // Very close to zero
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks convergence conditions for the specified method
        /// </summary>
        /// <param name="A">Coefficient matrix</param>
        /// <param name="method">Method name ("Jacobi" or "GaussSeidel")</param>
        /// <returns>True if convergence conditions are met, otherwise false</returns>
        private static bool CheckConvergence(double[,] A, string method)
        {
            int n = A.GetLength(0);

            switch (method)
            {
                case "Jacobi":
                case "GaussSeidel":
                    // Diagonal dominance - sufficient condition for convergence
                    for (int i = 0; i < n; i++)
                    {
                        double sum = 0;
                        for (int j = 0; j < n; j++)
                        {
                            if (i != j)
                                sum += Math.Abs(A[i, j]);
                        }
                        if (Math.Abs(A[i, i]) <= sum) // No diagonal dominance
                            return false;
                    }
                    return true;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Checks if a matrix is symmetric and positive definite
        /// </summary>
        /// <param name="A">Matrix to check</param>
        /// <returns>True if matrix is symmetric positive definite, otherwise false</returns>
        private static bool IsSymmetricPositiveDefinite(double[,] A)
        {
            int n = A.GetLength(0);

            // Check symmetry
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (Math.Abs(A[i, j] - A[j, i]) > 1e-10)
                        return false;
                }
            }

            // Check positive definiteness using leading principal minors
            for (int k = 1; k <= n; k++)
            {
                double[,] subMatrix = new double[k, k];
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < k; j++)
                    {
                        subMatrix[i, j] = A[i, j];
                    }
                }

                if (DeterminantOfMatrix(subMatrix) <= 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the coefficient matrix might have numerical stability issues for the specified method
        /// </summary>
        /// <param name="A">Coefficient matrix to check</param>
        /// <param name="methodName">Name of the solution method</param>
        /// <returns>Warning message if potential issues detected, empty string otherwise</returns>
        public static string CheckNumericalStability(double[,] A, string methodName)
        {
            int n = A.GetLength(0);

            // Calculate condition number approximation (using max row sum norm)
            double maxRowSum = 0;
            double minDiagonalElement = double.MaxValue;

            for (int i = 0; i < n; i++)
            {
                double rowSum = 0;
                for (int j = 0; j < n; j++)
                {
                    rowSum += Math.Abs(A[i, j]);
                }

                maxRowSum = Math.Max(maxRowSum, rowSum);

                if (Math.Abs(A[i, i]) < minDiagonalElement)
                {
                    minDiagonalElement = Math.Abs(A[i, i]);
                }
            }

            // For Jacobi and Gauss-Seidel, check diagonal dominance
            bool isDiagonallyDominant = true;
            for (int i = 0; i < n; i++)
            {
                double diagonalElement = Math.Abs(A[i, i]);
                double sumOfOthers = 0;

                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                        sumOfOthers += Math.Abs(A[i, j]);
                }

                if (diagonalElement <= sumOfOthers)
                {
                    isDiagonallyDominant = false;
                    break;
                }
            }

            // Check for symmetry (only for Gradient Descent)
            bool isSymmetric = true;
            if (methodName == "Gradient")
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (Math.Abs(A[i, j] - A[j, i]) > 1e-10)
                        {
                            isSymmetric = false;
                            break;
                        }
                    }
                    if (!isSymmetric) break;
                }
            }

            // Build warning message if needed
            System.Text.StringBuilder warning = new System.Text.StringBuilder();

            if (minDiagonalElement < 1e-6)
            {
                warning.AppendLine("Warning: Small diagonal elements detected. This may lead to numerical instability.");
            }

            if ((methodName == "Jacobi" || methodName == "GaussSeidel") && !isDiagonallyDominant)
            {
                warning.AppendLine($"Warning: Matrix is not diagonally dominant. {(methodName == "Jacobi" ? "Jacobi" : "Gauss-Seidel")} method may not converge.");
            }

            if (methodName == "Gradient" && !isSymmetric)
            {
                warning.AppendLine("Warning: Matrix is not symmetric. Gradient Descent method requires a symmetric matrix.");
            }

            // Check if there's a large difference in magnitude between elements
            double maxElement = 0;
            double minElement = double.MaxValue;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double absValue = Math.Abs(A[i, j]);
                    if (absValue > 0)  // Only consider non-zero elements
                    {
                        maxElement = Math.Max(maxElement, absValue);
                        minElement = Math.Min(minElement, absValue);
                    }
                }
            }

            if (minElement > 0 && maxElement / minElement > 1e5)
            {
                warning.AppendLine($"Warning: Large difference in magnitude between matrix elements detected (ratio: {maxElement / minElement:E2}). This may cause numerical instability.");
            }

            return warning.ToString();
        }

        /// <summary>
        /// Calculates the determinant of a matrix using recursive approach
        /// </summary>
        /// <param name="matrix">Input matrix</param>
        /// <returns>Determinant value</returns>
        private static double DeterminantOfMatrix(double[,] matrix)
        {
            int n = matrix.GetLength(0);

            if (n == 1)
                return matrix[0, 0];

            if (n == 2)
                return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

            double determinant = 0;
            for (int j = 0; j < n; j++)
            {
                determinant += Math.Pow(-1, j) * matrix[0, j] * DeterminantOfMatrix(Minor(matrix, 0, j));
            }

            return determinant;
        }

        /// <summary>
        /// Extracts a minor of a matrix by removing specified row and column
        /// </summary>
        /// <param name="matrix">Original matrix</param>
        /// <param name="row">Row to remove</param>
        /// <param name="col">Column to remove</param>
        /// <returns>Minor matrix</returns>
        private static double[,] Minor(double[,] matrix, int row, int col)
        {
            int n = matrix.GetLength(0);
            double[,] minor = new double[n - 1, n - 1];

            int minorRow = 0;
            for (int i = 0; i < n; i++)
            {
                if (i == row) continue;

                int minorCol = 0;
                for (int j = 0; j < n; j++)
                {
                    if (j == col) continue;

                    minor[minorRow, minorCol] = matrix[i, j];
                    minorCol++;
                }
                minorRow++;
            }

            return minor;
        }

        /// <summary>
        /// Calculates the error between two approximations (maximum absolute difference)
        /// </summary>
        /// <param name="x1">First approximation vector</param>
        /// <param name="x2">Second approximation vector</param>
        /// <returns>Maximum absolute difference between vector elements</returns>
        private static double CalculateError(double[] x1, double[] x2)
        {
            double maxDiff = 0;
            for (int i = 0; i < x1.Length; i++)
            {
                double diff = Math.Abs(x2[i] - x1[i]);
                if (diff > maxDiff)
                    maxDiff = diff;
            }
            return maxDiff;
        }

        /// <summary>
        /// Calculates the residual vector r = b - A*x
        /// </summary>
        /// <param name="A">Coefficient matrix</param>
        /// <param name="x">Solution vector</param>
        /// <param name="b">Right-hand side vector</param>
        /// <returns>Residual vector</returns>
        public static double[] CalculateResidual(double[,] A, double[] x, double[] b)
        {
            int n = b.Length;
            double[] r = new double[n];

            for (int i = 0; i < n; i++)
            {
                r[i] = b[i];
                for (int j = 0; j < n; j++)
                {
                    r[i] -= A[i, j] * x[j];
                }
            }

            return r;
        }

        /// <summary>
        /// Calculates the Euclidean norm of a vector
        /// </summary>
        /// <param name="vector">Input vector</param>
        /// <returns>Euclidean norm (square root of sum of squares)</returns>
        public static double CalculateNorm(double[] vector)
        {
            double sum = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                sum += vector[i] * vector[i];
            }
            return Math.Sqrt(sum);
        }
    }
}