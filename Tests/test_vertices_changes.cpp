#include "pch.h"
#include "../drawing_of_cliques/functions.hpp"
#include "../packages/Microsoft.googletest.v140.windesktop.msvcstl.static.rt-dyn.1.8.1.3/build/native/include/gtest/gtest.h"

TEST(graphs_add_vertex, graph_4){
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();

    //g.add_edge(g.segments[4]->from_, g.segments[7]->from_, g.outer_face);
    g.add_vertex(g.segments[g.edges.size() - 6]);

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2 * (n - 1) + 2);

    //faces
    for (int i = 0; i < n * (n - 1);i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->from_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_->index_, i);

            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }

    /*the new vertex is really new*/
    for (int i = 0; i < n * (n - 1);i++) {
        EXPECT_NE(g.segments[i]->from_, g.segments[g.edges.size() - 1]->from_);
    }

    EXPECT_EQ(g.segments[g.edges.size() - 7]->to_, g.segments[g.edges.size() - 8]->to_);
    EXPECT_EQ(g.segments[g.edges.size() - 7]->from_, g.segments[3]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->from_, g.segments[0]->from_);

    EXPECT_EQ(g.segments[g.edges.size() - 5]->to_, g.segments[1]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 5]->from_, g.segments[6]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->to_, g.segments[6]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->from_, g.segments[1]->from_);

    EXPECT_EQ(g.segments[g.edges.size() - 3]->to_, g.segments[2]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 3]->from_, g.segments[9]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->to_, g.segments[9]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->from_, g.segments[2]->from_);

    EXPECT_EQ(g.segments[g.edges.size() - 1]->to_, g.segments[0]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->from_, g.segments[g.edges.size() - 2]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->to_, g.segments[3]->from_);

    //edges 

    EXPECT_EQ(g.segments[0]->prev_->index_, 13); EXPECT_EQ(g.segments[0]->next_->index_, 14);
    EXPECT_EQ(g.segments[1]->prev_->index_, 15); EXPECT_EQ(g.segments[1]->next_->index_, 16);
    EXPECT_EQ(g.segments[2]->prev_->index_, 17); EXPECT_EQ(g.segments[2]->next_->index_, 12);
    EXPECT_EQ(g.segments[3]->prev_->index_, 12); EXPECT_EQ(g.segments[3]->next_->index_, 4);
    EXPECT_EQ(g.segments[4]->prev_->index_, 3); EXPECT_EQ(g.segments[4]->next_->index_, 5);
    EXPECT_EQ(g.segments[5]->prev_->index_, 4); EXPECT_EQ(g.segments[5]->next_->index_, 13);
    EXPECT_EQ(g.segments[6]->prev_->index_, 14); EXPECT_EQ(g.segments[6]->next_->index_, 7);
    EXPECT_EQ(g.segments[7]->prev_->index_, 6); EXPECT_EQ(g.segments[7]->next_->index_, 8);
    EXPECT_EQ(g.segments[8]->prev_->index_, 7); EXPECT_EQ(g.segments[8]->next_->index_, 15);
    EXPECT_EQ(g.segments[9]->prev_->index_, 16); EXPECT_EQ(g.segments[9]->next_->index_, 10);
    EXPECT_EQ(g.segments[10]->prev_->index_, 9); EXPECT_EQ(g.segments[10]->next_->index_, 11);
    EXPECT_EQ(g.segments[11]->prev_->index_, 10); EXPECT_EQ(g.segments[11]->next_->index_, 17);
    EXPECT_EQ(g.segments[12]->prev_->index_, 2); EXPECT_EQ(g.segments[12]->next_->index_, 12);
    EXPECT_EQ(g.segments[13]->prev_->index_, 5); EXPECT_EQ(g.segments[13]->next_->index_, 13);
    EXPECT_EQ(g.segments[14]->prev_->index_, 0); EXPECT_EQ(g.segments[14]->next_->index_, 6);
    EXPECT_EQ(g.segments[15]->prev_->index_, 8); EXPECT_EQ(g.segments[15]->next_->index_, 1);
    EXPECT_EQ(g.segments[16]->prev_->index_, 1); EXPECT_EQ(g.segments[16]->next_->index_, 9);
    EXPECT_EQ(g.segments[17]->prev_->index_, 11); EXPECT_EQ(g.segments[17]->next_->index_, 2);
    EXPECT_EQ(g.segments[18]->prev_->index_, 12); EXPECT_EQ(g.segments[18]->next_->index_, 3);
    EXPECT_EQ(g.segments[19]->prev_->index_, 13); EXPECT_EQ(g.segments[19]->next_->index_, 0);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 13); EXPECT_EQ(g.segments[13]->opposite_->index_, 12);
    EXPECT_EQ(g.segments[14]->opposite_->index_, 15); EXPECT_EQ(g.segments[15]->opposite_->index_, 14);
    EXPECT_EQ(g.segments[16]->opposite_->index_, 17); EXPECT_EQ(g.segments[17]->opposite_->index_, 16);
    EXPECT_EQ(g.segments[18]->opposite_->index_, 13); EXPECT_EQ(g.segments[19]->opposite_->index_, 12);

}

