// drawing_of_cliques.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "functions.hpp"

using namespace std;

int main()
{
    auto g = graph(4);
    print_graph(&g);
    g.create_all_special_vertices();
        //print_graph(&g);
    print_graph(&g);


    //g.add_vertex()

    //print_graph(&g);

    /*
    auto outer_face = make_shared<Face>(nullptr);
    g.add_edge(make_shared<Vertex>(nullptr, g.vertices[0].x, g.vertices[0].y), make_shared<Vertex>(nullptr, g.vertices[1].x, g.vertices[1].y), outer_face);
    print_graph(&g);
    */
}

