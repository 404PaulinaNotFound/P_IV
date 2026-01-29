using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using FinanseApp;

namespace FinanseApp.Tests
{
    public class MainWindowTests
    {
        // Test 1: Logika zarządzania transakcjami - dodawanie wydatku
        [Fact]
        public void AddButton_Click_ShouldAddExpenseToCategory()
        {
            // Arrange
            var window = new MainWindow();
            var categoryAmounts = GetPrivateField<Dictionary<string, Dictionary<int, decimal>>>(window, "_categoryAmounts");
            SetPrivateField(window, "_selectedCategory", "Zakupy");
            SetPrivateField(window, "_selectedMonth", 1);
            SetPrivateField(window, "_selectedAmount", 500);

            // Act
            InvokePrivateMethod(window, "AddButton_Click", null, null);

            // Assert
            Assert.True(categoryAmounts.ContainsKey("Zakupy"));
            Assert.True(categoryAmounts["Zakupy"].ContainsKey(1));
            Assert.Equal(500m, categoryAmounts["Zakupy"][1]);
        }

        // Test 2: Walidacja danych - próba dodania bez wybranej kwoty
        [Fact]
        public void AddButton_Click_WithoutSelectedAmount_ShouldShowError()
        {
            // Arrange
            var window = new MainWindow();
            SetPrivateField(window, "_selectedAmount", 0);
            var errorTextBlock = GetPrivateField<Avalonia.Controls.TextBlock>(window, "_errorTextBlock");

            // Act
            InvokePrivateMethod(window, "AddButton_Click", null, null);

            // Assert
            Assert.Equal("Wybierz kwotę.", errorTextBlock.Text);
        }

        // Test 3: Obliczenia finansowe - sumowanie wydatków w miesiącu
        [Fact]
        public void UpdateBudgetInfo_ShouldCalculateMonthlyExpensesCorrectly()
        {
            // Arrange
            var window = new MainWindow();
            var categoryAmounts = GetPrivateField<Dictionary<string, Dictionary<int, decimal>>>(window, "_categoryAmounts");
            
            // Dodaj wydatki w różnych kategoriach dla miesiąca 3
            categoryAmounts["Zakupy"] = new Dictionary<int, decimal> { { 3, 300m } };
            categoryAmounts["Paliwo"] = new Dictionary<int, decimal> { { 3, 200m } };
            categoryAmounts["Rozrywka"] = new Dictionary<int, decimal> { { 3, 150m } };
            
            SetPrivateField(window, "_selectedMonth", 3);
            var monthlyBalanceText = GetPrivateField<Avalonia.Controls.TextBlock>(window, "_monthlyBalanceText");

            // Act
            InvokePrivateMethod(window, "UpdateBudgetInfo");

            // Assert
            Assert.Contains("650", monthlyBalanceText.Text); // 300 + 200 + 150 = 650
        }

        // Test 4: Operacje na plikach JSON - zapis i odczyt danych
        [Fact]
        public void SaveAndLoadData_ShouldPreserveData()
        {
            // Arrange
            var testFilePath = "test_finanse_data.json";
            var window = new MainWindow();
            var categoryAmounts = GetPrivateField<Dictionary<string, Dictionary<int, decimal>>>(window, "_categoryAmounts");
            
            categoryAmounts["Zakupy"] = new Dictionary<int, decimal> { { 1, 500m } };
            categoryAmounts["Oszczędności"] = new Dictionary<int, decimal> { { 1, 1000m } };
            SetPrivateField(window, "_savingsGoal", 5000m);

            // Act - Save
            window.SaveDataToFile(testFilePath);

            // Create new window and load
            var newWindow = new MainWindow();
            newWindow.LoadDataFromFile(testFilePath);
            var loadedCategoryAmounts = GetPrivateField<Dictionary<string, Dictionary<int, decimal>>>(newWindow, "_categoryAmounts");
            var loadedSavingsGoal = GetPrivateField<decimal>(newWindow, "_savingsGoal");

            // Assert
            Assert.Equal(500m, loadedCategoryAmounts["Zakupy"][1]);
            Assert.Equal(1000m, loadedCategoryAmounts["Oszczędności"][1]);
            Assert.Equal(5000m, loadedSavingsGoal);

            // Cleanup
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }

        // Helper methods for accessing private members using reflection
        private T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            field.SetValue(obj, value);
        }

        private void InvokePrivateMethod(object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            method.Invoke(obj, parameters);
        }
    }
}
