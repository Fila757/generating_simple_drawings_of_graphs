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
