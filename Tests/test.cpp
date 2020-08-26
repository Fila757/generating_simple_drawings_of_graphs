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





TEST(graphs_add_edge, graph_4) {
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
        }
    }

    //edges 

    EXPECT_EQ(g.segments[0]->prev_->index_, 13); EXPECT_EQ(g.segments[0]->next_->index_, 1);
    EXPECT_EQ(g.segments[1]->prev_->index_, 0); EXPECT_EQ(g.segments[1]->next_->index_, 2);
    EXPECT_EQ(g.segments[2]->prev_->index_, 1); EXPECT_EQ(g.segments[2]->next_->index_, 12);
    EXPECT_EQ(g.segments[3]->prev_->index_, 12); EXPECT_EQ(g.segments[3]->next_->index_, 4);
    EXPECT_EQ(g.segments[4]->prev_->index_, 3); EXPECT_EQ(g.segments[4]->next_->index_, 5);
    EXPECT_EQ(g.segments[5]->prev_->index_, 4); EXPECT_EQ(g.segments[5]->next_->index_, 13);
    EXPECT_EQ(g.segments[12]->prev_->index_, 2); EXPECT_EQ(g.segments[12]->next_->index_, 3);
    EXPECT_EQ(g.segments[13]->prev_->index_, 5); EXPECT_EQ(g.segments[13]->next_->index_, 0);

    for (int i = 2; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
            EXPECT_EQ(g.segments[(n - 1) * i + j]->prev_->index_, g.segments[(n - 1) * i + (j - 1 + (n - 1)) % (n - 1)]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->next_->index_, g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->index_);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->opposite_, nullptr);
        }
    }
}

int main(int argc, char** argv) {
    testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}
