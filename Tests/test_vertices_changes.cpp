#include "pch.h"
#include "../drawing_of_cliques/functions.hpp"
#include "../packages/Microsoft.googletest.v140.windesktop.msvcstl.static.rt-dyn.1.8.1.3/build/native/include/gtest/gtest.h"

TEST(graphs_add_vertex, graph_4){
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();

    //g.add_edge(g.segments[4]->vertices_[0], g.segments[7]->vertices_[0], g.outer_face);
    g.add_vertex(g.segments[g.edges.size() - 6]);

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2 * (n - 1) + 2);

    //faces
    for (int i = 0; i < g.edges.size();i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back(), g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->vertices_[0]);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back()->index_, i);

            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }

    /*the new vertex is really new*/
    for (int i = 0; i < n * (n - 1);i++) {
        EXPECT_NE(g.segments[i]->vertices_[0], g.segments[g.edges.size() - 1]->vertices_[0]);
    }

    EXPECT_EQ(g.segments[g.edges.size() - 7]->vertices_.back(), g.segments[g.edges.size() - 8]->vertices_.back());
    EXPECT_EQ(g.segments[g.edges.size() - 7]->vertices_[0], g.segments[3]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->vertices_[0], g.segments[0]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_.back(), g.segments[1]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_[0], g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_.back(), g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_[0], g.segments[1]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_.back(), g.segments[2]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_[0], g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_.back(), g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_[0], g.segments[2]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_.back(), g.segments[0]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_[0], g.segments[g.edges.size() - 2]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->vertices_.back(), g.segments[3]->vertices_[0]);

    //edges 

    EXPECT_EQ(g.segments[0]->prev_->index_, 100); EXPECT_EQ(g.segments[0]->next_->index_, 002);
    EXPECT_EQ(g.segments[1]->prev_->index_, 200); EXPECT_EQ(g.segments[1]->next_->index_, 003);
    EXPECT_EQ(g.segments[2]->prev_->index_, 300); EXPECT_EQ(g.segments[2]->next_->index_, 001);
    EXPECT_EQ(g.segments[3]->prev_->index_, 001); EXPECT_EQ(g.segments[3]->next_->index_, 101);
    EXPECT_EQ(g.segments[4]->prev_->index_, 101); EXPECT_EQ(g.segments[4]->next_->index_, 101);
    EXPECT_EQ(g.segments[5]->prev_->index_, 101); EXPECT_EQ(g.segments[5]->next_->index_, 100);
    EXPECT_EQ(g.segments[6]->prev_->index_, 002); EXPECT_EQ(g.segments[6]->next_->index_, 202);
    EXPECT_EQ(g.segments[7]->prev_->index_, 202); EXPECT_EQ(g.segments[7]->next_->index_, 202);
    EXPECT_EQ(g.segments[8]->prev_->index_, 202); EXPECT_EQ(g.segments[8]->next_->index_, 200);
    EXPECT_EQ(g.segments[9]->prev_->index_, 003); EXPECT_EQ(g.segments[9]->next_->index_, 303);
    EXPECT_EQ(g.segments[10]->prev_->index_, 303); EXPECT_EQ(g.segments[10]->next_->index_, 303);
    EXPECT_EQ(g.segments[11]->prev_->index_, 303); EXPECT_EQ(g.segments[11]->next_->index_, 300);
    EXPECT_EQ(g.segments[12]->prev_->index_, 000); EXPECT_EQ(g.segments[12]->next_->index_, 001);
    EXPECT_EQ(g.segments[13]->prev_->index_, 101); EXPECT_EQ(g.segments[13]->next_->index_, 100);
    EXPECT_EQ(g.segments[14]->prev_->index_, 000); EXPECT_EQ(g.segments[14]->next_->index_, 202);
    EXPECT_EQ(g.segments[15]->prev_->index_, 202); EXPECT_EQ(g.segments[15]->next_->index_, 000);
    EXPECT_EQ(g.segments[16]->prev_->index_, 000); EXPECT_EQ(g.segments[16]->next_->index_, 303);
    EXPECT_EQ(g.segments[17]->prev_->index_, 303); EXPECT_EQ(g.segments[17]->next_->index_, 000);
    EXPECT_EQ(g.segments[18]->prev_->index_, 001); EXPECT_EQ(g.segments[18]->next_->index_, 101);
    EXPECT_EQ(g.segments[19]->prev_->index_, 100); EXPECT_EQ(g.segments[19]->next_->index_, 000);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 100); EXPECT_EQ(g.segments[13]->opposite_->index_, 001);
    EXPECT_EQ(g.segments[14]->opposite_->index_, 200); EXPECT_EQ(g.segments[15]->opposite_->index_, 002);
    EXPECT_EQ(g.segments[16]->opposite_->index_, 300); EXPECT_EQ(g.segments[17]->opposite_->index_, 003);
    EXPECT_EQ(g.segments[18]->opposite_->index_, 100); EXPECT_EQ(g.segments[19]->opposite_->index_, 001);

}

