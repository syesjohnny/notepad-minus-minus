using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace notepad_minus_minus;
/// <summary>
/// Interaction logic for WindowContainer.xaml
/// </summary>
public partial class WindowContainer : Window
{
	private volatile List<MainWindow> _windows = new();
	public WindowContainer()
	{
		InitializeComponent();
		this.Hide();
		AddWindow();
	}
	public MainWindow AddWindow()
	{
		MainWindow window = new(this);
		this._windows.Add(window);
		window.Show();
		return window;
	}
	public void RemoveWindow(MainWindow window)
	{
		window.Close();
		this._windows.Remove(window);

		if (this._windows.Count == 0)
			this.Close();
	}
	public void RemoveAll()
	{
		while (this._windows.Count > 0)
			RemoveWindow(this._windows[0]);
	}
}
