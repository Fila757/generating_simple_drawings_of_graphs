#include "pch.h"
#include "../drawing_of_cliques/functions.hpp"
#include "../packages/Microsoft.googletest.v140.windesktop.msvcstl.static.rt-dyn.1.8.1.3/build/native/include/gtest/gtest.h"

TEST(all_fingerprints, graph_4) {
    auto generator_of_fingerprints = fingerprints(4);
    vector<string> correct_fingerprints = { "023013012","023013021","023031012","023031021","032013012","032013021","032031012","032031021"};

    int i = 0;
    while (!generator_of_fingerprints.done) {
        EXPECT_EQ(generator_of_fingerprints.get_next(), correct_fingerprints[i]);
        i++;
    }
}

TEST(all_fingerprints, graph_3) {
    auto generator_of_fingerprints = fingerprints(3);
    vector<string> correct_fingerprints = { "0201" };

    int i = 0;
    while (!generator_of_fingerprints.done) {
        EXPECT_EQ(generator_of_fingerprints.get_next(), correct_fingerprints[i]);
        i++;
    }
}

/*
TEST(all_fingerprints, graph_5) {
    auto generator_of_fingerprints = fingerprints(5);
    vector<string> fingerprints;

    while (!generator_of_fingerprints.done) {
        fingerprints.push_back(generator_of_fingerprints.get_next());
    }

    EXPECT_EQ(fingerprints.size(), (int)pow(6, 4));

    
    for (int i = 0; i < fingerprints.size();i++) {
        EXPECT_EQ(fingerprints[i][0], '0');
        for (int j = i + 1; j < fingerprints.size();j++) {
            EXPECT_NE(fingerprints[i], fingerprints[j]);
        }
    }
    
}
*/
/*
TEST(all_fingerprints, graph_6) {
    auto generator_of_fingerprints = fingerprints(6);
    vector<string> fingerprints;

    while (!generator_of_fingerprints.done) {
        fingerprints.push_back(generator_of_fingerprints.get_next());
    }

    EXPECT_EQ(fingerprints.size(), (int)pow(24, 5));

    for (int i = 0; i < fingerprints.size();i++) {
        EXPECT_EQ(fingerprints[i][0], '0');
        for (int j = i + 1; j < fingerprints.size();j++) {
            EXPECT_NE(fingerprints[i], fingerprints[j]);
        }
    }
}
*/