TEST(graphs_delete_vertex, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();

    //g.add_edge(g.segments[4]->vertices_[0], g.segments[7]->vertices_[0], g.outer_face);
    g.add_vertex(g.segments[g.edges.size() - 6]);
    g.delete_vertex((g.segments[g.edges.size() - 1]->vertices_[0]).get());

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2 * (n - 1));

    //faces
    for (int i = 0; i < g.edges.size();i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back(), g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->vertices_[0]);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back()->index_, i);

            //opposite od edges
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }


    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_.back(), g.segments[0]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_[0], g.segments[3]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_.back(), g.segments[3]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_[0], g.segments[0]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_.back(), g.segments[1]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_[0], g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_.back(), g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_[0], g.segments[1]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_.back(), g.segments[2]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_[0], g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->vertices_.back(), g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->vertices_[0], g.segments[2]->vertices_[0]);

    //edges 

    EXPECT_EQ(g.segments[0]->prev_->index_, 100); EXPECT_EQ(g.segments[0]->next_->index_, 002);
    EXPECT_EQ(g.segments[1]->prev_->index_, 200); EXPECT_EQ(g.segments[1]->next_->index_, 003);
    EXPECT_EQ(g.segments[2]->prev_->index_, 300); EXPECT_EQ(g.segments[2]->next_->index_, 001);
    EXPECT_EQ(g.segments[3]->prev_->index_, 001); EXPECT_EQ(g.segments[3]->next_->index_, 101);
    EXPECT_EQ(g.segments[4]->prev_->index_, 101); EXPECT_EQ(g.segments[4]->next_->index_, 101);
    EXPECT_EQ(g.segments[5]->prev_->index_, 101); EXPECT_EQ(g.segments[5]->next_->index_, 100);
    EXPECT_EQ(g.segments[6]->prev_->index_, 002); EXPECT_EQ(g.segments[6]->next_->index_, 202);
    EXPECT_EQ(g.segments[7]->prev_->index_, 202); EXPECT_EQ(g.segments[7]->next_->index_, 202);
    EXPECT_EQ(g.segments[8]->prev_->index_, 202); EXPECT_EQ(g.segments[8]->next_->index_, 200);
    EXPECT_EQ(g.segments[9]->prev_->index_, 003); EXPECT_EQ(g.segments[9]->next_->index_, 303);
    EXPECT_EQ(g.segments[10]->prev_->index_, 303); EXPECT_EQ(g.segments[10]->next_->index_, 303);
    EXPECT_EQ(g.segments[11]->prev_->index_, 303); EXPECT_EQ(g.segments[11]->next_->index_, 300);
    EXPECT_EQ(g.segments[12]->prev_->index_, 000); EXPECT_EQ(g.segments[12]->next_->index_, 101);
    EXPECT_EQ(g.segments[13]->prev_->index_, 101); EXPECT_EQ(g.segments[13]->next_->index_, 000);
    EXPECT_EQ(g.segments[14]->prev_->index_, 000); EXPECT_EQ(g.segments[14]->next_->index_, 202);
    EXPECT_EQ(g.segments[15]->prev_->index_, 202); EXPECT_EQ(g.segments[15]->next_->index_, 000);
    EXPECT_EQ(g.segments[16]->prev_->index_, 000); EXPECT_EQ(g.segments[16]->next_->index_, 303);
    EXPECT_EQ(g.segments[17]->prev_->index_, 303); EXPECT_EQ(g.segments[17]->next_->index_, 000);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 100); EXPECT_EQ(g.segments[13]->opposite_->index_, 001);
    EXPECT_EQ(g.segments[14]->opposite_->index_, 200); EXPECT_EQ(g.segments[15]->opposite_->index_, 002);
    EXPECT_EQ(g.segments[16]->opposite_->index_, 300); EXPECT_EQ(g.segments[17]->opposite_->index_, 003);
}

