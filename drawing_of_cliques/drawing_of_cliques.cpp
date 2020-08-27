// drawing_of_cliques.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "functions.hpp"

using namespace std;

int main()
{
    auto g = graph(4);

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();

    print_graph(&g);

    g.add_edge(g.segments[4]->from_, g.segments[7]->from_, g.outer_face);

    print_graph(&g);

    g.delete_edge_back();

    print_graph(&g);

    cout << g.outer_face << endl;
}

