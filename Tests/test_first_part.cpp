#include "pch.h"
#include "../drawing_of_cliques/functions.hpp"
#include "../packages/Microsoft.googletest.v140.windesktop.msvcstl.static.rt-dyn.1.8.1.3/build/native/include/gtest/gtest.h"

TEST(graphs_properties, number_of_vertices1) {
    graph g = graph(8);
    EXPECT_EQ(g.number_of_vertices, 8);
}

TEST(graphs_properties, number_of_vertices2) {
    graph g = graph(1000);
    EXPECT_EQ(g.number_of_vertices, 1000);
}

TEST(graphs_create_special_vertex, graph_3) {
    auto g = graph(3);

    g.create_special_vertex(make_pair(0, 0), 1);

    EXPECT_EQ(g.edges.size(), 2);

    //faces
    for (int i = 0; i < 2;i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < 2;i++) {
        EXPECT_EQ(g.segments[i]->to_, g.segments[(i + 1) % 2]->from_);
        EXPECT_EQ(g.segments[i]->to_->index_, 1);
    }

    //edges 
    for (int i = 0; i < 2;i++) {
        EXPECT_EQ(g.segments[i]->prev_->index_, g.segments[(i - 1 + 2) % 2]->index_);
        EXPECT_EQ(g.segments[i]->next_->index_, g.segments[(i + 1) % 2]->index_);
        EXPECT_EQ(g.segments[i]->opposite_, nullptr);
    }
}


TEST(graphs_create_special_vertex, graph_4) {
    auto g = graph(4);

    g.create_special_vertex(make_pair(0, 0), 0);

    EXPECT_EQ(g.edges.size(), 3);

    //faces
    for (int i = 0; i < 3;i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }
    
    //vertices 
    for (int i = 0; i < 3;i++) {
        EXPECT_EQ(g.segments[i]->to_, g.segments[(i + 1) % 3]->from_);
        EXPECT_EQ(g.segments[i]->to_->index_, 0);
    }

    //edges 
    for (int i = 0; i < 3;i++) {
        EXPECT_EQ(g.segments[i]->prev_->index_, g.segments[(i - 1 + 3) % 3]->index_);
        EXPECT_EQ(g.segments[i]->next_->index_, g.segments[(i + 1) % 3]->index_);
        EXPECT_EQ(g.segments[i]->opposite_, nullptr);
    }
}

TEST(graphs_create_special_vertex, graph_5) {
    auto g = graph(5);

    g.create_special_vertex(make_pair(0, 0), 3);


    EXPECT_EQ(g.edges.size(), 4);

    //faces
    for (int i = 0; i < 4;i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < 4;i++) {
        EXPECT_EQ(g.segments[i]->to_, g.segments[(i + 1) % 4]->from_);
        EXPECT_EQ(g.segments[i]->to_->index_, 3);
    }

    //edges 
    for (int i = 0; i < 4;i++) {
        EXPECT_EQ(g.segments[i]->prev_->index_, g.segments[(i - 1 + 4) % 4]->index_);
        EXPECT_EQ(g.segments[i]->next_->index_, g.segments[(i + 1) % 4]->index_);
        EXPECT_EQ(g.segments[i]->opposite_, nullptr);
    }
}

TEST(graphs_all_special_vertices, graph_3) {
    auto g = graph(3);
    int n = g.number_of_vertices;
    g.create_all_special_vertices();
    //g.recolor_fingerprint("120201"); //recolor to the first coloring

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

            //opposite od edges
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
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


TEST(graphs_all_special_vertices, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;
    g.create_all_special_vertices();
    //g.recolor_fingerprint("123023013012"); //recolor to the first coloring

    EXPECT_EQ(g.edges.size(), n * (n - 1));

    //faces
    for (int i = 0; i < n*(n-1);i++) {
        EXPECT_EQ(g.segments[i]->face_, g.outer_face);
    }

    //vertices 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n-1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->from_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->to_->index_, i);

            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }

    //edges 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n-1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->prev_->index_, g.segments[(n - 1) * i + (j - 1 + (n - 1)) % (n - 1)]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->next_->index_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }
}