TEST(graphs_add_2_vertices_on_same_line, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();

    //g.add_edge(g.segments[4]->vertices_[0], g.segments[7]->vertices_[0], g.outer_face);
    g.add_vertex(g.segments[g.edges.size() - 6]);
    g.add_vertex(g.segments[g.edges.size() - 2]);

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2 * (n - 1) + 4);

    //faces
    for (int i = 0; i < g.edges.size() ;i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back(), g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->vertices_[0]);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back()->index_, i);

            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }

    EXPECT_NE(g.segments[g.edges.size() - 3]->vertices_[0], g.segments[g.edges.size() - 1]->vertices_[0]);
    /*the new vertex is really new*/
    for (int i = 0; i < n * (n - 1);i++) {
        EXPECT_NE(g.segments[i]->vertices_[0], g.segments[g.edges.size() - 1]->vertices_[0]);
        EXPECT_NE(g.segments[i]->vertices_[0], g.segments[g.edges.size() - 3]->vertices_[0]);
    }

    EXPECT_EQ(g.segments[g.edges.size() - 9]->vertices_.back(), g.segments[g.edges.size() - 1]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 9]->vertices_[0], g.segments[3]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 10]->vertices_.back(), g.segments[g.edges.size() - 4]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 10]->vertices_[0], g.segments[0]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 7]->vertices_.back(), g.segments[1]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 7]->vertices_[0], g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->vertices_.back(), g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->vertices_[0], g.segments[1]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_.back(), g.segments[2]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_[0], g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_.back(), g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_[0], g.segments[2]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_.back(), g.segments[0]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_[0], g.segments[g.edges.size() - 4]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_.back(), g.segments[g.edges.size() - 2]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_.back(), g.segments[g.edges.size() - 10]->vertices_.back());
    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_[0], g.segments[g.edges.size() - 2]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->vertices_.back(), g.segments[3]->vertices_[0]);

    //edges 

    EXPECT_EQ(g.segments[0]->prev_->index_, 100); EXPECT_EQ(g.segments[0]->next_->index_, 002);
    EXPECT_EQ(g.segments[1]->prev_->index_, 200); EXPECT_EQ(g.segments[1]->next_->index_, 003);
    EXPECT_EQ(g.segments[2]->prev_->index_, 300); EXPECT_EQ(g.segments[2]->next_->index_, 001);
    EXPECT_EQ(g.segments[3]->prev_->index_, 001); EXPECT_EQ(g.segments[3]->next_->index_, 101);
    EXPECT_EQ(g.segments[4]->prev_->index_, 101); EXPECT_EQ(g.segments[4]->next_->index_, 101);
    EXPECT_EQ(g.segments[5]->prev_->index_, 101); EXPECT_EQ(g.segments[5]->next_->index_, 100);
    EXPECT_EQ(g.segments[6]->prev_->index_, 002); EXPECT_EQ(g.segments[6]->next_->index_, 202);
    EXPECT_EQ(g.segments[7]->prev_->index_, 202); EXPECT_EQ(g.segments[7]->next_->index_, 202);
    EXPECT_EQ(g.segments[8]->prev_->index_, 202); EXPECT_EQ(g.segments[8]->next_->index_, 200);
    EXPECT_EQ(g.segments[9]->prev_->index_, 003); EXPECT_EQ(g.segments[9]->next_->index_, 303);
    EXPECT_EQ(g.segments[10]->prev_->index_, 303); EXPECT_EQ(g.segments[10]->next_->index_, 303);
    EXPECT_EQ(g.segments[11]->prev_->index_, 303); EXPECT_EQ(g.segments[11]->next_->index_, 300);
    EXPECT_EQ(g.segments[12]->prev_->index_, 000); EXPECT_EQ(g.segments[12]->next_->index_, 001);
    EXPECT_EQ(g.segments[13]->prev_->index_, 101); EXPECT_EQ(g.segments[13]->next_->index_, 100);
    EXPECT_EQ(g.segments[14]->prev_->index_, 000); EXPECT_EQ(g.segments[14]->next_->index_, 202);
    EXPECT_EQ(g.segments[15]->prev_->index_, 202); EXPECT_EQ(g.segments[15]->next_->index_, 000);
    EXPECT_EQ(g.segments[16]->prev_->index_, 000); EXPECT_EQ(g.segments[16]->next_->index_, 303);
    EXPECT_EQ(g.segments[17]->prev_->index_, 303); EXPECT_EQ(g.segments[17]->next_->index_, 000);
    EXPECT_EQ(g.segments[18]->prev_->index_, 001); EXPECT_EQ(g.segments[18]->next_->index_, 001);
    EXPECT_EQ(g.segments[19]->prev_->index_, 100); EXPECT_EQ(g.segments[19]->next_->index_, 000);
    EXPECT_EQ(g.segments[20]->prev_->index_, 001); EXPECT_EQ(g.segments[20]->next_->index_, 101);
    EXPECT_EQ(g.segments[21]->prev_->index_, 100); EXPECT_EQ(g.segments[21]->next_->index_, 100);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 100); EXPECT_EQ(g.segments[13]->opposite_->index_, 001);
    EXPECT_EQ(g.segments[14]->opposite_->index_, 200); EXPECT_EQ(g.segments[15]->opposite_->index_, 002);
    EXPECT_EQ(g.segments[16]->opposite_->index_, 300); EXPECT_EQ(g.segments[17]->opposite_->index_, 003);
    EXPECT_EQ(g.segments[18]->opposite_->index_, 100); EXPECT_EQ(g.segments[19]->opposite_->index_, 001);
    EXPECT_EQ(g.segments[20]->opposite_->index_, 100); EXPECT_EQ(g.segments[21]->opposite_->index_, 001);

}

