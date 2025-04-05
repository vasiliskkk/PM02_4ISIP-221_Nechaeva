using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM02_4ISIP_221_Nechaeva
{
    public class SimplexProblem
    {
        public double[] ObjectiveFunction { get; set; } // Целевая функция (коэффициенты)
        public double[,] Constraints { get; set; }     // Матрица ограничений
        public double[] RightHandSide { get; set; }    // Правые части ограничений
        public bool Maximize { get; set; } = true;     // Максимизация (true) или минимизация (false)
        public string[] Variables { get; set; }        // Имена переменных
        public double[] Solution { get; set; }         // Решение
        public double OptimalValue { get; set; }       // Оптимальное значение целевой функции
        public bool IsSolved { get; set; }             // Флаг решения
        public string Message { get; set; }            // Сообщение о результате
    }

    public class SimplexSolver
    {
        private const double Epsilon = 1e-10;

        public SimplexProblem Solve(SimplexProblem problem)
        {
            try
            {
                // Приведение задачи минимизации к задаче максимизации
                if (!problem.Maximize)
                {
                    problem.ObjectiveFunction = problem.ObjectiveFunction.Select(x => -x).ToArray();
                }

                int rows = problem.Constraints.GetLength(0);
                int cols = problem.Constraints.GetLength(1);

                // Добавляем slack variables
                int totalVars = cols + rows;
                double[,] tableau = new double[rows + 1, totalVars + 1];

                // Заполняем целевую функцию
                for (int j = 0; j < cols; j++)
                {
                    tableau[0, j] = -problem.ObjectiveFunction[j];
                }

                // Заполняем ограничения
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        tableau[i + 1, j] = problem.Constraints[i, j];
                    }

                    // Добавляем slack variable
                    tableau[i + 1, cols + i] = 1;

                    // Правая часть
                    tableau[i + 1, totalVars] = problem.RightHandSide[i];
                }

                // Основная фаза симплекс-метода
                while (true)
                {
                    // Находим входящую переменную (наибольший по модулю отрицательный коэффициент)
                    int pivotCol = -1;
                    double min = 0;
                    for (int j = 0; j < totalVars; j++)
                    {
                        if (tableau[0, j] < min - Epsilon)
                        {
                            min = tableau[0, j];
                            pivotCol = j;
                        }
                    }

                    // Если все коэффициенты неотрицательные - решение найдено
                    if (pivotCol == -1) break;

                    // Находим исходящую переменную (минимальное отношение)
                    int pivotRow = -1;
                    double minRatio = double.MaxValue;
                    for (int i = 1; i <= rows; i++)
                    {
                        if (tableau[i, pivotCol] > Epsilon)
                        {
                            double ratio = tableau[i, totalVars] / tableau[i, pivotCol];
                            if (ratio < minRatio - Epsilon)
                            {
                                minRatio = ratio;
                                pivotRow = i;
                            }
                        }
                    }

                    // Если не нашли pivotRow - задача неограничена
                    if (pivotRow == -1)
                    {
                        problem.IsSolved = false;
                        problem.Message = "Целевая функция неограничена";
                        return problem;
                    }

                    // Выполняем pivot операцию
                    double pivotValue = tableau[pivotRow, pivotCol];
                    for (int j = 0; j <= totalVars; j++)
                    {
                        tableau[pivotRow, j] /= pivotValue;
                    }

                    for (int i = 0; i <= rows; i++)
                    {
                        if (i != pivotRow)
                        {
                            double factor = tableau[i, pivotCol];
                            for (int j = 0; j <= totalVars; j++)
                            {
                                tableau[i, j] -= factor * tableau[pivotRow, j];
                            }
                        }
                    }
                }

                // Извлекаем решение
                double[] solution = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    int count = 0;
                    int row = -1;
                    for (int i = 1; i <= rows; i++)
                    {
                        if (Math.Abs(tableau[i, j] - 1) < Epsilon)
                        {
                            count++;
                            row = i;
                        }
                        else if (Math.Abs(tableau[i, j]) > Epsilon)
                        {
                            count = 2;
                        }
                    }

                    if (count == 1)
                    {
                        solution[j] = tableau[row, totalVars];
                    }
                }

                problem.Solution = solution;
                problem.OptimalValue = tableau[0, totalVars];
                if (!problem.Maximize) problem.OptimalValue = -problem.OptimalValue;
                problem.IsSolved = true;
                problem.Message = "Решение найдено";
            }
            catch (Exception ex)
            {
                problem.IsSolved = false;
                problem.Message = $"Ошибка: {ex.Message}";
            }

            return problem;
        }
    }
}