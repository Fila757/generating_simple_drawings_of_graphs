// drawing_of_cliques.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "functions.hpp"
#include <thread>

using namespace std;

int main()
{
    int n = 8;

    cout << "number of thrds " << std::thread::hardware_concurrency() << endl;

    int number_of_threads = std::thread::hardware_concurrency();

    vector<graph> graphs;
     
    system(("split -d --additional-suffix=.txt -l" +to_string( 5370725 / (number_of_threads - 1) + 1) +  " data/graph" + to_string(n) + ".txt data/graph" + to_string(n) + "_").data());
    
    for(int i = 0; i < number_of_threads - 1;i++){
    	graphs.push_back(graph(n, i));
    }
    cout << "graphs created" << endl;
    
    //auto g = graph(n, 1);
    //g.create_all_possible_drawings();
    vector<thread> threads;
    for(int i = 0; i < number_of_threads - 1;i++){
    	threads.push_back(thread(&graph::create_all_possible_drawings, &graphs[i]));
    }
    
    cout << "after run" << endl;
    for(int i = 0; i < number_of_threads - 1;i++)
       threads[i].join();
    
    system(("cat ../VizualizerWPF/data/graph" + to_string(n) + "_* > ../VizualizerWPF/data/graph" + to_string(n) + ".txt").data());
                                                                         
    system("rm data/*_*");
    system("rm ../VizualizerWPF/data/*_*");
     
}

