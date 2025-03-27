using System.Windows;
using System.Windows.Controls;
using UP02.Pages.Main;

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
        OpenPage(new PageAuthorization());
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