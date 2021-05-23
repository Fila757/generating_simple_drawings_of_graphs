#include "pch.h"
#include "../drawing_of_cliques/functions.hpp"
#include "../packages/Microsoft.googletest.v140.windesktop.msvcstl.static.rt-dyn.1.8.1.3/build/native/include/gtest/gtest.h"

TEST(graphs_add_edge_outer_face, graph_4) {
    auto g = graph(4, 0, nullptr);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.add_edge(g.segments[g.starts[0][1]]->from_, g.segments[g.starts[1][0]]->from_, g.outer_face, 0, 1, true);

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2);

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
    EXPECT_EQ(g.segments[g.edges.size() - 1]->to_, g.segments[0]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->from_, g.segments[3]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->to_, g.segments[3]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->from_, g.segments[0]->from_);
        
    //edges 

    EXPECT_EQ(g.segments[0]->prev_->index_, 100); EXPECT_EQ(g.segments[0]->next_->index_, 000);
    EXPECT_EQ(g.segments[1]->prev_->index_, 000); EXPECT_EQ(g.segments[1]->next_->index_, 000);
    EXPECT_EQ(g.segments[2]->prev_->index_, 000); EXPECT_EQ(g.segments[2]->next_->index_, 001);
    EXPECT_EQ(g.segments[3]->prev_->index_, 001); EXPECT_EQ(g.segments[3]->next_->index_, 101);
    EXPECT_EQ(g.segments[4]->prev_->index_, 101); EXPECT_EQ(g.segments[4]->next_->index_, 101);
    EXPECT_EQ(g.segments[5]->prev_->index_, 101); EXPECT_EQ(g.segments[5]->next_->index_, 100);
    EXPECT_EQ(g.segments[12]->prev_->index_, 000); EXPECT_EQ(g.segments[12]->next_->index_, 101);
    EXPECT_EQ(g.segments[13]->prev_->index_, 101); EXPECT_EQ(g.segments[13]->next_->index_, 000);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 100); EXPECT_EQ(g.segments[13]->opposite_->index_, 001);

    for (int i = 2; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->prev_->index_, g.segments[(n - 1) * i + (j - 1 + (n - 1)) % (n - 1)]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->next_->index_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }
}

TEST(graphs_create_base_star, graph_4) {
    auto g = graph(4, 0, nullptr);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2*(n - 1));

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

TEST(graphs_delete_edge_outer_face, graph_4) {
    auto g = graph(4, 0, nullptr);
    int n = g.number_of_vertices;
    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring

    g.add_edge(g.segments[g.starts[0][1]]->from_, g.segments[g.starts[1][0]]->from_, g.outer_face, 0, 1, true);
    g.delete_edge_back(true);

    EXPECT_EQ(g.edges.size(), n * (n - 1));

    //faces
    for (int i = 0; i < n * (n - 1);i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->from_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_->index_, i);
        }
    }

    //edges 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->prev_->index_, g.segments[(n - 1) * i + (j - 1 + (n - 1)) % (n - 1)]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->next_->index_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }
}

TEST(graphs_delete_base_star_outer_face, graph_4) {
    auto g = graph(4, 0, nullptr);
    int n = g.number_of_vertices;
    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring

    g.create_base_star();
    g.delete_edge_back(true);
    g.delete_edge_back(true);
    g.delete_edge_back(true);

    EXPECT_EQ(g.edges.size(), n * (n - 1));

    //faces
    for (int i = 0; i < n * (n - 1);i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->from_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_->index_, i);
        }
    }

    //edges 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->prev_->index_, g.segments[(n - 1) * i + (j - 1 + (n - 1)) % (n - 1)]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->next_->index_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }
}