TEST(graphs_all_special_vertices, graph_5) {
    auto g = graph(5);
    int n = g.number_of_vertices;
    g.create_all_special_vertices();
    //g.recolor_fingerprint("12340234013401240123"); //recolor to the first coloring

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

            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
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

TEST(graphs_all_special_vertices, graph_100) {
    auto g = graph(100);
    int n = g.number_of_vertices;
    g.create_all_special_vertices();
    //g.recolor_fingerprint("12340234013401240123"); //recolor to the first coloring

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

            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
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


TEST(graphs_add_edge_outer_face, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.add_edge(g.segments[g.starts[0][1]]->from_, g.segments[g.starts[1][0]]->from_, g.outer_face, true);

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

    EXPECT_EQ(g.segments[0]->prev_->index_, 13); EXPECT_EQ(g.segments[0]->next_->index_, 1);
    EXPECT_EQ(g.segments[1]->prev_->index_, 0); EXPECT_EQ(g.segments[1]->next_->index_, 2);
    EXPECT_EQ(g.segments[2]->prev_->index_, 1); EXPECT_EQ(g.segments[2]->next_->index_, 12);
    EXPECT_EQ(g.segments[3]->prev_->index_, 12); EXPECT_EQ(g.segments[3]->next_->index_, 4);
    EXPECT_EQ(g.segments[4]->prev_->index_, 3); EXPECT_EQ(g.segments[4]->next_->index_, 5);
    EXPECT_EQ(g.segments[5]->prev_->index_, 4); EXPECT_EQ(g.segments[5]->next_->index_, 13);
    EXPECT_EQ(g.segments[12]->prev_->index_, 2); EXPECT_EQ(g.segments[12]->next_->index_, 3);
    EXPECT_EQ(g.segments[13]->prev_->index_, 5); EXPECT_EQ(g.segments[13]->next_->index_, 0);
    EXPECT_EQ(g.segments[12]->opposite_->index_, 13); EXPECT_EQ(g.segments[13]->opposite_->index_, 12);

    for (int i = 2; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->prev_->index_, g.segments[(n - 1) * i + (j - 1 + (n - 1)) % (n - 1)]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->next_->index_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }
}

TEST(graphs_create_base_star, graph_4) {
    auto g = graph(4);
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

TEST(graphs_delete_edge_outer_face, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;
    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring

    g.add_edge(g.segments[g.starts[0][1]]->from_, g.segments[g.starts[1][0]]->from_, g.outer_face, true);
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
    auto g = graph(4);
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
    auto g = graph(4);
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

    EXPECT_EQ(g.segments[0]->prev_->index_, 13); EXPECT_EQ(g.segments[0]->next_->index_, 1);
    EXPECT_EQ(g.segments[1]->prev_->index_, 0); EXPECT_EQ(g.segments[1]->next_->index_, 2);
    EXPECT_EQ(g.segments[2]->prev_->index_, 1); EXPECT_EQ(g.segments[2]->next_->index_, 12);
    EXPECT_EQ(g.segments[3]->prev_->index_, 12); EXPECT_EQ(g.segments[3]->next_->index_, 4);
    EXPECT_EQ(g.segments[4]->prev_->index_, 3); EXPECT_EQ(g.segments[4]->next_->index_, 5);
    EXPECT_EQ(g.segments[5]->prev_->index_, 4); EXPECT_EQ(g.segments[5]->next_->index_, 13);
    EXPECT_EQ(g.segments[12]->prev_->index_, 2); EXPECT_EQ(g.segments[12]->next_->index_, 3);
    EXPECT_EQ(g.segments[13]->prev_->index_, 5); EXPECT_EQ(g.segments[13]->next_->index_, 0);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 13); EXPECT_EQ(g.segments[13]->opposite_->index_, 12);

    for (int i = 2; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->prev_->index_, g.segments[(n - 1) * i + (j - 1 + (n - 1)) % (n - 1)]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->next_->index_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }
}

TEST(graphs_create_base_star_and_add_edge, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();
    g.add_edge(g.segments[4]->from_, g.segments[7]->from_, g.outer_face);

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

    EXPECT_EQ(g.segments[0]->prev_->index_, 13); EXPECT_EQ(g.segments[0]->next_->index_, 14);
    EXPECT_EQ(g.segments[1]->prev_->index_, 15); EXPECT_EQ(g.segments[1]->next_->index_, 16);
    EXPECT_EQ(g.segments[2]->prev_->index_, 17); EXPECT_EQ(g.segments[2]->next_->index_, 12);
    EXPECT_EQ(g.segments[3]->prev_->index_, 12); EXPECT_EQ(g.segments[3]->next_->index_, 18);
    EXPECT_EQ(g.segments[4]->prev_->index_, 19); EXPECT_EQ(g.segments[4]->next_->index_, 5);
    EXPECT_EQ(g.segments[5]->prev_->index_, 4); EXPECT_EQ(g.segments[5]->next_->index_, 13);
    EXPECT_EQ(g.segments[6]->prev_->index_, 14); EXPECT_EQ(g.segments[6]->next_->index_, 19);
    EXPECT_EQ(g.segments[7]->prev_->index_, 18); EXPECT_EQ(g.segments[7]->next_->index_, 8);
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
    EXPECT_EQ(g.segments[18]->prev_->index_, 3); EXPECT_EQ(g.segments[18]->next_->index_, 7);
    EXPECT_EQ(g.segments[19]->prev_->index_, 6); EXPECT_EQ(g.segments[19]->next_->index_, 4);

    EXPECT_EQ(g.segments[12]->opposite_->index_, 13); EXPECT_EQ(g.segments[13]->opposite_->index_, 12);
    EXPECT_EQ(g.segments[14]->opposite_->index_, 15); EXPECT_EQ(g.segments[15]->opposite_->index_, 14);
    EXPECT_EQ(g.segments[16]->opposite_->index_, 17); EXPECT_EQ(g.segments[17]->opposite_->index_, 16);
    EXPECT_EQ(g.segments[18]->opposite_->index_, 19); EXPECT_EQ(g.segments[19]->opposite_->index_, 18);
}

TEST(graphs_create_delete_edge, graph_4) {
    auto g = graph(4);
    int n = g.number_of_vertices;

    g.create_all_special_vertices();
    g.recolor_fingerprint("123023013012"); //recolor to the first coloring
    g.create_base_star();

    g.add_edge(g.segments[4]->from_, g.segments[7]->from_, g.outer_face);
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

int main(int argc, char** argv) {
    testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}
