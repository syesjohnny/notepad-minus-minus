using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
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
		get => this._hasChanged;
		set
		{
			this._hasChanged = value;
			this.SetTitle();
		}
	}
	public string CurrentFilePath
	{
		get => this._currentFilePath;
		set
		{
			this._currentFilePath = value;
			this.SetTitle();
			if (!string.IsNullOrEmpty(value))
			{
				this.OpenFileFolderMenuItem.IsEnabled = true;
				this.ReOpenMenuItem.IsEnabled = true;
				this.DeleteFileMenuItem.IsEnabled = true;
				this.ReOpenNotepadMenuItem.IsEnabled = true;
				this.FileReadonly.IsEnabled = true;
			}
			else
			{
				this.OpenFileFolderMenuItem.IsEnabled = false;
				this.ReOpenMenuItem.IsEnabled = false;
				this.DeleteFileMenuItem.IsEnabled = false;
				this.ReOpenNotepadMenuItem.IsEnabled = false;
				this.FileReadonly.IsEnabled = false;
			}
		}
	}
	#endregion
	public MainWindow(WindowContainer parent)
	{
		this.InitializeComponent();
		this.SetupEditingArea();

		this.SetTitle();
		this._parent = parent;
		this.Closing += this.OnClosing;
	}

	#region Window/File Control
	private void SetTitle()
	{
		bool isEmpty = string.IsNullOrEmpty(this.CurrentFilePath);
		this.Title = string.Format(
			"{0}{1} {2}{3}{4}",
			!this.HasChanged ? "" : "*",
			isEmpty ? "Unnamed" : Path.GetFileName(this.CurrentFilePath),
			isEmpty ? "" : "(",
			this.CurrentFilePath,
			isEmpty ? "" : ")"
		);
	}
	private void NewWindow(object _, RoutedEventArgs _2) =>
		this._parent.AddWindow();

	#region File
	private void OnNewFile(object _, RoutedEventArgs _2)
	{
		var result = this.AskSaveChange();

		if (result == MessageBoxResult.Cancel) return;
		if (result == MessageBoxResult.Yes) this.SaveFile();
		this.CurrentFilePath = string.Empty;
		this.EditingArea.Clear();
		this.HasChanged = false;
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
		if (this.HasChanged &&
			!string.IsNullOrEmpty(this.EditingArea.Text) &&
			(string.IsNullOrEmpty(this.CurrentFilePath) || File.Exists(this.CurrentFilePath)))
		{
			return MessageBox.Show("Do you want to save changes?", "Warning", MessageBoxButton.YesNoCancel);
		}
		return null;
	}
	private void OnSave(object _, RoutedEventArgs _2) => this.SaveFile();
	private void OnSaveAs(object _, RoutedEventArgs _2)
	{
		SaveFileDialog dialog = new()
		{
			Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
			Title = "Save File"
		};

		if (dialog.ShowDialog() == true)
		{
			this.CurrentFilePath = dialog.FileName;
			File.WriteAllText(this.CurrentFilePath, this.EditingArea.Text);
		}
	}
	private void SaveFile()
	{
		if (string.IsNullOrEmpty(this.CurrentFilePath))
		{
			SaveFileDialog dialog = new()
			{
				Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
				Title = "Save File"
			};

			if (dialog.ShowDialog() == true)
			{
				this.CurrentFilePath = dialog.FileName;
            }
			return;
		}
		try
		{
			File.WriteAllText(this.CurrentFilePath, this.EditingArea.Text);
		}
		catch (UnauthorizedAccessException)
		{
			MessageBox.Show("File is read-only!");
		}
		catch (SecurityException)
		{
			MessageBox.Show("Permission denied.");
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
		this.HasChanged = false;
	}
	private void OpenFile(string path)
	{
		this.CurrentFilePath = path;
		this.EditingArea.Text = File.ReadAllText(this.CurrentFilePath);
		this.HasChanged = false;
		this.FileReadonly.IsChecked = (File.GetAttributes(this.CurrentFilePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
    }
	private void OnOpen(object _, RoutedEventArgs _2)
	{
		var result = this.AskSaveChange();

		if (result == MessageBoxResult.Yes)
			this.SaveFile();

		OpenFileDialog openFileDialog = new()
		{
			Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*"
		};
		if (openFileDialog.ShowDialog() == true)
		{
			this.OpenFile(openFileDialog.FileName);
		}
	}
	private void OnReOpen(object sender, RoutedEventArgs e)
	{
		var result = this.AskSaveChange();

		if (result == MessageBoxResult.Yes)
			this.SaveFile();

		this.OpenFile(this.CurrentFilePath);
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
		var result = this.AskSaveChange();

		if (result == MessageBoxResult.Cancel) return;
		if (result == MessageBoxResult.Yes) this.SaveFile();
	}
	private void RemoveCurrentFile(object _, RoutedEventArgs _2)
	{
        var check = MessageBox.Show("Do you want to save changes?", "Warning", MessageBoxButton.YesNoCancel);
		if (check == MessageBoxResult.Yes) {
			FileSystem.DeleteFile(this.CurrentFilePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
			this.HasChanged = false;
			this.OnNewFile(null!, null!);
        }
	}
	private void ReadonlyFile(object _, RoutedEventArgs _2)
	{
        if (this.FileReadonly.IsChecked)
        {
            File.SetAttributes(this._currentFilePath, File.GetAttributes(this._currentFilePath) | FileAttributes.ReadOnly);
        }
        else
        {
            File.SetAttributes(this._currentFilePath, File.GetAttributes(this._currentFilePath) & ~FileAttributes.ReadOnly);
        }
    }
	#endregion

	#endregion

	private void ReOpenInNotepad(object sender, RoutedEventArgs e) =>
		Process.Start("notepad.exe", this.CurrentFilePath);
}