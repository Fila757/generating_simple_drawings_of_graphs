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

    //g.add_edge(g.segments[4]->from_, g.segments[7]->from_, g.outer_face);
    g.add_vertex(g.segments[g.edges.size() - 6]);
    g.add_vertex(g.segments[g.edges.size() - 1]);
    g.delete_vertex(g.edges.back().from_.get());
    print_graph(&g);
}

