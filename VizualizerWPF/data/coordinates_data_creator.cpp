// Example program
#include <iostream>
#include <string>
#include <fstream>
#include <vector>
#include <bits/stdc++.h>
#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif



using namespace std;

int number_of_vertices = 4;

inline vector<pair<double, double> > create_circle(double radius, double cx, double cy, int n) {
	vector<pair<double, double> > circle;

	double unit_angle = (double)360 / n;

	for (int i = 0; i < n;i++) {
		circle.push_back(make_pair(cx + radius * cos((i + 1) * (unit_angle / 180) * M_PI), cy + radius * sin((i + 1) * (unit_angle / 180) * M_PI)));
	}

	return circle;
}


inline void create_all_special_vertices() {


	auto circle = create_circle(100, 0, 0, number_of_vertices - 1);

	auto c1 = create_circle(20, 0, 0, number_of_vertices - 1); //the center
	for (auto i = 0; i < c1.size();i++) {
		cout << c1[i].first << " " << c1[i].second << " " << c1[(i + 1) % (number_of_vertices - 1)].first << " " << c1[(i + 1) % (number_of_vertices - 1)].second << endl;
	}

	for (int i = 0; i < number_of_vertices - 1;i++) { //the rest of a star
		c1 = create_circle(20, circle[i].first, circle[i].second, number_of_vertices - 1);
		for (int j = 0; j < c1.size();j++) {
			cout << c1[j].first << " " << c1[j].second << " " << c1[(j + 1) % (number_of_vertices - 1)].first << " " << c1[(j + 1) % (number_of_vertices - 1)].second << endl;
		}
	}
	//file.close();
}


int main()
{
	create_all_special_vertices();
}
