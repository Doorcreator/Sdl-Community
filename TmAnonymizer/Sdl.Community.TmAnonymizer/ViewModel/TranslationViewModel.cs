﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Sdl.Community.SdlTmAnonymizer.Commands;
using Sdl.Community.SdlTmAnonymizer.Helpers;
using Sdl.Community.SdlTmAnonymizer.Model;
using Sdl.Community.SdlTmAnonymizer.Services;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using PreviewWindow = Sdl.Community.SdlTmAnonymizer.View.PreviewWindow;
using WaitWindow = Sdl.Community.SdlTmAnonymizer.View.WaitWindow;

namespace Sdl.Community.SdlTmAnonymizer.ViewModel
{
	public class TranslationViewModel : ViewModelBase, IDisposable
	{
		private readonly ObservableCollection<TmFile> _tmsCollection;
		private readonly ObservableCollection<AnonymizeTranslationMemory> _anonymizeTranslationMemories;
		private readonly TranslationMemoryViewModel _translationMemoryViewModel;
		private readonly BackgroundWorker _backgroundWorker;		
		private readonly Settings _settings;
		private readonly SettingsService _settingsService;
		private TmFile _selectedTm;
		private ObservableCollection<Rule> _rules;
		private Rule _selectedItem;
		private bool _selectAll;
		private ICommand _selectAllCommand;
		private ICommand _previewCommand;
		private ICommand _removeRuleCommand;
		private ICommand _importCommand;
		private ICommand _exportCommand;
		private ObservableCollection<SourceSearchResult> _sourceSearchResults;
		private WaitWindow _waitWindow;
		private IList _selectedItems;

		public TranslationViewModel(TranslationMemoryViewModel translationMemoryViewModel)
		{
			_translationMemoryViewModel = translationMemoryViewModel;

			_settingsService = _translationMemoryViewModel.SettingsService;
			_settings = _settingsService.GetSettings();

			_selectedItems = new List<Rule>();

			_anonymizeTranslationMemories = new ObservableCollection<AnonymizeTranslationMemory>();

			_rules = new ObservableCollection<Rule>(_settingsService.GetRules());
			foreach (var rule in _rules)
			{
				rule.PropertyChanged += Rule_PropertyChanged;
			}

			_sourceSearchResults = new ObservableCollection<SourceSearchResult>();

			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.DoWork += BackgroundWorker_DoWork;
			_backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;


			_tmsCollection = _translationMemoryViewModel.TmsCollection;
			_tmsCollection.CollectionChanged += TmsCollection_CollectionChanged;

			_translationMemoryViewModel.PropertyChanged += _translationMemoryViewModel_PropertyChanged;

			RulesCollection.CollectionChanged += RulesCollection_CollectionChanged;
		}		

		public ICommand SelectAllCommand => _selectAllCommand ?? (_selectAllCommand = new CommandHandler(SelectAllRules, true));

		public ICommand PreviewCommand => _previewCommand ?? (_previewCommand = new CommandHandler(PreviewChanges, true));

		public ICommand RemoveRuleCommand => _removeRuleCommand ?? (_removeRuleCommand = new CommandHandler(RemoveRule, true));

		public ICommand ImportCommand => _importCommand ?? (_importCommand = new CommandHandler(Import, true));

		public ICommand ExportCommand => _exportCommand ?? (_exportCommand = new CommandHandler(Export, true));

		public IList SelectedItems
		{
			get => _selectedItems;
			set
			{
				_selectedItems = value;
				OnPropertyChanged(nameof(SelectedItems));
			}
		}		

		public ObservableCollection<SourceSearchResult> SourceSearchResults
		{
			get => _sourceSearchResults;
			set
			{
				if (Equals(value, _sourceSearchResults))
				{
					return;
				}
				_sourceSearchResults = value;
				OnPropertyChanged(nameof(SourceSearchResults));
			}
		}

		public ObservableCollection<Rule> RulesCollection
		{
			get => _rules;
			set
			{
				if (Equals(value, _rules))
				{
					return;
				}
				_rules = value;
				OnPropertyChanged(nameof(RulesCollection));
			}
		}

		public Rule SelectedItem
		{
			get => _selectedItem;
			set
			{
				_selectedItem = value;
				OnPropertyChanged(nameof(SelectedItem));
				if (RulesCollection.Any(r => r.Id == null))
				{
					SetIdForNewRules();
				}
			}
		}

		public TmFile SelectedTm
		{
			get => _selectedTm;
			set
			{
				_selectedTm = value;
				OnPropertyChanged(nameof(SelectedTm));
			}
		}

