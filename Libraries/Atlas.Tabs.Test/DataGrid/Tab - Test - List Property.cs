﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestObjectProperties : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private PropertyTest propertyTest = new PropertyTest();

			public override void Load(Call call, TabModel model)
			{
				model.Items = ListProperty.Create(propertyTest);
				model.Editing = true;

				model.Actions =  new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Toggle", Toggle),
				};
			}

			private void Toggle(Call call)
			{
				propertyTest.Boolean = !propertyTest.Boolean;
			}
		}
	}

	public class PropertyTest : INotifyPropertyChanged
	{
		private bool _Boolean;
		[Editing]
		public bool Boolean
		{
			get
			{
				return _Boolean;
			}
			set
			{
				_Boolean = value;
				NotifyPropertyChangedContext(nameof(Boolean));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		
		private void NotifyPropertyChangedContext(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public List<int> list { get; set; } = new List<int>() { 1, 2, 3 };
	}
}
