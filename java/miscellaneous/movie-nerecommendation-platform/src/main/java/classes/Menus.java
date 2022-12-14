package classes;

import classes.auxiliaryclasses.MenusFuncs;
import classes.auxiliaryclasses.PrintMenus;

import classes.auxiliaryclasses.InsertIntoTables;
import classes.auxiliaryclasses.Inutils;
import java.sql.Connection;
import java.sql.SQLException;
import java.util.ArrayList;
import org.apache.commons.lang3.mutable.MutableBoolean;

public class Menus extends MenusFuncs {

	public static void userMenu(Connection connection, int user_id) throws SQLException {
		boolean exit = false;
		short option;

		PrintMenus.printUserMenuMenu();
		do {
			System.out.print(">");
			option = Inutils.getSmallIntInputFromUser();

			ArrayList<Genres> genres = insertFromDBinGenreArray(connection, user_id);

			if (genres.size() == 0) {
				System.out
						.println("the database is empty. please contact the admin to add new movies to the platform..");
				option = -1;
			}

			switch (option) {
				case 0: {
					PrintMenus.printUserMenuMenu();
					break;
				}

				case 1: {
					MutableBoolean exists = new MutableBoolean(false);
					Object[] movieCredentials = findMovie(connection, genres, exists);

					if (exists.booleanValue() == true) {
						System.out.println(movieCredentials[1] + " | " + movieCredentials[2] + " | "
								+ movieCredentials[3] + " | " + movieCredentials[4]);

						int movie_id = (int) movieCredentials[0];
						promptUserForLikeAndDislike(connection, user_id, movie_id);
					} else {
						System.out.println("the movie could not be found..\n");
					}
					break;
				}

				case 2: {
					PrintMenus.printUserMenuMenu2();
					short subOption = Inutils.getSmallIntInputFromUser();

					if (subOption == 1 || subOption == 2) {

						if (subOption == 1) {
							sortMoviesByLikes(genres);
						} else {
							sortMoviesByReleaseDate(genres);
						}

						printFeed(connection, genres);
					} else {
						System.out.println("unfortunately that is not an available option..");
					}
					break;
				}

				case 3: {
					printUserLikedMoviesList(connection, user_id);
					break;
				}

				default: {
					exit = true;
					break;
				}
			}
		} while (exit == false);

		System.out.print("logging out");
		Inutils.printLoadingDots();
	}

	public static void adminMenu(Connection connection) {
		boolean exit = false;
		short option;

		PrintMenus.printAdminMenuMenu();
		do {
			System.out.print(">");
			option = Inutils.getSmallIntInputFromUser();
			switch (option) {
				case 0: {
					PrintMenus.printAdminMenuMenu();
					break;
				}

				case 1: {
					PrintMenus.printAdminMenuMenu2();
					short subOption = Inutils.getSmallIntInputFromUser();
					if (subOption == 1) {
						InsertIntoTables.insertIntoTableMoviesByFile(connection);
					} else if (subOption == 2) {
						InsertIntoTables.insertIntoTableMoviesByConsole(connection);
					} else {
						System.out.println("unfortunately that is not an available option..");
					}

					break;
				}

				// if i were to make a function to delete a movie from the database i would also have to meddle with the users_movies table hmmmm..

				default: {
					exit = true;
					break;
				}
			}

		} while (exit == false);

		System.out.println("logging out..");
	}
}
