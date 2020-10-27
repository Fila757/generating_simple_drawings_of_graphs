#include "pch.h"
#include "../drawing_of_cliques/functions.hpp"
#include "../packages/Microsoft.googletest.v140.windesktop.msvcstl.static.rt-dyn.1.8.1.3/build/native/include/gtest/gtest.h"

TEST(dijsktra, test1) {
    graph g = graph(6);
    EXPECT_EQ(g.number_of_vertices, 6);
}

TEST(dijsktra, test2) {
    graph g = graph(8);
    EXPECT_EQ(g.number_of_vertices, 8);
}
