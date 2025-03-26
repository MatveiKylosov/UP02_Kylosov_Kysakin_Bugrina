using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UP02;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static Frame mainFrame = new Frame();
    public MainWindow()
    {
        InitializeComponent();
        mainFrame = this.MainFrame;
    }

    public static void ClearFrame()
    {
        mainFrame.Content = null;
    }

    public static void OpenPage(Page page)
    {
        if (mainFrame != null)
        {
            mainFrame.Navigate(page);
        }
    }

    public static void GoBack()
    {
        if (mainFrame != null && mainFrame.CanGoBack)
        {
            mainFrame.GoBack();
        }
    }
}