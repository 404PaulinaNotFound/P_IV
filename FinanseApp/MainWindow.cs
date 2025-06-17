using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanseApp
{
    public class MainWindow : Window
    {
        private Button _addButton;
        private TextBox _annualGoalTextBox;
        private TextBlock _monthlyBalanceText;
        private TextBlock _categoryBreakdownText;
        private TextBlock _errorTextBlock;
        private StackPanel _categoryButtonsPanel;
        private StackPanel _monthButtonsPanel;
        private int _selectedAmount = 0;

        private Dictionary<string, Dictionary<int, decimal>> _categoryAmounts = new();
        private decimal _savingsGoal = 1000;
        private TextBlock _savingsProgressText;
        private string _selectedCategory = "Jedzenie";
        private int _selectedMonth = DateTime.Today.Month;

        public MainWindow()
        {
            Width = 400;
            Height = 700;
            Title = "Aplikacja Finansowa";
            Background = new SolidColorBrush(Color.Parse("#FFC0CB"));

            var scrollViewer = new ScrollViewer
            {
                Content = CreateMainPanel()
            };

            Content = scrollViewer;
        }

        private Panel CreateMainPanel()
        {
            var mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            // Kategorie
            mainPanel.Children.Add(new TextBlock { Text = "Kategoria:", Margin = new Thickness(5) });

            _categoryButtonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5)
            };

            var categories = new List<string> { "Jedzenie", "Transport", "Rozrywka", "Oszczędności", "Opłaty" };
            foreach (var category in categories)
            {
                var button = new Button
                {
                    Content = category,
                    Tag = category,
                    Padding = new Thickness(5),
                    Background = category == _selectedCategory ? Brushes.LightBlue : Brushes.LightGray,
                    Margin = new Thickness(5)
                };

                button.Click += (_, __) =>
                {
                    _selectedCategory = (string)button.Tag;

                    foreach (var child in _categoryButtonsPanel.Children.OfType<Button>())
                        child.Background = Brushes.LightGray;

                    button.Background = Brushes.LightBlue;
                };

                _categoryButtonsPanel.Children.Add(button);
            }

            mainPanel.Children.Add(_categoryButtonsPanel);

            // Szybkie kwoty
            mainPanel.Children.Add(new TextBlock { Text = "Wybierz szybką kwotę:", Margin = new Thickness(5) });

            var amountButtonsPanel = new WrapPanel
            {
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            for (int i = 100; i <= 2000; i += 100)
            {
                var button = new Button
                {
                    Content = $"{i} zł",
                    Tag = i,
                    Padding = new Thickness(5),
                    Width = 60,
                    Margin = new Thickness(5),
                    Background = Brushes.LightGray
                };

                button.Click += (_, __) =>
                {
                    _selectedAmount = (int)button.Tag;

                    foreach (var child in amountButtonsPanel.Children.OfType<Button>())
                        child.Background = Brushes.LightGray;

                    button.Background = Brushes.LightBlue;
                };

                amountButtonsPanel.Children.Add(button);
            }

            mainPanel.Children.Add(amountButtonsPanel);

            // Miesiąc
            mainPanel.Children.Add(new TextBlock { Text = "Miesiąc:", Margin = new Thickness(5) });

            _monthButtonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            for (int i = 1; i <= 12; i++)
            {
                string label = i.ToString("D2");
                var button = new Button
                {
                    Content = label,
                    Tag = i,
                    Padding = new Thickness(5),
                    Background = i == _selectedMonth ? Brushes.LightBlue : Brushes.LightGray,
                    Margin = new Thickness(5)
                };

                button.Click += (_, __) =>
                {
                    _selectedMonth = (int)button.Tag;

                    foreach (var child in _monthButtonsPanel.Children.OfType<Button>())
                        child.Background = Brushes.LightGray;

                    button.Background = Brushes.LightBlue;
                    UpdateBudgetInfo();
                };

                _monthButtonsPanel.Children.Add(button);
            }

            mainPanel.Children.Add(_monthButtonsPanel);

            // Dodaj
            _addButton = new Button
            {
                Content = "Dodaj",
                Width = 200,
                Margin = new Thickness(5)
            };
            _addButton.Click += AddButton_Click;
            mainPanel.Children.Add(_addButton);

// Cel oszczędności
mainPanel.Children.Add(new TextBlock { Text = "Cel oszczędności:", Margin = new Thickness(5) });

var savingsGoalPanel = new WrapPanel
{
    Margin = new Thickness(5),
    HorizontalAlignment = HorizontalAlignment.Center
};

for (int i = 500; i <= 10000; i = i == 500 ? 1000 : i + 1000)
{
    var button = new Button
    {
        Content = $"{i} zł",
        Tag = i,
        Padding = new Thickness(5),
        Width = 80,
        Margin = new Thickness(5),
        Background = i == _savingsGoal ? Brushes.LightBlue : Brushes.LightGray
    };

    button.Click += (_, __) =>
    {
        _savingsGoal = Convert.ToDecimal(button.Tag);

        foreach (var child in savingsGoalPanel.Children.OfType<Button>())
            child.Background = Brushes.LightGray;

        button.Background = Brushes.LightBlue;
        UpdateBudgetInfo(); 
    };

    savingsGoalPanel.Children.Add(button);
}

mainPanel.Children.Add(savingsGoalPanel);

_savingsProgressText = new TextBlock
{
    Margin = new Thickness(5),
    TextWrapping = TextWrapping.Wrap
};
mainPanel.Children.Add(_savingsProgressText);

            // Wydatki w miesiącu
            _monthlyBalanceText = new TextBlock
            {
                Text = "Wydatki w wybranym miesiącu: 0,00 zł",
                Margin = new Thickness(5)
            };
            mainPanel.Children.Add(_monthlyBalanceText);
    _categoryBreakdownText = new TextBlock
    {
    Margin = new Thickness(5),
    TextWrapping = TextWrapping.Wrap
    };
    mainPanel.Children.Add(_categoryBreakdownText);
            // Błędy
            _errorTextBlock = new TextBlock
            {
                Foreground = Brushes.Red,
                Margin = new Thickness(5),
                TextWrapping = TextWrapping.Wrap
            };
            mainPanel.Children.Add(_errorTextBlock);

            return mainPanel;
        }

        private void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _errorTextBlock.Text = string.Empty;

            if (_selectedAmount <= 0)
            {
                _errorTextBlock.Text = "Wybierz kwotę.";
                return;
            }

            var category = _selectedCategory;
            var monthKey = _selectedMonth;

            if (!_categoryAmounts.ContainsKey(category))
                _categoryAmounts[category] = new Dictionary<int, decimal>();

            if (_categoryAmounts[category].ContainsKey(monthKey))
                _categoryAmounts[category][monthKey] += _selectedAmount;
            else
                _categoryAmounts[category][monthKey] = _selectedAmount;

            _selectedAmount = 0;
            UpdateBudgetInfo();
        }

        private void UpdateBudgetInfo()
        {
            int selectedMonth = _selectedMonth;

            var totalSpent = _categoryAmounts
                .SelectMany(kv => kv.Value)
                .Where(e => e.Key == selectedMonth)
                .Sum(e => e.Value);

            _monthlyBalanceText.Text = $"Wydatki w wybranym miesiącu: {totalSpent:C}";
            var breakdownLines = _categoryAmounts
    .Where(kv => kv.Value.ContainsKey(_selectedMonth))
    .Select(kv => $"{kv.Key}: {kv.Value[_selectedMonth]:C}");

            _categoryBreakdownText.Text = string.Join("\n", breakdownLines);

decimal totalSavings = 0;

if (_categoryAmounts.TryGetValue("Oszczędności", out var savingsByMonth))
{
    totalSavings = savingsByMonth.Values.Sum();
}

decimal percent = _savingsGoal > 0 ? Math.Min(100, (totalSavings / _savingsGoal) * 100) : 0;

_savingsProgressText.Text = $"Zebrano oszczędności: {totalSavings:C} / {_savingsGoal:C} ({percent:F1}%)";
        }
    }
} 