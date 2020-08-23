#include "pch.h"
#include "../drawing_of_cliques/functions.hpp"
#include "../packages/Microsoft.googletest.v140.windesktop.msvcstl.static.rt-dyn.1.8.1.3/build/native/include/gtest/gtest.h"

TEST(graphs, number_of_vertices) {
    graph g = graph(8);
    EXPECT_EQ(8, g.number_of_vertices);
}

TEST(graphs, size_of_block2) {
    graph g = graph(1000);
    EXPECT_EQ(1000, g.number_of_vertices);
}




TEST(TestCaseName, TestName) {
    EXPECT_EQ(1, 1);
    EXPECT_TRUE(true);
}

int main(int argc, char** argv) {
    testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}
