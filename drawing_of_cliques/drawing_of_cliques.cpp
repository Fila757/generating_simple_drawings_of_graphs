// drawing_of_cliques.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "functions.hpp"

using namespace std;

/// <summary>
/// The checker is design for Linux.
/// </summary>
/// <returns></returns>
int main()
{
    int n = 8;

    cout << "number of thrds " << std::thread::hardware_concurrency() << endl;

    int number_of_threads = std::thread::hardware_concurrency();

    vector<graph> graphs;
    shared_ptr<canonic_wraper> shared_wraper = make_shared<canonic_wraper>();

    system(("python3 select_fingerprints.py data/graph" + to_string(n-1) + ".txt").data());
 
    system(("split -d --additional-suffix=.txt -l" +to_string( 11556 / (number_of_threads - 1) + 1) +  " data/graph" + to_string(n-1) + ".txt data/graph" + to_string(n-1) + "_").data());

    for(int i = 0; i < number_of_threads - 1;i++){
        graphs.push_back(graph(n, i, shared_wraper));
    }

    vector<thread> threads;

    for(int i = 0; i < number_of_threads - 1;i++){
        threads.push_back(thread(&graph::create_all_possible_drawings, &graphs[i]));
    }


    for(int i = 0; i < number_of_threads - 1;i++)
        threads[i].join();

    system(("cat data/graph" + to_string(n) + "_* > data/graph" + to_string(n) + ".txt").data());
    system("rm data/*_*");

    system(("python3 sort_lines.py data/graph" + to_string(n) + ".txt").data());


}