		public bool SelectAll
		{
			get => _selectAll;
			set
			{
				if (Equals(value, _selectAll))
				{
					return;
				}
				_selectAll = value;
				OnPropertyChanged(nameof(SelectAll));
			}
		}

		private void Export()
		{
			if (SelectedItems.Count > 0)
			{
				var selectedRules = new List<Rule>();
				var fileDialog = new SaveFileDialog
				{
					Title = StringResources.Export_Export_selected_expressions,
					Filter = @"Excel |*.xlsx"
				};

				var result = fileDialog.ShowDialog();
				if (result == DialogResult.OK && fileDialog.FileName != string.Empty)
				{
					foreach (var rule in SelectedItems.OfType<Rule>())
					{
						selectedRules.Add(rule);
					}
					Expressions.ExportExporessions(fileDialog.FileName, selectedRules);
					System.Windows.Forms.MessageBox.Show(StringResources.Export_File_was_exported_successfully_to_selected_location, System.Windows.Forms.Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			else
			{
				System.Windows.Forms.MessageBox.Show(StringResources.Export_Please_select_at_least_one_row_to_export, System.Windows.Forms.Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void Import()
		{
			var fileDialog = new OpenFileDialog
			{
				Title = StringResources.Import_Please_select_the_files_you_want_to_import,
				Filter = @"Excel |*.xlsx",
				CheckFileExists = true,
				CheckPathExists = true,
				DefaultExt = "xlsx",
				Multiselect = true
			};

			var result = fileDialog.ShowDialog();
			if (result == DialogResult.OK && fileDialog.FileNames.Length > 0)
			{
				var importedExpressions = Expressions.GetImportedExpressions(fileDialog.FileNames.ToList());

				foreach (var expression in importedExpressions)
				{
					var ruleExist = RulesCollection.FirstOrDefault(s => s.Name.Equals(expression.Name));
					if (ruleExist == null)
					{
						RulesCollection.Add(expression);
					}
				}

				_settings.Rules = RulesCollection.ToList();
				_settingsService.SaveSettings(_settings);
			}
		}

		private void RemoveRule()
		{
			var message = System.Windows.Forms.MessageBox.Show(StringResources.RemoveRule_Are_you_sure_you_want_to_remove_selected_rules,
				System.Windows.Forms.Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

			if (message == DialogResult.OK)
			{
				if (SelectedItems != null)
				{
					var selectedRules = new List<Rule>();

					foreach (var selectedItem in SelectedItems)
					{
						if (!selectedItem.GetType().Name.Equals("NamedObject"))
						{
							var item = (Rule)selectedItem;
							var rule = new Rule
							{
								Id = item.Id
							};
							selectedRules.Add(rule);
						}

					}
					SelectedItems.Clear();
					foreach (var rule in selectedRules)
					{
						var ruleRoRemove = RulesCollection.FirstOrDefault(r => r.Id.Equals(rule.Id));
						if (ruleRoRemove != null)
						{
							RulesCollection.Remove(ruleRoRemove);
						}
					}
				}

				_settings.Rules = RulesCollection.ToList();
				_settingsService.SaveSettings(_settings);
			}
		}

		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			System.Windows.Application.Current.Dispatcher.Invoke(delegate
			{
				var previewViewModel = new PreviewWindowViewModel(SourceSearchResults, _anonymizeTranslationMemories,
					_tmsCollection, _translationMemoryViewModel);

				var previewWindow = new PreviewWindow(previewViewModel);
				previewWindow.Loaded += PreviewWindow_Loaded;
				previewWindow.Closing += PreviewWindow_Closing;
				previewWindow.ShowDialog();
			});
		}

		private void PreviewWindow_Loaded(object sender, RoutedEventArgs e)
		{
			_waitWindow?.Close();
		}

		private void PreviewWindow_Closing(object sender, CancelEventArgs e)
		{
			SourceSearchResults.Clear();
		}

		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var selectedTms = _tmsCollection.Where(t => t.IsSelected).ToList();
			var selectedRulesCount = RulesCollection.Count(r => r.IsSelected);
			if (selectedTms.Count > 0 && selectedRulesCount > 0)
			{
				System.Windows.Application.Current.Dispatcher.Invoke(delegate
				{
					_waitWindow = new WaitWindow();
					_waitWindow.Show();
				});
				System.Windows.Application.Current.Dispatcher.Invoke(delegate { }, DispatcherPriority.Background);

				var serverTms = selectedTms.Where(s => s.IsServerTm).ToList();
				if (serverTms.Any())
				{
					var uri = new Uri(_translationMemoryViewModel.Credentials.Url);
					var translationProvider = new TranslationProviderServer(uri, false,
						_translationMemoryViewModel.Credentials.UserName,
						_translationMemoryViewModel.Credentials.Password);

					//get all tus for selected translation memories
					foreach (var serverTm in serverTms)
					{
						var tus = _translationMemoryViewModel.TmService.ServerBasedTmGetTranslationUnits(translationProvider, serverTm.Path,
							SourceSearchResults, GetSelectedRules());
						if (!_anonymizeTranslationMemories.Any(n => n.TmPath.Equals(tus.TmPath)))
						{
							_anonymizeTranslationMemories.Add(tus);
						}
					}
				}

				//file based tms
				foreach (var tm in selectedTms.Where(s => !s.IsServerTm))
				{
					var tus = _translationMemoryViewModel.TmService.FileBaseTmGetTranslationUnits(tm.Path, SourceSearchResults, GetSelectedRules());
					if (!_anonymizeTranslationMemories.Any(n => n.TmPath.Equals(tus.TmPath)))
					{
						_anonymizeTranslationMemories.Add(tus);
					}
				}
			}
			else
			{
				System.Windows.Forms.MessageBox.Show(StringResources.Please_select_at_least_one_translation_memory_and_a_rule_to_preview_the_changes,
					System.Windows.Forms.Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

		}

		private void _translationMemoryViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals("TmsCollection"))
			{
				//removed from tm collection
				RefreshPreviewWindow();
			}
		}

		private void RulesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (var item in e.NewItems)
				{
					var rule = (Rule)item;
					rule.PropertyChanged += Rule_PropertyChanged;
				}
			}
		}

		private void Rule_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			_settings.Rules = RulesCollection.ToList();
			_settingsService.SaveSettings(_settings);
		}

		private void TmsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				if (e.OldItems == null) return;
				foreach (TmFile removedTm in e.OldItems)
				{
					//Remove search resoults for deleted tm
					var tusForRemovedTm = SourceSearchResults.Where(t => t.TmFilePath.Equals(removedTm.Path)).ToList();
					foreach (var tu in tusForRemovedTm)
					{
						SourceSearchResults.Remove(tu);
					}
					//remove the tm from the list use in preview windoew
					var removed = _anonymizeTranslationMemories.FirstOrDefault(t => t.TmPath.Equals(removedTm.Path));
					if (removed != null)
					{
						_anonymizeTranslationMemories.Remove(removed);
					}
				}
			}

			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (TmFile newTm in e.NewItems)
				{
					newTm.PropertyChanged += NewTm_PropertyChanged;
				}
			}
		}