TEST(graphs_delete_2_vertices_vertex, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();

    //g.add_edge(g.segments[4]->vertices_[0], g.segments[7]->vertices_[0], g.outer_face);
    g.add_vertex(g.segments[g.edges.size() - 6]);
    g.add_vertex(g.segments[g.edges.size() - 1]);
    g.delete_vertex((g.segments[g.edges.size() - 1]->vertices_[0]).get());
    g.delete_vertex((g.segments[g.edges.size() - 1]->vertices_[0]).get());

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2 * (n - 1));

    //faces
    for (int i = 0; i < g.edges.size();i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back(), g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->vertices_[0]);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back()->index_, i);

            //opposite od edges
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }


    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_.back(), g.segments[0]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_[0], g.segments[3]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_.back(), g.segments[3]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_[0], g.segments[0]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_.back(), g.segments[1]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_[0], g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_.back(), g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_[0], g.segments[1]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_.back(), g.segments[2]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_[0], g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->vertices_.back(), g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->vertices_[0], g.segments[2]->vertices_[0]);

    //edges 

    EXPECT_EQ(g.segments[0]->prev_->index_, 100); EXPECT_EQ(g.segments[0]->next_->index_, 002);
    EXPECT_EQ(g.segments[1]->prev_->index_, 200); EXPECT_EQ(g.segments[1]->next_->index_, 003);
    EXPECT_EQ(g.segments[2]->prev_->index_, 300); EXPECT_EQ(g.segments[2]->next_->index_, 001);
    EXPECT_EQ(g.segments[3]->prev_->index_, 001); EXPECT_EQ(g.segments[3]->next_->index_, 101);
    EXPECT_EQ(g.segments[4]->prev_->index_, 101); EXPECT_EQ(g.segments[4]->next_->index_, 101);
    EXPECT_EQ(g.segments[5]->prev_->index_, 101); EXPECT_EQ(g.segments[5]->next_->index_, 100);
    EXPECT_EQ(g.segments[6]->prev_->index_, 002); EXPECT_EQ(g.segments[6]->next_->index_, 202);
    EXPECT_EQ(g.segments[7]->prev_->index_, 202); EXPECT_EQ(g.segments[7]->next_->index_, 202);
    EXPECT_EQ(g.segments[8]->prev_->index_, 202); EXPECT_EQ(g.segments[8]->next_->index_, 200);
    EXPECT_EQ(g.segments[9]->prev_->index_, 003); EXPECT_EQ(g.segments[9]->next_->index_, 303);
    EXPECT_EQ(g.segments[10]->prev_->index_, 303); EXPECT_EQ(g.segments[10]->next_->index_, 303);
    EXPECT_EQ(g.segments[11]->prev_->index_, 303); EXPECT_EQ(g.segments[11]->next_->index_, 300);
    EXPECT_EQ(g.segments[12]->prev_->index_, 000); EXPECT_EQ(g.segments[12]->next_->index_, 101);
    EXPECT_EQ(g.segments[13]->prev_->index_, 101); EXPECT_EQ(g.segments[13]->next_->index_, 000);
    EXPECT_EQ(g.segments[14]->prev_->index_, 000); EXPECT_EQ(g.segments[14]->next_->index_, 202);
    EXPECT_EQ(g.segments[15]->prev_->index_, 202); EXPECT_EQ(g.segments[15]->next_->index_, 000);
    EXPECT_EQ(g.segments[16]->prev_->index_, 000); EXPECT_EQ(g.segments[16]->next_->index_, 303);
    EXPECT_EQ(g.segments[17]->prev_->index_, 303); EXPECT_EQ(g.segments[17]->next_->index_, 000);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 100); EXPECT_EQ(g.segments[13]->opposite_->index_, 001);
    EXPECT_EQ(g.segments[14]->opposite_->index_, 200); EXPECT_EQ(g.segments[15]->opposite_->index_, 002);
    EXPECT_EQ(g.segments[16]->opposite_->index_, 300); EXPECT_EQ(g.segments[17]->opposite_->index_, 003);
}

