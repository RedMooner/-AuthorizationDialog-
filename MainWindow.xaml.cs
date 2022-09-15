using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.DirectoryServices.AccountManagement;
using System.Windows.Interop;
using AuthorizationDialogLib;
namespace DialogBox
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        private void Open(object sender, RoutedEventArgs e)
        {
            AuthorizationDialog authorizationDialog = new AuthorizationDialog(this); //создаем диалоговое окно
            authorizationDialog.ErrorMessage += ErrorMessage; // подписываемся на событие ошибки
            authorizationDialog.Login = "test"; // задаем логин
            authorizationDialog.Password = "test"; // задем пароь
            if (authorizationDialog.ShowDialog("Авторизация", "Логин и пароль") &&  // вызываем диалог и проверяем что все успешно
                authorizationDialog.DialogResult == AuthorizationDialog.DialogResults.OK)
                MessageBox.Show("Успешно!");
            else
                MessageBox.Show("не верный логин и пароль");
        }

        private void ErrorMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}

