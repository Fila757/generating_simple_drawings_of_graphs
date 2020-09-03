// drawing_of_cliques.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "functions.hpp"

using namespace std;

int main()
{
    auto g = graph(4);

    cout << "minimal: " << g.find_canonic_fingerprint("123032031021") << endl;
 
    //g.create_all_possible_drawings();
    //print_graph(&g);
}