TEST(graphs_add_2_vertices_and_delete_one, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();

    //g.add_edge(g.segments[4]->vertices_[0], g.segments[7]->vertices_[0], g.outer_face);
    g.add_vertex(g.segments[g.edges.size() - 6]);
    g.add_vertex(g.segments[g.edges.size() - 1]);
    g.delete_vertex(g.edges.back().vertices_[0].get());

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2 * (n - 1) + 2);

    //faces
    for (int i = 0; i < g.edges.size();i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back(), g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->vertices_[0]);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back()->index_, i);

            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }

    /*the new vertex is really new*/
    for (int i = 0; i < n * (n - 1);i++) {
        EXPECT_NE(g.segments[i]->vertices_[0], g.segments[g.edges.size() - 1]->vertices_[0]);
    }

    EXPECT_EQ(g.segments[g.edges.size() - 7]->vertices_.back(), g.segments[g.edges.size() - 8]->vertices_.back());
    EXPECT_EQ(g.segments[g.edges.size() - 7]->vertices_[0], g.segments[3]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->vertices_[0], g.segments[0]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_.back(), g.segments[1]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_[0], g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_.back(), g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_[0], g.segments[1]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_.back(), g.segments[2]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_[0], g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_.back(), g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_[0], g.segments[2]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_.back(), g.segments[0]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_[0], g.segments[g.edges.size() - 2]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->vertices_.back(), g.segments[3]->vertices_[0]);

    //edges 

    EXPECT_EQ(g.segments[0]->prev_->index_, 100); EXPECT_EQ(g.segments[0]->next_->index_, 002);
    EXPECT_EQ(g.segments[1]->prev_->index_, 200); EXPECT_EQ(g.segments[1]->next_->index_, 003);
    EXPECT_EQ(g.segments[2]->prev_->index_, 300); EXPECT_EQ(g.segments[2]->next_->index_, 001);
    EXPECT_EQ(g.segments[3]->prev_->index_, 001); EXPECT_EQ(g.segments[3]->next_->index_, 101);
    EXPECT_EQ(g.segments[4]->prev_->index_, 101); EXPECT_EQ(g.segments[4]->next_->index_, 101);
    EXPECT_EQ(g.segments[5]->prev_->index_, 101); EXPECT_EQ(g.segments[5]->next_->index_, 100);
    EXPECT_EQ(g.segments[6]->prev_->index_, 002); EXPECT_EQ(g.segments[6]->next_->index_, 202);
    EXPECT_EQ(g.segments[7]->prev_->index_, 202); EXPECT_EQ(g.segments[7]->next_->index_, 202);
    EXPECT_EQ(g.segments[8]->prev_->index_, 202); EXPECT_EQ(g.segments[8]->next_->index_, 200);
    EXPECT_EQ(g.segments[9]->prev_->index_, 003); EXPECT_EQ(g.segments[9]->next_->index_, 303);
    EXPECT_EQ(g.segments[10]->prev_->index_, 303); EXPECT_EQ(g.segments[10]->next_->index_, 303);
    EXPECT_EQ(g.segments[11]->prev_->index_, 303); EXPECT_EQ(g.segments[11]->next_->index_, 300);
    EXPECT_EQ(g.segments[12]->prev_->index_, 000); EXPECT_EQ(g.segments[12]->next_->index_, 001);
    EXPECT_EQ(g.segments[13]->prev_->index_, 101); EXPECT_EQ(g.segments[13]->next_->index_, 100);
    EXPECT_EQ(g.segments[14]->prev_->index_, 000); EXPECT_EQ(g.segments[14]->next_->index_, 202);
    EXPECT_EQ(g.segments[15]->prev_->index_, 202); EXPECT_EQ(g.segments[15]->next_->index_, 000);
    EXPECT_EQ(g.segments[16]->prev_->index_, 000); EXPECT_EQ(g.segments[16]->next_->index_, 303);
    EXPECT_EQ(g.segments[17]->prev_->index_, 303); EXPECT_EQ(g.segments[17]->next_->index_, 000);
    EXPECT_EQ(g.segments[18]->prev_->index_, 001); EXPECT_EQ(g.segments[18]->next_->index_, 101);
    EXPECT_EQ(g.segments[19]->prev_->index_, 100); EXPECT_EQ(g.segments[19]->next_->index_, 000);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 100); EXPECT_EQ(g.segments[13]->opposite_->index_, 001);
    EXPECT_EQ(g.segments[14]->opposite_->index_, 200); EXPECT_EQ(g.segments[15]->opposite_->index_, 002);
    EXPECT_EQ(g.segments[16]->opposite_->index_, 300); EXPECT_EQ(g.segments[17]->opposite_->index_, 003);
    EXPECT_EQ(g.segments[18]->opposite_->index_, 100); EXPECT_EQ(g.segments[19]->opposite_->index_, 001);

}

