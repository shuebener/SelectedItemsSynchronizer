namespace SelectedItemsBindingDemo
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Linq;
	using System.Threading;
	using System.Windows.Input;

	public class ViewModel : INotifyPropertyChanged
	{
		private readonly ObservableCollection<string> selectedNames = [];
		private readonly ObservableCollection<string> selectedSecondaries = [];
		private readonly ObservableCollection<DateTime> selectedDates = [DateTime.Today];

		private string? summary = null;

		private int selectingMap;
		private readonly object selectingMapSynchLock = new();

		public ViewModel()
		{
			selectedNames.CollectionChanged += SelectedNamesCollectionChanged;
			selectedDates.CollectionChanged += SelectedDatesCollectionChanged;
		}

		void SelectedDatesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(StartDate));
			OnPropertyChanged(nameof(EndDate));
		}

		private void SelectedNamesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			lock (selectingMapSynchLock)
			{
				if (SelectedNames.Count > 0)
				{
					System.Diagnostics.Debug.Print("SelectedNames.CollectionChanged={0}", SelectedNames.Aggregate((i, o) => o = string.Format("{0}, {1}", o, i)));
				}
				else
				{
					System.Diagnostics.Debug.Print("SelectedNames.CollectionChanged=empty");
				}
				Interlocked.Increment(ref selectingMap);
				if (SelectedNames.Count == 1)
				{
					SelectMap(false, SelectedNames.Single());
				}
				else
				{
					SelectedSecondaries.Clear();
				}
				Interlocked.Decrement(ref selectingMap);
			}
		}

		public static IEnumerable<string> AvailableNames
			=> [
				  "Abraham",
				  "George",
				  "James",
				  "Joel",
				  "John",
				  "Peter",
				  "Samuel",
				  "Zachariah"
				];

		public static IEnumerable<string> AvailableSecondaries
			=> [
				"Abraham1",
				"George1",
				"James1",
				"Joel1",
				"John1",
				"Peter1",
				"Samuel1",
				"Zachariah1",
				"Abraham2",
				"George2",
				"James2",
				"Joel2",
				"John2",
				"Peter2",
				"Samuel2",
				"Zachariah2"
			];

		public string? Summary
		{
			get => summary;
			private set
			{
				summary = value;
				OnPropertyChanged(nameof(Summary));
			}
		}

		public ICommand SelectAll
		{
			get
			{
				return new RelayCommand(
				  parameter =>
				  {
					  selectedNames.Clear();
					  foreach (string item in AvailableNames)
					  {
						  selectedNames.Add(item);
					  }
				  });
			}
		}

		public ObservableCollection<string> SelectedNames
			=> selectedNames;

		public ObservableCollection<string> SelectedSecondaries
			=> selectedSecondaries;

		public ObservableCollection<DateTime> SelectedDates
			=> selectedDates;

		public DateTime StartDate
			=> selectedDates.Count == 0 ? DateTime.MaxValue : SelectedDates.Min();

		public DateTime EndDate
			=> selectedDates.Count == 0 ? DateTime.MaxValue : SelectedDates.Max();

		public ICommand NamesSelectionChangedCommand
		{
			get
			{
				return new RelayCommand(
				  parameter =>
				  {
					  System.Diagnostics.Debug.Print("{0} NamesSelectionChangedCommand", DateTime.Now);
					  CommonNamesSelectionAction(parameter);
				  });
			}
		}

		public ICommand NamesViewSelectionChangedCommand
		{
			get
			{
				return new RelayCommand(
				  parameter =>
				  {
					  System.Diagnostics.Debug.Print("{0} NamesViewSelectionChangedCommand", DateTime.Now);
					  CommonNamesSelectionAction(parameter);
				  });
			}
		}

		private void CommonNamesSelectionAction(object parameter)
		{
			lock (selectingMapSynchLock)
			{
				try
				{
					IList items = (IList)parameter;
					IEnumerable<string>? currentSelectedItems = items.Cast<string>();

					if (currentSelectedItems == null)
					{
						return;
					}

					UpdateSummary(currentSelectedItems);
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		public ICommand SecondariesSelectionChangedCommand
		{
			get
			{
				return new RelayCommand(
				  parameter =>
				  {
					  lock (selectingMapSynchLock)
					  {
						  try
						  {
							  IList items = (IList)parameter;
							  IEnumerable<string>? currentSecondariesSelected = items.Cast<string>();

							  if (selectingMap == 0)
							  {
								  Interlocked.Increment(ref selectingMap);
								  if (currentSecondariesSelected.Count() == 1)
								  {
									  SelectMap(true, AvailableNames.First(o => currentSecondariesSelected.Single().Contains(o)));
								  }
								  else if (currentSecondariesSelected.Count() == 0)
								  {
									  SelectedNames.Clear();
								  }
								  else
								  {
									  //If this is uncommented then the ability multi-select Secondaries is removed
									  //this.SelectedNames.Clear();
								  }
								  Interlocked.Decrement(ref selectingMap);
							  }
						  }
						  catch (Exception)
						  {
							  throw;
						  }
					  }
				  });
			}
		}

		private void SelectMap(bool isDriverSecondary, string nameToSelect)
		{
			lock (selectingMapSynchLock)
			{
				Interlocked.Increment(ref selectingMap);
				if (isDriverSecondary)
				{
					if (SelectedNames.Count != 1 || SelectedNames.Single() != nameToSelect)
					{
						SelectedNames.Clear();
						SelectedNames.Add(nameToSelect);
					}
				}
				else
				{
					List<string> secondariesToSelect = AvailableSecondaries.Where(o => o.Contains(nameToSelect)).ToList();
					if (SelectedSecondaries.Count != secondariesToSelect.Count || SelectedSecondaries.Intersect(secondariesToSelect).Count() != secondariesToSelect.Count)
					{
						SelectedSecondaries.Clear();
						foreach (string? secondary in secondariesToSelect)
						{
							SelectedSecondaries.Add(secondary);
						}
					}
				}

				Interlocked.Decrement(ref selectingMap);
			}
		}

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void UpdateSummary(IEnumerable<string> selectedNames)
		{
			Summary = $"{selectedNames.Count()} names are selected.";
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler? PropertyChanged = null;

		#endregion
	}
}