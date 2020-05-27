#include "pch.h"
#include "../drawing_of_cliques/functions.hpp"

TEST(graphs, śize_og_block1) {
    graph g = graph(8);
    EXPECT_EQ(1, g.size_of_block);
}

TEST(graphs, size_of_block2) {
    graph g = graph(9);
    EXPECT_EQ(1, g.size_of_block);
}

TEST(graphs, size_of_block3) {
    graph g = graph(1);
    EXPECT_EQ(0, g.size_of_block);
}

TEST(graphs, size_of_block4) {
    graph g = graph(17);
    EXPECT_EQ(2, g.size_of_block);
}

TEST(graphs, size_of_block5) {
    graph g = graph(12);
    EXPECT_EQ(2, g.size_of_block);
}

TEST(graphs, size_of_block6) {
    graph g = graph(18);
    EXPECT_EQ(3, g.size_of_block);
}

TEST(graphs, vertices_positions1) {
    vector<pair<int, int> > original;
    original.push_back(make_pair(0, 0));
    original.push_back(make_pair(2, -1));
    original.push_back(make_pair(1, 2));
    original.push_back(make_pair(-2, 1));
    original.push_back(make_pair(-1, -2));
    original.push_back(make_pair(2, 0));
    original.push_back(make_pair(0, 2));
    original.push_back(make_pair(-2, 0));
    original.push_back(make_pair(0, -2));
    original.push_back(make_pair(2, 1));

    graph g = graph(10);
    for (int i = 0; i < 10;i++) {
        g.create_next_vertex();
    }
    vector<pair<int, int> > data;
    for (auto it : g.vertices) {
        data.push_back(make_pair(it.x, it.y));
    }

    EXPECT_EQ(original, data);
}


TEST(graphs, vertices2) {
    vector<pair<int, int> > original;
    original.push_back(make_pair(0, 0));

    graph g = graph(1);
    for (int i = 0; i < 1;i++) {
        g.create_next_vertex();
    }
    vector<pair<int, int> > data;
    for (auto it : g.vertices) {
        data.push_back(make_pair(it.x, it.y));
    }

    EXPECT_EQ(original, data);
}

TEST(TestCaseName, TestName) {
  EXPECT_EQ(1, 1);
  EXPECT_TRUE(true);
}

int main(int argc, char** argv) {
    testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}