TEST(graphs_delete_vertex, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();

    //g.add_edge(g.segments[4]->from_, g.segments[7]->from_, g.outer_face);
    g.add_vertex(g.segments[g.edges.size() - 6]);
    g.delete_vertex((g.segments[g.edges.size() - 1]->from_).get());

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2 * (n - 1));

    //faces
    for (int i = 0; i < n * (n - 1);i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->from_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_->index_, i);

            //opposite od edges
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }


    EXPECT_EQ(g.segments[g.edges.size() - 5]->to_, g.segments[0]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 5]->from_, g.segments[3]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->to_, g.segments[3]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->from_, g.segments[0]->from_);

    EXPECT_EQ(g.segments[g.edges.size() - 3]->to_, g.segments[1]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 3]->from_, g.segments[6]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->to_, g.segments[6]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->from_, g.segments[1]->from_);

    EXPECT_EQ(g.segments[g.edges.size() - 1]->to_, g.segments[2]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->from_, g.segments[9]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->to_, g.segments[9]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->from_, g.segments[2]->from_);

    //edges 

    EXPECT_EQ(g.segments[0]->prev_->index_, 13); EXPECT_EQ(g.segments[0]->next_->index_, 14);
    EXPECT_EQ(g.segments[1]->prev_->index_, 15); EXPECT_EQ(g.segments[1]->next_->index_, 16);
    EXPECT_EQ(g.segments[2]->prev_->index_, 17); EXPECT_EQ(g.segments[2]->next_->index_, 12);
    EXPECT_EQ(g.segments[3]->prev_->index_, 12); EXPECT_EQ(g.segments[3]->next_->index_, 4);
    EXPECT_EQ(g.segments[4]->prev_->index_, 3); EXPECT_EQ(g.segments[4]->next_->index_, 5);
    EXPECT_EQ(g.segments[5]->prev_->index_, 4); EXPECT_EQ(g.segments[5]->next_->index_, 13);
    EXPECT_EQ(g.segments[6]->prev_->index_, 14); EXPECT_EQ(g.segments[6]->next_->index_, 7);
    EXPECT_EQ(g.segments[7]->prev_->index_, 6); EXPECT_EQ(g.segments[7]->next_->index_, 8);
    EXPECT_EQ(g.segments[8]->prev_->index_, 7); EXPECT_EQ(g.segments[8]->next_->index_, 15);
    EXPECT_EQ(g.segments[9]->prev_->index_, 16); EXPECT_EQ(g.segments[9]->next_->index_, 10);
    EXPECT_EQ(g.segments[10]->prev_->index_, 9); EXPECT_EQ(g.segments[10]->next_->index_, 11);
    EXPECT_EQ(g.segments[11]->prev_->index_, 10); EXPECT_EQ(g.segments[11]->next_->index_, 17);
    EXPECT_EQ(g.segments[12]->prev_->index_, 2); EXPECT_EQ(g.segments[12]->next_->index_, 3);
    EXPECT_EQ(g.segments[13]->prev_->index_, 5); EXPECT_EQ(g.segments[13]->next_->index_, 0);
    EXPECT_EQ(g.segments[14]->prev_->index_, 0); EXPECT_EQ(g.segments[14]->next_->index_, 6);
    EXPECT_EQ(g.segments[15]->prev_->index_, 8); EXPECT_EQ(g.segments[15]->next_->index_, 1);
    EXPECT_EQ(g.segments[16]->prev_->index_, 1); EXPECT_EQ(g.segments[16]->next_->index_, 9);
    EXPECT_EQ(g.segments[17]->prev_->index_, 11); EXPECT_EQ(g.segments[17]->next_->index_, 2);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 13); EXPECT_EQ(g.segments[13]->opposite_->index_, 12);
    EXPECT_EQ(g.segments[14]->opposite_->index_, 15); EXPECT_EQ(g.segments[15]->opposite_->index_, 14);
    EXPECT_EQ(g.segments[16]->opposite_->index_, 17); EXPECT_EQ(g.segments[17]->opposite_->index_, 16);
}


