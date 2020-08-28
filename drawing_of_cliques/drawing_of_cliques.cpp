// drawing_of_cliques.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "functions.hpp"

using namespace std;

int main()
{
   
    auto generator_of_fingerprints = fingerprints(4);

    while (!generator_of_fingerprints.done) {
        cout << generator_of_fingerprints.get_next() << endl;
    }
    
    //print_graph(&g);
}