TEST(graphs_create_base_star_and_add_edge_and_delete_vertex, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();
    g.add_edge(g.segments[4]->vertices_[0], g.segments[7]->vertices_[0], g.outer_face, 1, 2);
    g.add_vertex(g.segments[g.edges.size() - 2]);
    g.delete_vertex((g.segments[g.edges.size() - 1]->vertices_[0]).get());

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2 * (n - 1) + 2);

    //faces
    set<int> first_face = { 6, 14, 0, 13, 5, 4, 19 };
    int first_face_representant = *first_face.begin();
    int second_face_representant = 1;

    EXPECT_NE(g.segments[first_face_representant]->face_, g.segments[second_face_representant]->face_);
    for (int i = 0; i < g.edges.size();i++) {
        if (first_face.count(i)) {
            EXPECT_EQ(g.segments[i]->face_, g.segments[first_face_representant]->face_);
        }
        else {
            EXPECT_EQ(g.segments[i]->face_, g.segments[second_face_representant]->face_);
        }
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back(), g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->vertices_[0]);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back()->index_, i);

            //opposite od edges
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }

    EXPECT_EQ(g.segments[g.edges.size() - 7]->vertices_.back(), g.segments[0]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 7]->vertices_[0], g.segments[3]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->vertices_.back(), g.segments[3]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->vertices_[0], g.segments[0]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_.back(), g.segments[1]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_[0], g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_.back(), g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_[0], g.segments[1]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_.back(), g.segments[2]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_[0], g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_.back(), g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_[0], g.segments[2]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_.back(), g.segments[4]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_[0], g.segments[7]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->vertices_.back(), g.segments[7]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->vertices_[0], g.segments[4]->vertices_[0]);

    //edges 


    EXPECT_EQ(g.segments[0]->prev_->index_, 100); EXPECT_EQ(g.segments[0]->next_->index_, 002);
    EXPECT_EQ(g.segments[1]->prev_->index_, 200); EXPECT_EQ(g.segments[1]->next_->index_, 003);
    EXPECT_EQ(g.segments[2]->prev_->index_, 300); EXPECT_EQ(g.segments[2]->next_->index_, 001);
    EXPECT_EQ(g.segments[3]->prev_->index_, 001); EXPECT_EQ(g.segments[3]->next_->index_, 102);
    EXPECT_EQ(g.segments[4]->prev_->index_, 201); EXPECT_EQ(g.segments[4]->next_->index_, 101);
    EXPECT_EQ(g.segments[5]->prev_->index_, 101); EXPECT_EQ(g.segments[5]->next_->index_, 100);
    EXPECT_EQ(g.segments[6]->prev_->index_, 002); EXPECT_EQ(g.segments[6]->next_->index_, 201);
    EXPECT_EQ(g.segments[7]->prev_->index_, 102); EXPECT_EQ(g.segments[7]->next_->index_, 202);
    EXPECT_EQ(g.segments[8]->prev_->index_, 202); EXPECT_EQ(g.segments[8]->next_->index_, 200);
    EXPECT_EQ(g.segments[9]->prev_->index_, 003); EXPECT_EQ(g.segments[9]->next_->index_, 303);
    EXPECT_EQ(g.segments[10]->prev_->index_, 303); EXPECT_EQ(g.segments[10]->next_->index_, 303);
    EXPECT_EQ(g.segments[11]->prev_->index_, 303); EXPECT_EQ(g.segments[11]->next_->index_, 300);
    EXPECT_EQ(g.segments[12]->prev_->index_, 000); EXPECT_EQ(g.segments[12]->next_->index_, 101);
    EXPECT_EQ(g.segments[13]->prev_->index_, 101); EXPECT_EQ(g.segments[13]->next_->index_, 000);
    EXPECT_EQ(g.segments[14]->prev_->index_, 000); EXPECT_EQ(g.segments[14]->next_->index_, 202);
    EXPECT_EQ(g.segments[15]->prev_->index_, 202); EXPECT_EQ(g.segments[15]->next_->index_, 000);
    EXPECT_EQ(g.segments[16]->prev_->index_, 000); EXPECT_EQ(g.segments[16]->next_->index_, 303);
    EXPECT_EQ(g.segments[17]->prev_->index_, 303); EXPECT_EQ(g.segments[17]->next_->index_, 000);
    EXPECT_EQ(g.segments[18]->prev_->index_, 101); EXPECT_EQ(g.segments[18]->next_->index_, 202);
    EXPECT_EQ(g.segments[19]->prev_->index_, 202); EXPECT_EQ(g.segments[19]->next_->index_, 101);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 100); EXPECT_EQ(g.segments[13]->opposite_->index_, 001);
    EXPECT_EQ(g.segments[14]->opposite_->index_, 200); EXPECT_EQ(g.segments[15]->opposite_->index_, 002);
    EXPECT_EQ(g.segments[16]->opposite_->index_, 300); EXPECT_EQ(g.segments[17]->opposite_->index_, 003);
    EXPECT_EQ(g.segments[18]->opposite_->index_, 201); EXPECT_EQ(g.segments[19]->opposite_->index_, 102);
}