		private void NewTm_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			RefreshPreviewWindow();
		}

		private void RefreshPreviewWindow()
		{
			var unselectedTms = _tmsCollection.Where(t => !t.IsSelected).ToList();
			foreach (var tm in unselectedTms)
			{
				var anonymizedTmToRemove = _anonymizeTranslationMemories.FirstOrDefault(t => t.TmPath.Equals(tm.Path));
				if (anonymizedTmToRemove != null)
				{
					_anonymizeTranslationMemories.Remove(anonymizedTmToRemove);
				}

				//remove search results for that tm
				var searchResultsForTm = SourceSearchResults.Where(r => r.TmFilePath.Equals(tm.Path)).ToList();
				foreach (var result in searchResultsForTm)
				{
					SourceSearchResults.Remove(result);
				}
			}
		}

		private void PreviewChanges()
		{
			_backgroundWorker.RunWorkerAsync();
		}

		private List<Rule> GetSelectedRules()
		{
			return RulesCollection.Where(r => r.IsSelected).ToList();
		}

		private void SelectAllRules()
		{
			foreach (var rule in RulesCollection)
			{
				rule.IsSelected = SelectAll;
			}
		}

		private void SetIdForNewRules()
		{
			var newRules = RulesCollection.Where(r => r.Id == null).ToList();
			foreach (var rule in newRules)
			{
				rule.Id = Guid.NewGuid().ToString();
			}
		}

		public void Dispose()
		{
			_backgroundWorker.DoWork -= BackgroundWorker_DoWork;
			_backgroundWorker.RunWorkerCompleted -= BackgroundWorker_RunWorkerCompleted;
			_backgroundWorker?.Dispose();

			_tmsCollection.CollectionChanged -= TmsCollection_CollectionChanged;
			_translationMemoryViewModel.PropertyChanged -= _translationMemoryViewModel_PropertyChanged;

			foreach (var rule in _rules)
			{
				rule.PropertyChanged -= Rule_PropertyChanged;
			}

			RulesCollection.CollectionChanged -= RulesCollection_CollectionChanged;
		}
	}
}
