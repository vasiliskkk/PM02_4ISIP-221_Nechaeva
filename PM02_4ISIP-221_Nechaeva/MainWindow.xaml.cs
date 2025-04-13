using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        // Переименованный класс для задачи линейного программирования
        public class LinearProgrammingProblem
        {
            public bool IsMaximization { get; set; }
            public double[] ObjectiveCoefficients { get; set; }
            public string[] VariableNames { get; set; }
            public double[,] ConstraintCoefficients { get; set; }
            public double[] ConstraintValues { get; set; }
            public bool HasSolution { get; set; }
            public double OptimalSolutionValue { get; set; }
            public double[] VariableValues { get; set; }
            public string SolutionStatus { get; set; }
        }

        // Переименованный класс для решения задачи
        public class LinearProgrammingSolver
        {
            public LinearProgrammingProblem FindSolution(LinearProgrammingProblem problem)
            {
                // Здесь должна быть реализация симплекс-метода
                // Это заглушка для демонстрации

                var result = new LinearProgrammingProblem
                {
                    IsMaximization = problem.IsMaximization,
                    ObjectiveCoefficients = problem.ObjectiveCoefficients,
                    VariableNames = problem.VariableNames,
                    ConstraintCoefficients = problem.ConstraintCoefficients,
                    ConstraintValues = problem.ConstraintValues,
                    HasSolution = true,
                    OptimalSolutionValue = problem.IsMaximization ? 100.0 : 50.0,
                    VariableValues = new double[problem.VariableNames.Length],
                    SolutionStatus = "Решение найдено успешно"
                };

                // Заполняем "решение" случайными значениями для демонстрации
                Random rnd = new Random();
                for (int i = 0; i < result.VariableValues.Length; i++)
                {
                    result.VariableValues[i] = Math.Round(rnd.NextDouble() * 10, 2);
                }

                return result;
            }
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
            constraintsItems.ItemsSource = constraintItems;
        }

        private void BtnSolve_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
            {
                MessageBox.Show("Обнаружены ошибки ввода. Проверьте вкладку 'Валидация'",
                              "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Получаем данные из интерфейса
                bool maximize = rbMaximize.IsChecked == true;
                var objectiveItems = (List<CoefficientItem>)objectiveFunctionItems.ItemsSource;
                var constraintItems = (List<ConstraintItem>)constraintsItems.ItemsSource;

                int variables = objectiveItems.Count;
                int constraints = constraintItems.Count;

                // Формируем задачу для симплекс-метода
                var problem = new LinearProgrammingProblem
                {
                    IsMaximization = maximize,
                    ObjectiveCoefficients = objectiveItems.Select(x => x.Coefficient).ToArray(),
                    VariableNames = objectiveItems.Select(x => x.Variable).ToArray(),
                    ConstraintCoefficients = new double[constraints, variables],
                    ConstraintValues = new double[constraints]
                };

                // Заполняем ограничения
                for (int i = 0; i < constraints; i++)
                {
                    var constraint = constraintItems[i];
                    for (int j = 0; j < variables; j++)
                    {
                        problem.ConstraintCoefficients[i, j] = constraint.Coefficients[j].Coefficient;
                    }
                    problem.ConstraintValues[i] = constraint.RightHandSide;

                    // Приводим ограничения типа ≥ к ≤
                    if (constraint.Inequality == 2) // ≥
                    {
                        for (int j = 0; j < variables; j++)
                        {
                            problem.ConstraintCoefficients[i, j] = -problem.ConstraintCoefficients[i, j];
                        }
                        problem.ConstraintValues[i] = -problem.ConstraintValues[i];
                    }
                }

                // Решаем задачу
                var solver = new LinearProgrammingSolver();
                var result = solver.FindSolution(problem);

                // Отображаем результаты
                DisplayResults(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayResults(LinearProgrammingProblem result)
        {
            string solutionText = "";
            string detailsText = "";

            if (result.HasSolution)
            {
                solutionText += $"Оптимальное значение: {result.OptimalSolutionValue:F4}\n\n";
                solutionText += "Оптимальное решение:\n";

                for (int j = 0; j < result.VariableValues.Length; j++)
                {
                    solutionText += $"{result.VariableNames[j]} = {result.VariableValues[j]:F4}\n";
                }

                detailsText += $"Сообщение: {result.SolutionStatus}\n";
                detailsText += $"Тип задачи: {(result.IsMaximization ? "Максимизация" : "Минимизация")}\n\n";

                detailsText += "Целевая функция:\n";
                for (int j = 0; j < result.ObjectiveCoefficients.Length; j++)
                {
                    detailsText += $"{result.ObjectiveCoefficients[j]:F2}*{result.VariableNames[j]} ";
                    if (j < result.ObjectiveCoefficients.Length - 1)
                        detailsText += "+ ";
                }
                detailsText += $"→ {(result.IsMaximization ? "max" : "min")}\n\n";

                detailsText += "Ограничения:\n";
                for (int i = 0; i < result.ConstraintCoefficients.GetLength(0); i++)
                {
                    for (int j = 0; j < result.ConstraintCoefficients.GetLength(1); j++)
                    {
                        detailsText += $"{result.ConstraintCoefficients[i, j]:F2}*{result.VariableNames[j]} ";
                        if (j < result.ConstraintCoefficients.GetLength(1) - 1)
                            detailsText += "+ ";
                    }
                    detailsText += $"≤ {result.ConstraintValues[i]:F2}\n";
                }
            }
            else
            {
                solutionText = $"Решение не найдено: {result.SolutionStatus}";
                detailsText = solutionText;
            }

            txtSolution.Text = solutionText;
            txtDetails.Text = detailsText;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSolution.Text))
                {
                    MessageBox.Show("Нет решения для сохранения", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Сохранить решение",
                    DefaultExt = ".txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string solutionText = $"Решение задачи ЛП (симплекс-метод)\n\n" +
                                        $"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
                                        $"РЕШЕНИЕ:\n{txtSolution.Text}\n\n" +
                                        $"ПОДРОБНОСТИ:\n{txtDetails.Text}";

                    File.WriteAllText(saveFileDialog.FileName, solutionText);
                    MessageBox.Show("Решение успешно сохранено", "Сохранение",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            // Очистка полей ввода
            tbVariables.Text = "3";
            tbConstraints.Text = "2";
            rbMaximize.IsChecked = true;

            // Очистка целевой функции и ограничений
            objectiveFunctionItems.ItemsSource = new List<CoefficientItem>();
            constraintsItems.ItemsSource = new List<ConstraintItem>();

            // Очистка результатов
            txtSolution.Clear();
            txtDetails.Clear();
            txtValidation.Clear();
        }

        private bool ValidateInput()
        {
            StringBuilder validationErrors = new StringBuilder();

            // Проверка количества переменных и ограничений
            if (!int.TryParse(tbVariables.Text, out int varCount) || varCount <= 0)
                validationErrors.AppendLine("Количество переменных должно быть положительным числом");

            if (!int.TryParse(tbConstraints.Text, out int constrCount) || constrCount <= 0)
                validationErrors.AppendLine("Количество ограничений должно быть положительным числом");

            // Проверка коэффициентов целевой функции
            if (objectiveFunctionItems.ItemsSource is List<CoefficientItem> objectiveItems)
            {
                foreach (var item in objectiveItems)
                {
                    if (!double.TryParse(item.Coefficient.ToString(), out _))
                        validationErrors.AppendLine("Коэффициенты целевой функции должны быть числами");
                }
            }

            // Проверка ограничений
            if (constraintsItems.ItemsSource is List<ConstraintItem> constraintItems)
            {
                foreach (var constraint in constraintItems)
                {
                    foreach (var coeff in constraint.Coefficients)
                    {
                        if (!double.TryParse(coeff.Coefficient.ToString(), out _))
                            validationErrors.AppendLine("Коэффициенты ограничений должны быть числами");
                    }

                    if (!double.TryParse(constraint.RightHandSide.ToString(), out _))
                        validationErrors.AppendLine("Правые части ограничений должны быть числами");
                }
            }

            txtValidation.Text = validationErrors.ToString();
            return validationErrors.Length == 0;
        }
    }
}