using _01S.ViewModels;
using System.Windows;

namespace _01S
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Создаёт главное окно и задаёт его модель представления.
        /// </summary>
        /// <param name="viewModel">Модель представления главного окна.</param>
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
