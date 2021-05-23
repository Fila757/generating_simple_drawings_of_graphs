#include "pch.h"
#include "../drawing_of_cliques/functions.hpp"
#include "../packages/Microsoft.googletest.v140.windesktop.msvcstl.static.rt-dyn.1.8.1.3/build/native/include/gtest/gtest.h"


TEST(canonic_fingerprint, graph_4) {
	auto g = graph(4, 0, nullptr);

	EXPECT_EQ(g.find_canonic_fingerprint("123023013012"), "123023013012");
	EXPECT_EQ(g.find_canonic_fingerprint("123023031021"), "123023013012");
	EXPECT_EQ(g.find_canonic_fingerprint("123032031012"), "123023013012");

	EXPECT_EQ(g.find_canonic_fingerprint("123032013021"), "123032013021");

	EXPECT_EQ(g.find_canonic_fingerprint("123023013021"), "123023013021");
	EXPECT_EQ(g.find_canonic_fingerprint("123023031012"), "123023013021");
	EXPECT_EQ(g.find_canonic_fingerprint("123032013012"), "123023013021");
	EXPECT_EQ(g.find_canonic_fingerprint("123032031021"), "123023013021");

}

TEST(right_chosing_canonics, graph_5) {
	auto g = graph(5, 0, nullptr);

	EXPECT_EQ(g.find_canonic_fingerprint("12340234013401240123"), "12340234013401240123");
	EXPECT_EQ(g.find_canonic_fingerprint("12340234013401420132"), "12340234013401420132"); //second ańd third is the same 
	EXPECT_EQ(g.find_canonic_fingerprint("12340234013404120312"), "12340234013401420132"); // because of invers labeling needs to be countet as well
	EXPECT_EQ(g.find_canonic_fingerprint("12340234014301240132"), "12340234014301240132");
	EXPECT_EQ(g.find_canonic_fingerprint("12340243013401420312"), "12340243013401420312");
	EXPECT_EQ(g.find_canonic_fingerprint("12340342014302410321"), "12340342014302410321");

}