TEST(graphs_create_base_star_and_add_edge_and_add_vertex, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();
    g.add_edge(g.segments[4]->vertices_[0], g.segments[7]->vertices_[0], g.outer_face, 1, 2);
    g.add_vertex(g.segments[g.edges.size() - 2]);

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2 * (n - 1) + 4);

    //faces
    set<int> first_face = { 6, 14, 0, 13, 5, 4, 19, 21};
    int first_face_representant = *first_face.begin();
    int second_face_representant = 1;

    EXPECT_NE(g.segments[first_face_representant]->face_, g.segments[second_face_representant]->face_);
    for (int i = 0; i < g.edges.size();i++) {
        if (first_face.count(i)) {
            EXPECT_EQ(g.segments[i]->face_, g.segments[first_face_representant]->face_);
        }
        else {
            EXPECT_EQ(g.segments[i]->face_, g.segments[second_face_representant]->face_);
        }
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back(), g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->vertices_[0]);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back()->index_, i);

            //opposite od edges
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }

    for (int i = 0; i < n * (n - 1);i++) {
        EXPECT_NE(g.segments[i]->vertices_[0], g.segments[g.edges.size() - 1]->vertices_[0]);
    }

    EXPECT_EQ(g.segments[g.edges.size() - 9]->vertices_.back(), g.segments[0]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 9]->vertices_[0], g.segments[3]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 10]->vertices_.back(), g.segments[3]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 10]->vertices_[0], g.segments[0]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 7]->vertices_.back(), g.segments[1]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 7]->vertices_[0], g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->vertices_.back(), g.segments[6]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->vertices_[0], g.segments[1]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_.back(), g.segments[2]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 5]->vertices_[0], g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_.back(), g.segments[9]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->vertices_[0], g.segments[2]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_[0], g.segments[7]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 3]->vertices_.back(), g.segments[g.edges.size() - 4]->vertices_.back());
    EXPECT_EQ(g.segments[g.edges.size() - 4]->vertices_[0], g.segments[4]->vertices_[0]);

    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_.back(), g.segments[4]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->vertices_[0], g.segments[g.edges.size() - 2]->vertices_[0]);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->vertices_.back(), g.segments[7]->vertices_[0]);


    //edges 

    EXPECT_EQ(g.segments[0]->prev_->index_, 100); EXPECT_EQ(g.segments[0]->next_->index_, 002);
    EXPECT_EQ(g.segments[1]->prev_->index_, 200); EXPECT_EQ(g.segments[1]->next_->index_, 003);
    EXPECT_EQ(g.segments[2]->prev_->index_, 300); EXPECT_EQ(g.segments[2]->next_->index_, 001);
    EXPECT_EQ(g.segments[3]->prev_->index_, 001); EXPECT_EQ(g.segments[3]->next_->index_, 102);
    EXPECT_EQ(g.segments[4]->prev_->index_, 201); EXPECT_EQ(g.segments[4]->next_->index_, 101);
    EXPECT_EQ(g.segments[5]->prev_->index_, 101); EXPECT_EQ(g.segments[5]->next_->index_, 100);
    EXPECT_EQ(g.segments[6]->prev_->index_, 002); EXPECT_EQ(g.segments[6]->next_->index_, 201);
    EXPECT_EQ(g.segments[7]->prev_->index_, 102); EXPECT_EQ(g.segments[7]->next_->index_, 202);
    EXPECT_EQ(g.segments[8]->prev_->index_, 202); EXPECT_EQ(g.segments[8]->next_->index_, 200);
    EXPECT_EQ(g.segments[9]->prev_->index_, 003); EXPECT_EQ(g.segments[9]->next_->index_, 303);
    EXPECT_EQ(g.segments[10]->prev_->index_, 303); EXPECT_EQ(g.segments[10]->next_->index_, 303);
    EXPECT_EQ(g.segments[11]->prev_->index_, 303); EXPECT_EQ(g.segments[11]->next_->index_, 300);
    EXPECT_EQ(g.segments[12]->prev_->index_, 000); EXPECT_EQ(g.segments[12]->next_->index_, 101);
    EXPECT_EQ(g.segments[13]->prev_->index_, 101); EXPECT_EQ(g.segments[13]->next_->index_, 000);
    EXPECT_EQ(g.segments[14]->prev_->index_, 000); EXPECT_EQ(g.segments[14]->next_->index_, 202);
    EXPECT_EQ(g.segments[15]->prev_->index_, 202); EXPECT_EQ(g.segments[15]->next_->index_, 000);
    EXPECT_EQ(g.segments[16]->prev_->index_, 000); EXPECT_EQ(g.segments[16]->next_->index_, 303);
    EXPECT_EQ(g.segments[17]->prev_->index_, 303); EXPECT_EQ(g.segments[17]->next_->index_, 000);
    EXPECT_EQ(g.segments[18]->prev_->index_, 101); EXPECT_EQ(g.segments[18]->next_->index_, 102);
    EXPECT_EQ(g.segments[19]->prev_->index_, 202); EXPECT_EQ(g.segments[19]->next_->index_, 201);
    EXPECT_EQ(g.segments[20]->prev_->index_, 102); EXPECT_EQ(g.segments[20]->next_->index_, 202);
    EXPECT_EQ(g.segments[21]->prev_->index_, 201); EXPECT_EQ(g.segments[21]->next_->index_, 101);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 100); EXPECT_EQ(g.segments[13]->opposite_->index_, 001);
    EXPECT_EQ(g.segments[14]->opposite_->index_, 200); EXPECT_EQ(g.segments[15]->opposite_->index_, 002);
    EXPECT_EQ(g.segments[16]->opposite_->index_, 300); EXPECT_EQ(g.segments[17]->opposite_->index_, 003);
    EXPECT_EQ(g.segments[18]->opposite_->index_, 201); EXPECT_EQ(g.segments[19]->opposite_->index_, 102);
    EXPECT_EQ(g.segments[20]->opposite_->index_, 201); EXPECT_EQ(g.segments[19]->opposite_->index_, 102);
}
