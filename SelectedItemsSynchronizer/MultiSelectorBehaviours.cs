namespace SelectedItemsSynchronizer
{
	using System;
	using System.Collections;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;

	/// <summary>
	/// A sync behaviour for a multiselector.
	/// </summary>
	public static class MultiSelectorBehaviours
	{
		/// <summary>
		/// The synchronized selected items.
		/// </summary>
		public static readonly DependencyProperty SynchronizedSelectedItems = DependencyProperty.RegisterAttached(
		  "SynchronizedSelectedItems", typeof(IList), typeof(MultiSelectorBehaviours), new PropertyMetadata(null, OnSynchronizedSelectedItemsChanged));

		private static readonly DependencyProperty SynchronizationManagerProperty = DependencyProperty.RegisterAttached(
		  "SynchronizationManager", typeof(ISynchronizationManager), typeof(MultiSelectorBehaviours), new PropertyMetadata(null));

		/// <summary>
		/// Gets the synchronized selected items.
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <returns>The list that is acting as the sync list.</returns>
		public static IList GetSynchronizedSelectedItems(DependencyObject dependencyObject)
		{
			return (IList)dependencyObject.GetValue(SynchronizedSelectedItems);
		}

		/// <summary>
		/// Sets the synchronized selected items.
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <param name="value">The value to be set as synchronized items.</param>
		public static void SetSynchronizedSelectedItems(DependencyObject dependencyObject, IList value)
		{
			dependencyObject.SetValue(SynchronizedSelectedItems, value);
		}

		private static ISynchronizationManager GetSynchronizationManager(DependencyObject dependencyObject)
		{
			return (ISynchronizationManager)dependencyObject.GetValue(SynchronizationManagerProperty);
		}

		private static void SetSynchronizationManager(DependencyObject dependencyObject, ISynchronizationManager? value)
		{
			dependencyObject.SetValue(SynchronizationManagerProperty, value);
		}

		private static void OnSynchronizedSelectedItemsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null)
			{
				ISynchronizationManager synchronizer = GetSynchronizationManager(dependencyObject);
				synchronizer.StopSynchronizing();

				SetSynchronizationManager(dependencyObject, null);
			}

			if (dependencyObject is Calendar calendar)
			{
				ISynchronizationManager synchronizer = GetSynchronizationManager(dependencyObject);
				if (synchronizer == null)
				{
					synchronizer = new CalendarSynchronizationManager(calendar);
					SetSynchronizationManager(dependencyObject, synchronizer);
				}

				synchronizer.StartSynchronizing();
			}
			else
			{
				// check that this property is an IList, and that it is being set on a ListBox
				if (e.NewValue is IList
					&& dependencyObject is Selector selector)
				{
					ISynchronizationManager synchronizer = GetSynchronizationManager(dependencyObject);
					if (synchronizer == null)
					{
						synchronizer = new SelectorSynchronizationManager(selector);
						SetSynchronizationManager(dependencyObject, synchronizer);
					}

					synchronizer.StartSynchronizing();
				}
			}
		}

		private class CalendarSynchronizationManager : BaseSynchronizationManager<Calendar>, ISynchronizationManager
		{
			/// <summary>
			/// Initialises a new instance of the <see cref="CalendarSynchronizationManager"/> class.
			/// </summary>
			/// <param name="calendar">The calendar to sync.</param>
			internal CalendarSynchronizationManager(Calendar calendar)
			  : base(calendar)
			{
			}

			protected override IList GetSelectedItemsCollection(Calendar calendar)
			{
				return calendar.SelectedDates;
			}
		}

		private abstract class BaseSynchronizationManager<T>(T source) : ISynchronizationManager
		  where T : DependencyObject
		{
			private readonly T source = source;
			private TwoListSynchronizer? synchronizer = null;

			/// <summary>
			/// Starts synchronizing the list.
			/// </summary>
			public void StartSynchronizing()
			{
				IList list = GetSynchronizedSelectedItems(source);

				if (list != null)
				{
					synchronizer = new TwoListSynchronizer(GetSelectedItemsCollection(source), list);
					synchronizer.StartSynchronizing();
				}
			}

			/// <summary>
			/// Stops synchronizing the list.
			/// </summary>
			public void StopSynchronizing()
			{
				synchronizer?.StopSynchronizing();
			}

			protected abstract IList GetSelectedItemsCollection(T source);
		}

		/// <summary>
		/// A synchronization manager.
		/// </summary>
		private class SelectorSynchronizationManager : BaseSynchronizationManager<Selector>
		{
			/// <summary>
			/// Initialises a new instance of the <see cref="SelectorSynchronizationManager"/> class.
			/// </summary>
			/// <param name="selector">The selector.</param>
			internal SelectorSynchronizationManager(Selector selector)
			  : base(selector)
			{
			}

			protected override IList GetSelectedItemsCollection(Selector selector)
			{
				return selector switch
				{
					MultiSelector ms	=> ms.SelectedItems,
					ListBox lb			=> lb.SelectedItems,
					_					=> throw new InvalidOperationException("Target object has no SelectedItems property to bind.")
				};
			}
		}
	}
}