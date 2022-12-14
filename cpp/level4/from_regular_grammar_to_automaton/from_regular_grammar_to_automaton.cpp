#include <iostream>
#include <fstream>

#include "Inutils.cpp"
#include "includes/Grammar.h"
#include "includes/FiniteAutomaton.h"
#include "includes/GrammarToAutomaton.h"

Grammar CreateGrammar(){
	Grammar grammar;

	std::ifstream file1("text files/inputGrammar.txt");
	if(not file1.is_open()){
		std::cout << "the file wasnt opened correctly. \n";
		std::cout << "either choose to insert the data by hand or exit: \n";
		std::cout << "0 - insert data from keyboard \n1 - exit the program \n";
		char insertOrExit;
		std::cin >> insertOrExit;
		if(insertOrExit == '0')
			grammar.readGrammar();
		// else continue, close the file and skip the menu
	}
	else
		file1 >> grammar;
	file1.close();

	return grammar;
}

void Menu(Grammar& grammar){
	std::cout << "~~~~~ welcome ~~~~~\n\n";
	FiniteAutomaton finaut(GrammarToFiniteAutomaton(grammar));
	bool exit = false;
	while(true){
		printThreeDots();
		std::cout << "\n\n";
		printMainMenu();
		char letter;
		std::cin >> letter;
		std::cin.ignore(256, '\n');
		std::cout << "\n";
		switch(letter){
			case '1':{
				grammar.printGrammar();
				break;
			}

			case '2':{
				int n;
				std::cout << "insert how many words you want to be generated: ";
				std::cin >> n;
				std::cout << "the words generated are as follows: \n\n";
				for(int i = 0; i < n; i++){
					std::string generatedWord = grammar.generateWord();
					std::cout << "the generated word " << i + 1 << " is: " << generatedWord << "\n\n";
				}
				break;
			}

			case '3':{
				std::cout << finaut << std::endl;
				break;
			}

			case '4':{
				std::string word;
				std::cout << "enter the word \n";
				std::cin >> word;
				if(finaut.CheckWord(word))
					std::cout << "the word \"" << word << "\" is accepted by the automaton. \n";
				else
					std::cout << "unfortunately the word \"" << word << "\" is not accepted by the automaton.. \n";
				break;
			}

			case '5':{
				std::string generatedWord = grammar.generateWord();
				if(finaut.CheckWord(generatedWord))
					std::cout << "the generated word \"" << generatedWord << "\" is accepted by the automaton. \n";
				else
					std::cout << "unfortunately the generated word \"" << generatedWord << "\" is not accepted by the automaton.. \n";
				break;
			}

			default:{
				exit = true;
				break;
			}
		}
		if(exit == true)
			break;
	}
	std::cout << "exiting the menu";
	printThreeDots();
}

int main(){
	Grammar grammar = CreateGrammar();

	if(grammar.verifyGrammar() == true and grammar.isRegular() == true)
		Menu(grammar);
}
