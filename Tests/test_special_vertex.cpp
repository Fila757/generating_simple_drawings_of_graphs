#include "pch.h"
#include "../drawing_of_cliques/functions.hpp"
#include "../packages/Microsoft.googletest.v140.windesktop.msvcstl.static.rt-dyn.1.8.1.3/build/native/include/gtest/gtest.h"

TEST(graphs_create_special_vertex, graph_3) {
    auto g = graph(3, 0, nullptr);

    g.create_special_vertex(1);

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
    auto g = graph(4, 0, nullptr);

    g.create_special_vertex(0);

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
    auto g = graph(5, 0, nullptr);

    g.create_special_vertex(3);


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