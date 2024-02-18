using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace notepad_minus_minus;

// Only for misc/core thing
public partial class MainWindow : Window
{
	private bool _hasChanged = false;
	private string _currentFilePath = string.Empty;
	private WindowContainer _parent;

	#region Properties
	public bool HasChanged
	{
		get => _hasChanged;
		set
		{
			_hasChanged = value;
			SetTitle();
		}
	}
	public string CurrentFilePath
	{
		get => _currentFilePath;
		set
		{
			_currentFilePath = value;
			SetTitle();
			if (!string.IsNullOrEmpty(value))
			{
				this.OpenFileFolderMenuItem.IsEnabled = true;
				this.ReOpenMenuItem.IsEnabled = true;
				this.DeleteFileMenuItem.IsEnabled = true;
				this.ReOpenNotepadMenuItem.IsEnabled = true;
			}
			else
			{
				this.OpenFileFolderMenuItem.IsEnabled = false;
				this.ReOpenMenuItem.IsEnabled = false;
				this.DeleteFileMenuItem.IsEnabled = false;
				this.ReOpenNotepadMenuItem.IsEnabled = false;
			}
		}
	}
	#endregion
	public MainWindow(WindowContainer parent)
	{
		InitializeComponent();
		this.SetupEditingArea();

		SetTitle();
		this._parent = parent;
		this.Closing += OnClosing;
	}

	#region Window/File Control
	private void SetTitle()
	{
		bool isEmpty = string.IsNullOrEmpty(CurrentFilePath);
		this.Title = string.Format(
			"{0}{1} {2}{3}{4}",
			!HasChanged ? "" : "*",
			isEmpty ? "Unnamed" : Path.GetFileName(CurrentFilePath),
			isEmpty ? "" : "(",
			CurrentFilePath,
			isEmpty ? "" : ")"
		);
	}
	private void NewWindow(object _, RoutedEventArgs _2) =>
		this._parent.AddWindow();

	#region File
	private void OnNewFile(object _, RoutedEventArgs _2)
	{
		var result = AskSaveChange();

		if (result == MessageBoxResult.Cancel) return;
		if (result == MessageBoxResult.Yes) SaveFile();
		CurrentFilePath = string.Empty;
		this.EditingArea.Clear();
		HasChanged = false;
	}
	/// <summary>
	/// It DOES check haschanged property
	/// </summary>
	/// <returns>
	/// null: failed to ask
	/// other: returned from message box
	/// </returns>
	private MessageBoxResult? AskSaveChange()
	{
		if (HasChanged &&
			!string.IsNullOrEmpty(this.EditingArea.Text) &&
			(string.IsNullOrEmpty(CurrentFilePath) || File.Exists(CurrentFilePath)))
		{
			return MessageBox.Show("Do you want to save changes?", "Warning", MessageBoxButton.YesNoCancel);
		}
		return null;
	}
	private void OnSave(object _, RoutedEventArgs _2) => SaveFile();
	private void OnSaveAs(object _, RoutedEventArgs _2)
	{
		SaveFileDialog dialog = new()
		{
			Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
			Title = "Save File"
		};

		if (dialog.ShowDialog() == true)
		{
			CurrentFilePath = dialog.FileName;
			File.WriteAllText(CurrentFilePath, this.EditingArea.Text);
		}
	}
	private void SaveFile()
	{
		if (string.IsNullOrEmpty(CurrentFilePath))
		{
			SaveFileDialog dialog = new()
			{
				Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
				Title = "Save File"
			};

			if (dialog.ShowDialog() == true)
			{
				CurrentFilePath = dialog.FileName;
				File.WriteAllText(CurrentFilePath, this.EditingArea.Text);
				HasChanged = false;
			}
			return;
		}

		File.WriteAllText(CurrentFilePath, this.EditingArea.Text);
	}
	private void OpenFile(string path)
	{
		CurrentFilePath = path;
		this.EditingArea.Text = File.ReadAllText(CurrentFilePath);
		HasChanged = false;
	}
	private void OnOpen(object _, RoutedEventArgs _2)
	{
		var result = AskSaveChange();

		if (result == MessageBoxResult.Yes)
			SaveFile();

		OpenFileDialog openFileDialog = new()
		{
			Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*"
		};
		if (openFileDialog.ShowDialog() == true)
		{
			OpenFile(openFileDialog.FileName);
		}
	}
	private void OnReOpen(object sender, RoutedEventArgs e)
	{
		var result = AskSaveChange();

		if (result == MessageBoxResult.Yes)
			SaveFile();

		OpenFile(CurrentFilePath);
	}
	private void OpenInCMD(object _, RoutedEventArgs _2) =>
		Process.Start("cmd", $"/k cd /d \"{Path.GetDirectoryName(this.CurrentFilePath)}\"");
	private void OpenInExplorer(object _, RoutedEventArgs _2) =>
		Process.Start("explorer", Path.GetDirectoryName(this.CurrentFilePath)!);
	private void OnExitWindow(object _, RoutedEventArgs _2) => 
		this._parent.RemoveWindow(this);
	private void OnExitAll(object _, RoutedEventArgs _2) =>
		this._parent.RemoveAll();
	private void OnClosing(object? _, EventArgs _2)
	{
		var result = AskSaveChange();

		if (result == MessageBoxResult.Cancel) return;
		if (result == MessageBoxResult.Yes) SaveFile();
	}
	private void RemoveCurrentFile(object _, RoutedEventArgs _2)
	{
		FileSystem.DeleteFile(CurrentFilePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
		// it is not neccessry to make options of delete permently/send to recycle bin cuz send to recycle bin actually follows system options
		HasChanged = false;
		OnNewFile(null!, null!);
	}
	#endregion

	#endregion

	private void ReOpenInNotepad(object sender, RoutedEventArgs e) =>
		Process.Start("notepad.exe", CurrentFilePath);
}