TEST(graphs_delete_some_edges_outer_face, graph_4) {
    auto g = graph(4, 0, nullptr);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring

    g.create_base_star();
    g.delete_edge_back(true);
    g.delete_edge_back(true);

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2);

    //faces
    for (int i = 0; i < n * (n - 1);i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->from_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_->index_, i);
        }
    }
    EXPECT_EQ(g.segments[g.edges.size() - 1]->to_, g.segments[0]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->from_, g.segments[3]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->to_, g.segments[3]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->from_, g.segments[0]->from_);

    //edges 

    EXPECT_EQ(g.segments[0]->prev_->index_, 100); EXPECT_EQ(g.segments[0]->next_->index_, 000);
    EXPECT_EQ(g.segments[1]->prev_->index_, 000); EXPECT_EQ(g.segments[1]->next_->index_, 000);
    EXPECT_EQ(g.segments[2]->prev_->index_, 000); EXPECT_EQ(g.segments[2]->next_->index_, 001);
    EXPECT_EQ(g.segments[3]->prev_->index_, 001); EXPECT_EQ(g.segments[3]->next_->index_, 101);
    EXPECT_EQ(g.segments[4]->prev_->index_, 101); EXPECT_EQ(g.segments[4]->next_->index_, 101);
    EXPECT_EQ(g.segments[5]->prev_->index_, 101); EXPECT_EQ(g.segments[5]->next_->index_, 100);
    EXPECT_EQ(g.segments[12]->prev_->index_, 000); EXPECT_EQ(g.segments[12]->next_->index_, 101);
    EXPECT_EQ(g.segments[13]->prev_->index_, 101); EXPECT_EQ(g.segments[13]->next_->index_, 000);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 100); EXPECT_EQ(g.segments[13]->opposite_->index_, 001);

    for (int i = 2; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->prev_->index_, g.segments[(n - 1) * i + (j - 1 + (n - 1)) % (n - 1)]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->next_->index_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }
}

TEST(graphs_create_base_star_and_add_edge, graph_4) {
    auto g = graph(4, 0, nullptr);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();
    g.add_edge(g.segments[4]->from_, g.segments[7]->from_, g.outer_face, 1, 2);

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2 * (n - 1) + 2);

    //faces
    set<int> first_face = {6, 14, 0, 13, 5, 4, 19};
    int first_face_representant = *first_face.begin();
    int second_face_representant = 1;

    EXPECT_NE(g.segments[first_face_representant]->face_, g.segments[second_face_representant]->face_);
    for (int i = 0; i < g.edges.size() ;i++){
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
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->from_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_->index_, i);

            //opposite od edges
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }

    EXPECT_EQ(g.segments[g.edges.size() - 7]->to_, g.segments[0]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 7]->from_, g.segments[3]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->to_, g.segments[3]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->from_, g.segments[0]->from_);

    EXPECT_EQ(g.segments[g.edges.size() - 5]->to_, g.segments[1]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 5]->from_, g.segments[6]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->to_, g.segments[6]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->from_, g.segments[1]->from_);

    EXPECT_EQ(g.segments[g.edges.size() - 3]->to_, g.segments[2]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 3]->from_, g.segments[9]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->to_, g.segments[9]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->from_, g.segments[2]->from_);

    EXPECT_EQ(g.segments[g.edges.size() - 1]->to_, g.segments[4]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->from_, g.segments[7]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->to_, g.segments[7]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->from_, g.segments[4]->from_);
    
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

TEST(graphs_create_delete_edge, graph_4) {
    auto g = graph(4, 0, nullptr);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();

    g.add_edge(g.segments[4]->from_, g.segments[7]->from_, g.outer_face, 1, 2);
    g.delete_edge_back();

    EXPECT_EQ(g.edges.size(), n * (n - 1) + 2*(n - 1));

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

TEST(graphs_create_base_star_and_add_edge_starts12_starts21, graph_4) {
    auto g = graph(4, 0, nullptr);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();
    g.add_edge(g.segments[g.starts[1][2]]->from_, g.segments[g.starts[2][1]]->from_, g.outer_face, 1, 2);

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
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->from_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_->index_, i);

            //opposite od edges
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }

    EXPECT_EQ(g.segments[g.edges.size() - 7]->to_, g.segments[0]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 7]->from_, g.segments[3]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->to_, g.segments[3]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 8]->from_, g.segments[0]->from_);

    EXPECT_EQ(g.segments[g.edges.size() - 5]->to_, g.segments[1]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 5]->from_, g.segments[6]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->to_, g.segments[6]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 6]->from_, g.segments[1]->from_);

    EXPECT_EQ(g.segments[g.edges.size() - 3]->to_, g.segments[2]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 3]->from_, g.segments[9]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->to_, g.segments[9]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 4]->from_, g.segments[2]->from_);

    EXPECT_EQ(g.segments[g.edges.size() - 1]->to_, g.segments[4]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 1]->from_, g.segments[7]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->to_, g.segments[7]->from_);
    EXPECT_EQ(g.segments[g.edges.size() - 2]->from_, g.segments[4]->from_);

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

