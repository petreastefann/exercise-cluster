﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MemoryTilesGame {
	class User {
		private string name;
		private string imagePath;
		private int matchesPlayed;
		private int matchesWon;

		public User() {
			Random random = new Random();
			name = "user" + random.Next(1000, 10000).ToString();
			imagePath = "../../Assets/greysquare.png";
			// ik these initializations^ are useless but eh
			matchesPlayed = 0;
			matchesWon = 0;
		}

		public string Name {
			get; set;
		}

		public string ImagePath {
			get; set;
		}

		public int MatchesPlayed {
			get; set;
		}

		public int MatchesWon {
			get; set;
		}
	}

	public partial class ChooseUserWindow : Window {
		string usersJsonFilePath;
		JArray usersArray;
		int currentUserIndex;

		public ChooseUserWindow() {
			InitializeComponent();

			initializeUsersJsonFile();

			initializeUsersArray();
		}

		private void nextWindow_Click(object sender, RoutedEventArgs e) {
			if(userImage.Source != null) {
				Close();
				StartGameWindow startGameWindow = new StartGameWindow(usersArray[currentUserIndex]);    // jtoken argument
				startGameWindow.Show();
			}
			else {
				MessageBox.Show("no user has been assigned yet.");
			}
		}

		private void exitApp(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}

		private void back_Click(object sender, RoutedEventArgs e) {
			Close();
			MainWindow mainWindow = new MainWindow();
			mainWindow.Show();
		}

		private void nextUser_Click(object sender, RoutedEventArgs e) {
			currentUserIndex++;
			if(currentUserIndex >= usersArray.Count)
				currentUserIndex = 0;

			updateCurrentUserOnScreen();
		}

		private void prevUser_Click(object sender, RoutedEventArgs e) {
			currentUserIndex--;
			if(currentUserIndex < 0)
				currentUserIndex = usersArray.Count - 1;

			updateCurrentUserOnScreen();
		}

		private void newUser_Click(object sender, RoutedEventArgs e) {
			User newUser = new User();

			CreateNewUserWindow createNewUserWindow = new CreateNewUserWindow();
			createNewUserWindow.ShowDialog();

			newUser.Name = createNewUserWindow.NewUserName;
			newUser.ImagePath = createNewUserWindow.NewUserImagePath;

			if(newUser.Name != "" && newUser.ImagePath != "") {
				usersArray.Add(JObject.FromObject(newUser));

				updateUsersJsonFileFromUsersArray(usersArray);
			}
		}

		private void deleteUser_Click(object sender, RoutedEventArgs e) {
			if(usersArray.Count == 0 || userImage.Source == null) {
				MessageBox.Show("you cannot delete a non-existent user.");
				return;
			}

			var indexOfUserToDelete = currentUserIndex;
			if(usersArray.Count == 1) {
				userImage.Source = null;
				userName.Text = "";
			}
			else {
				nextUser_Click(sender, e);
			}
			usersArray.RemoveAt(indexOfUserToDelete);

			updateUsersJsonFileFromUsersArray(usersArray);

			MessageBox.Show("user deleted succesfully");
		}

		private void updateCurrentUserOnScreen() {
			try {
				JToken currentUser = usersArray[currentUserIndex];
				// check for null values that get parsed to the json file (if they might) and if yes add a catch for null arguments or smth
				userImage.Source = new BitmapImage(new Uri(currentUser.Value<string>("ImagePath")));
				userName.Text = currentUser.Value<string>("Name");
			}
			catch(System.ArgumentOutOfRangeException) {
				return;
			}
			catch(System.ArgumentNullException) {
				eliminateCurrentNullUser();
			}
		}

		private void initializeUsersJsonFile() {
			usersJsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "users.json");

			if(!File.Exists(usersJsonFilePath)) {
				// create json file with an empty array
				updateUsersJsonFileFromUsersArray(new JArray());
			}
		}

		private void initializeUsersArray() {
			usersArray = JArray.Parse(File.ReadAllText(usersJsonFilePath));
			currentUserIndex = -1;
		}

		private void updateUsersJsonFileFromUsersArray(JArray usersArray) {
			string json = JsonConvert.SerializeObject(usersArray, Formatting.Indented);
			File.WriteAllText(usersJsonFilePath, json);
		}

		// i dont know why but sometimes a null user gets added. i have to find out where and why it happens.
		private void eliminateCurrentNullUser() {
			var indexOfUserToDelete = currentUserIndex;
			if(usersArray.Count == 1) {
				userImage.Source = null;
				userName.Text = "";
			}
			else {
				currentUserIndex++;
				if(currentUserIndex >= usersArray.Count)
					currentUserIndex = 0;

				updateCurrentUserOnScreen();
			}
			usersArray.RemoveAt(indexOfUserToDelete);

			updateUsersJsonFileFromUsersArray(usersArray);
		}
	}
}
