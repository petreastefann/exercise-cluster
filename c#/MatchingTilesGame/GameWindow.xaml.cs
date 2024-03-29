﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MatchingTilesGame {
	public partial class GameWindow : Window {
		DispatcherTimer _timer;
		TimeSpan _time;
		private int button1ListIndex;
		private int button1ImageId;
		private Button button1;
		List<KeyValuePair<int, ImageBrush>> imagesList; // param1 - id, param2 - image
		int nbOfTilesLeftToFlip;

		public bool RoundWon {
			get; set;
		}

		public GameWindow(JToken user, int cols, int rows) {
			InitializeComponent();

			userImage.Source = new BitmapImage(new Uri(user.Value<string>("ImagePath")));
			userName.Text = user.Value<string>("Name");

			initializeMembers(cols, rows);

			createGrid(cols, rows);

			addImagesToGrid(cols, rows);

			placeTimer(cols, rows);
		}

		private void createGrid(int cols, int rows) {
			for(int i = 0; i < rows; i++) {
				RowDefinition rowDef = new RowDefinition();
				GameGrid.RowDefinitions.Add(rowDef);
			}

			for(int i = 0; i < cols; i++) {
				ColumnDefinition colDef = new ColumnDefinition();
				GameGrid.ColumnDefinitions.Add(colDef);
			}

			// create the buttons and add them to the grid
			for(int i = 0; i < rows; i++) {
				for(int j = 0; j < cols; j++) {
					Button button = new Button();
					button.MinHeight = 50;
					button.MinWidth = 50;
					button.Margin = new Thickness(2);

					button.Click += Button_Click; // add event handler to each button

					Grid.SetRow(button, i);
					Grid.SetColumn(button, j);
					GameGrid.Children.Add(button);
				}
			}

			GameGrid.VerticalAlignment = VerticalAlignment.Top;
			GameGrid.HorizontalAlignment = HorizontalAlignment.Left;
			GameGrid.Margin = new Thickness(0, 40, 0, 50);  // the distance between the top and the bottom part of the window

			// adding a SizeChanged event handler to the window for resizing the buttons based on the window dimensions
			this.SizeChanged += (sender, e) => {
				// calculate the new button size based on the window size
				double size = Math.Min((this.ActualWidth - GameGrid.Margin.Right) / cols, this.ActualHeight / rows);

				// update the button sizes
				foreach(var child in GameGrid.Children) {
					if(child is Button button) {
						button.Width = size;
						button.Height = size;
					}
				}
			};
		}

		private void addImagesToGrid(int cols, int rows) {
			// specify the folder path where your images are stored
			string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images");

			// get the file names of all the images in the folder
			string[] paths = Directory.GetFiles(folderPath, "*.png");

			// place the images in an intermediary list to shuffle them for random selection
			List<string> intermediaryList = new List<string>(paths);
			Random random = new Random();
			intermediaryList.Sort((a, b) => random.Next(2) == 0 ? -1 : 1);


			// put images in a list
			for(int i = 0; i < cols * rows / 2; i++) {
				imagesList.Add(new KeyValuePair<int, ImageBrush>(i, new ImageBrush { ImageSource = new BitmapImage(new Uri(intermediaryList[i], UriKind.Relative)) }));
				imagesList.Add(new KeyValuePair<int, ImageBrush>(i, new ImageBrush { ImageSource = new BitmapImage(new Uri(intermediaryList[i], UriKind.Relative)) }));
			}

			imagesList.Sort((a, b) => random.Next(2) == 0 ? -1 : 1);

			// Loop through each button and assign an image and a tag to it (the tag represents the index in the list)
			int imagesListId = 0;
			foreach(var child in GameGrid.Children) {
				if(child is Button button) {
					button.Tag = imagesListId++;
					button.Background = new ImageBrush(new BitmapImage(new Uri("Assets/greysquare.png", UriKind.Relative)));
				}
			}
		}

		private void placeTimer(int cols, int rows) {
			if(cols * rows == 4) {
				_time = TimeSpan.FromSeconds(3);
			}
			else if(cols * rows == 6) {
				_time = TimeSpan.FromSeconds(6);
			}
			else {
				_time = TimeSpan.FromSeconds((int)(10 * Math.Pow((Math.Sqrt((cols * rows) / 7)), 3) + 5));
			}
			tbTime.Margin = new Thickness(0, 0, GameGrid.ActualWidth - tbTime.ActualWidth, 0);

			_timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate {
				tbTime.Text = _time.ToString("c");

				if(_time == TimeSpan.Zero) {
					_timer.Stop();
					IsHitTestVisible = false;   // made sure that if messagebox doesnt work i still shouldnt be able to click

					youLostMessage.Text = "damn you suck";
					MessageBox.Show("the time is up. you lost..");

					Close();
				}

				_time = _time.Add(TimeSpan.FromSeconds(-1));
			}, System.Windows.Application.Current.Dispatcher);

			_timer.Start();
		}

		private async void Button_Click(object sender, RoutedEventArgs e) {
			// disable the mouse for the entire grid
			IsHitTestVisible = false;

			// first flipped image
			if(button1ListIndex == -1) {
				button1ListIndex = (int)((Button)sender).Tag;
				button1ImageId = imagesList[button1ListIndex].Key;
				button1 = (Button)sender;
				button1.Background = imagesList[button1ListIndex].Value;

				await Task.Delay(700);
			}
			// second flipped image
			else {
				((Button)sender).Background = imagesList[(int)((Button)sender).Tag].Value;
				await Task.Delay(700);

				// if they have the same imageid but different button
				if(imagesList[(int)((Button)sender).Tag].Key == button1ImageId && ((Button)sender) != button1) {
					nbOfTilesLeftToFlip -= 2;

					((Button)sender).Background = imagesList[(int)((Button)sender).Tag].Value;

					((Button)sender).IsEnabled = button1.IsEnabled = false;
				}
				else {
					((Button)sender).Background = button1.Background = new ImageBrush(new BitmapImage(new Uri("Assets/greysquare.png", UriKind.Relative)));
				}

				button1 = null;
				button1ImageId = -1;
				button1ListIndex = -1;

				if(nbOfTilesLeftToFlip == 0) {
					_timer.Stop();
					RoundWon = true;
					MessageBox.Show("nice");
					Close();
				}
			}

			// reenable the mouse for the entire grid
			IsHitTestVisible = true;
		}

		private void initializeMembers(int cols, int rows) {
			RoundWon = false;
			button1ListIndex = -1;
			button1ImageId = -1;
			button1 = null;
			imagesList = new List<KeyValuePair<int, ImageBrush>>();
			nbOfTilesLeftToFlip = cols * rows;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			_timer.Stop();

			MessageBox.Show("youre about to exit the window.");
		}
	}
}
