﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace notepad_minus_minus;

// Editing area
public partial class MainWindow : Window
{
	private bool _leftControlPressed;
	private void SetupEditingArea()
	{
		KeyEventHandler keyDown =
			(_, key) => this._leftControlPressed = (key.Key == Key.LeftCtrl);
		KeyEventHandler keyUp =
			(_, key) => this._leftControlPressed = false;
		TextChangedEventHandler textChanged =
			(_, args) => HasChanged = true;
		this.EditingArea.KeyDown += keyDown;
		this.EditingArea.PreviewKeyDown += keyDown;
		this.EditingArea.KeyUp += keyUp;
		this.EditingArea.PreviewKeyUp += keyDown;
		this.EditingArea.PreviewMouseWheel += EditingArea_Scroll;
		this.EditingArea.MouseWheel += EditingArea_Scroll;
		this.EditingArea.SelectionChanged += OnSelectionChange;
		this.EditingArea.TextChanged += textChanged;
		OnSelectionChange(null, null);
	}
	private void EditingArea_Scroll(object _, MouseWheelEventArgs args)
	{
		bool isScrollUp = args.Delta > 0;
		if (!this._leftControlPressed ||
			(this.EditingArea.FontSize > 694.20 && isScrollUp) || 
			(this.EditingArea.FontSize < 1 && !isScrollUp)) return;

		double sizeToAdd = this.EditingArea.FontSize * 0.2;
		if (sizeToAdd > 12)
			sizeToAdd = 12;
		this.EditingArea.FontSize += sizeToAdd * (isScrollUp ? 1 : -1);
		this.ZoomInfo.Content = $"%{this.EditingArea.FontSize / 14 * 100:0}";
	}
	private void OnSelectionChange(object? _, RoutedEventArgs? _2)
	{
		this.SelectionInfo.Content =
				$"Length: {this.EditingArea.SelectionLength}, Lines: {Regex.Matches(this.EditingArea.SelectedText, "\n").Count}";
		string textB4Select = this.EditingArea.Text[0..this.EditingArea.SelectionStart];
		int b4SelectReturnCount = Regex.Matches(textB4Select, "\n").Count;
		this.DocInfo.Content =
			$"Ln: {b4SelectReturnCount + 1}, Col: {textB4Select.Length - textB4Select.LastIndexOf('\n')}, " +
			$"Pos: {this.EditingArea.SelectionStart - b4SelectReturnCount + 1}";
	}
}