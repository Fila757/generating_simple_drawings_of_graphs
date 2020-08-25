// drawing_of_cliques.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "functions.hpp"

using namespace std;

int main()
{
    auto g = graph(4);

    //g.create_special_vertex(make_pair(0, 0), 0);

    print_graph(&g);
    g.create_all_special_vertices();
        //print_graph(&g);
    print_graph(&g);

    g.recolor_fingerprint("123023013012");
    print_graph(&g);

    g.add_edge(g.segments[g.starts[0][1]]->from_, g.segments[g.starts[1][0]]->from_, g.outer_face, true);
    print_graph(&g);
}

