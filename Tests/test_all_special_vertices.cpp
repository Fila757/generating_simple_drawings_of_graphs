#include "pch.h"
#include "../drawing_of_cliques/functions.hpp"
#include "../packages/Microsoft.googletest.v140.windesktop.msvcstl.static.rt-dyn.1.8.1.3/build/native/include/gtest/gtest.h"

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
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back(), g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->vertices_[0]);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back()->index_, i);

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
    for (int i = 0; i < n * (n - 1);i++) {
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

    //edges 
    for (int i = 0; i < n;i++) {
        for (int j = 0; j < n - 1;j++) {
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
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back(), g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->vertices_[0]);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back()->index_, i);

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

TEST(graphs_all_special_vertices, graph_8) {
    auto g = graph(8);
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
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back(), g.segments[(n - 1) * i + ((j + 1) % (n - 1))]->vertices_[0]);
            EXPECT_EQ(g.segments[(n - 1) * i + j]->vertices_.back()->index_, i);

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
