using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PM02_4ISIP_221_Nechaeva
{
    public partial class MainWindow : Window
    {
        public class CoefficientItem
        {
            public string Variable { get; set; }
            public double Coefficient { get; set; }
        }

        public class ConstraintItem
        {
            public List<CoefficientItem> Coefficients { get; set; } = new List<CoefficientItem>();
            public int Inequality { get; set; } // 0: ≤, 1: =, 2: ≥
            public double RightHandSide { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            CreateProblem(3, 2);
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(tbVariables.Text, out int variables) &&
                int.TryParse(tbConstraints.Text, out int constraints))
            {
                if (variables > 0 && constraints > 0)
                {
                    CreateProblem(variables, constraints);
                }
                else
                {
                    MessageBox.Show("Количество переменных и ограничений должно быть положительным числом.");
                }
            }
            else
            {
                MessageBox.Show("Введите корректные числа для количества переменных и ограничений.");
            }
        }

        private void CreateProblem(int variables, int constraints)
        {
            // Создаем целевую функцию
            var objectiveItems = new List<CoefficientItem>();
            for (int j = 0; j < variables; j++)
            {
                objectiveItems.Add(new CoefficientItem
                {
                    Variable = $"x{j + 1}",
                    Coefficient = 0
                });
            }
            objectiveFunctionItems.ItemsSource = objectiveItems;

            // Создаем ограничения
            var constraintItems = new List<ConstraintItem>();
            for (int i = 0; i < constraints; i++)
            {
                var constraint = new ConstraintItem();
                for (int j = 0; j < variables; j++)
                {
                    constraint.Coefficients.Add(new CoefficientItem
                    {
                        Variable = $"x{j + 1}",
                        Coefficient = 0
                    });
                }
                constraintItems.Add(constraint);
            }
            constraintsItems.ItemsSource = constraintItems; // Исправлено имя
        }

        private void BtnSolve_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем данные из интерфейса
                bool maximize = rbMaximize.IsChecked == true;
                var objectiveItems = objectiveFunctionItems.ItemsSource.Cast<CoefficientItem>().ToList();
                var constraintItems = constraintsItems.ItemsSource.Cast<ConstraintItem>().ToList(); // Исправлено имя

                int variables = objectiveItems.Count;
                int constraints = constraintItems.Count;

                // Формируем задачу для симплекс-метода
                var problem = new SimplexProblem
                {
                    Maximize = maximize,
                    ObjectiveFunction = objectiveItems.Select(x => x.Coefficient).ToArray(),
                    Variables = objectiveItems.Select(x => x.Variable).ToArray(),
                    Constraints = new double[constraints, variables],
                    RightHandSide = new double[constraints]
                };

                // Заполняем ограничения
                for (int i = 0; i < constraints; i++)
                {
                    var constraint = constraintItems[i];
                    for (int j = 0; j < variables; j++)
                    {
                        problem.Constraints[i, j] = constraint.Coefficients[j].Coefficient;
                    }
                    problem.RightHandSide[i] = constraint.RightHandSide;

                    // Приводим ограничения типа ≥ к ≤
                    if (constraint.Inequality == 2) // ≥
                    {
                        for (int j = 0; j < variables; j++)
                        {
                            problem.Constraints[i, j] = -problem.Constraints[i, j];
                        }
                        problem.RightHandSide[i] = -problem.RightHandSide[i];
                    }
                }

                // Решаем задачу
                var solver = new SimplexSolver();
                var result = solver.Solve(problem);

                // Отображаем результаты
                DisplayResults(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayResults(SimplexProblem result)
        {
            string solutionText = "";
            string detailsText = "";

            if (result.IsSolved)
            {
                solutionText += $"Оптимальное значение: {result.OptimalValue:F4}\n\n";
                solutionText += "Оптимальное решение:\n";

                for (int j = 0; j < result.Solution.Length; j++)
                {
                    solutionText += $"{result.Variables[j]} = {result.Solution[j]:F4}\n";
                }

                detailsText += $"Сообщение: {result.Message}\n";
                detailsText += $"Тип задачи: {(result.Maximize ? "Максимизация" : "Минимизация")}\n\n";

                detailsText += "Целевая функция:\n";
                for (int j = 0; j < result.ObjectiveFunction.Length; j++)
                {
                    detailsText += $"{result.ObjectiveFunction[j]:F2}*{result.Variables[j]} ";
                    if (j < result.ObjectiveFunction.Length - 1)
                        detailsText += "+ ";
                }
                detailsText += $"→ {(result.Maximize ? "max" : "min")}\n\n";

                detailsText += "Ограничения:\n";
                for (int i = 0; i < result.Constraints.GetLength(0); i++)
                {
                    for (int j = 0; j < result.Constraints.GetLength(1); j++)
                    {
                        detailsText += $"{result.Constraints[i, j]:F2}*{result.Variables[j]} ";
                        if (j < result.Constraints.GetLength(1) - 1)
                            detailsText += "+ ";
                    }
                    detailsText += $"≤ {result.RightHandSide[i]:F2}\n";
                }
            }
            else
            {
                solutionText = $"Решение не найдено: {result.Message}"; // Исправлено "найдено"
                detailsText = solutionText;
            }

            txtSolution.Text = solutionText;
            txtDetails.Text = detailsText;
        }
    }
}