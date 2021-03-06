#pragma once

#include <cmath>
#include <memory>
#include <iterator>
#include <iostream>
#include <vector>
#include <list>
#include <algorithm>
#include <set>
#include <unordered_map>
#include <fstream>
#include <string>

#include "triangulation.hpp"
#include <queue>
#include <unordered_set>

#include <boost/geometry.hpp>
#include <boost/geometry/geometries/polygon.hpp>
#include <boost/geometry/geometries/adapted/boost_tuple.hpp>
#include <boost/geometry/algorithms/assign.hpp>

BOOST_GEOMETRY_REGISTER_BOOST_TUPLE_CS(cs::cartesian)

using namespace std;

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

#ifndef INF
#define INF 100000000
#endif // !INF

#ifndef EPSILON
#define EPSILON 0.0001
#endif

#define x first
#define y second

inline bool print_bool = false;

using array_4D = vector<vector<vector<vector<bool> > > >;

struct Vertex;
struct Edge;
struct Face;

/// <summary>
/// Class to store vertex, containing information about edge <c>to_</c> going into it
/// </summary>
struct Vertex {
	Edge* to_;

	int index_ = 0;

	double x_, y_;

	pair<double, double> shift_first = make_pair(0, 0);
	pair<double, double> shift_second = make_pair(0, 0);

	Vertex() {}

	Vertex(double x, double y) : x_(x), y_(y) {}

	//it is really good to be the opposite one because when you create new vertex 
	//from the edge side then it is good to go from the intersection opposite side
	Vertex(Edge* to) : to_(to) {}

	bool operator==(const Vertex& a) {
		return x_ == a.x_ && y_ == a.y_;
	}

	bool operator!=(const Vertex& a) {
		return !(*this == a);
	}

	bool operator==(Vertex* a) {
		return a->x_ == x_ && a->y_ == y_;
	}

	bool operator!=(Vertex* a) {
		return !(this == a);
	}


	bool operator==(const Vertex* a) {
		return a->x_ == x_ && a->y_ == y_;
	}

	bool operator!=(const Vertex* a) {
		return !(this == a);
	}
};

/// <summary>
/// Structure to store a face containing about one edge incident to it
/// </summary>
struct Face {
	Edge* edge_;

	Face() {}

	Face(Edge* edge) : edge_(edge) {}
};

/// <summary>
/// Structure to store the edge containing information about <c>next_</c>, <c>prev_</c>, <c>opposite_</c> edges and vertices and face incident to it.
/// </summary>
struct Edge {
	int index_ = 0;

	Edge* next_;
	Edge* prev_;
	Edge* opposite_;

	vector<shared_ptr<Vertex> > vertices_;

	shared_ptr<Face> face_;

	Edge() {}

	Edge(Edge* next, Edge* prev, Edge* opposite, vector<shared_ptr<Vertex> > vertices, shared_ptr<Face> face, int index) :
		next_(next), prev_(prev), opposite_(opposite), vertices_(vertices), face_(face), index_(index) {}

	bool operator==(const Edge& a) {
		return (vertices_[0] == a.vertices_[0] && vertices_.back() == a.vertices_.back());
	}

	bool operator!=(const Edge& a) {
		return !(*this == a);
	}

};

/// <summary>
/// Main class to store all the information about graph, edges, vertices, ...
/// </summary>
struct graph {

	/*normal part*/
	int number_of_vertices = 0; //just real vertices

	vector<Vertex> outer_vertices{Vertex(336, 341), Vertex(310, -337), Vertex(-329, -283), Vertex(-302, 297)}; 

	int realized = 0;
	bool done = false;

	int counter = 0;

	vector< pair<double, double> > most_away;

	list<Edge> edges;

	shared_ptr<Face> outer_face;

	ofstream output_file;

	vector<pair<double, double> > vertices_;

	vector<pair<double, double> > coordinates_of_special_vertices;

	int index;

	void close_files() {
		output_file.close();
	}

	graph(int n, int ii) {
		index = ii;

		number_of_vertices = n;
		outer_face = make_shared<Face>();

		starts.resize(number_of_vertices);
		for (int i = 0; i < number_of_vertices;i++) {
			starts[i].resize(number_of_vertices);
		}

		blocked.resize(number_of_vertices);
		for (int i = 0; i < number_of_vertices;i++) {
			blocked[i].resize(number_of_vertices);
			for (int j = 0; j < number_of_vertices;j++) {
				blocked[i][j].resize(number_of_vertices);
				for (int k = 0; k < number_of_vertices;k++) {
					blocked[i][j][k].resize(number_of_vertices);
					for (int l = 0; l < number_of_vertices;l++) {
						blocked[i][j][k][l] = false;
					}
				}
			}
		}

		string string_index = index < 10 ? "0" + to_string(index) : to_string(index);
		auto output_path = "../VizualizerWPF/data/graph"
			+ to_string(n) + "_" + string_index + ".txt";
		output_file.open(output_path);
	}

	void add_edge(
		shared_ptr<Vertex> a,
		shared_ptr<Vertex> b,
		shared_ptr<Face> face,
		int a_index,
		int b_index,
		pair<double, double> previous_vertex,
		pair<double, double> next_vertex,
		bool outer_face_bool = false);
	void add_vertex(Edge* edge);

	void delete_edge_back(bool outer_face_bool = false);
	void delete_vertex(Vertex* a);

	//void redirect_previous_segment(int a, int b, shared_ptr<Vertex> destination);
	shared_ptr<Vertex> tearup_lines_in_half(
		Edge* edge,
		vector<shared_ptr<Vertex> >& a_half,
		vector<shared_ptr<Vertex> >& b_half
	);

	vector<shared_ptr<Vertex> > find_path_through_triangulation(
		shared_ptr<Vertex> a,
		shared_ptr<Vertex> b,
		shared_ptr<Face> face,
		int a_index,
		int b_index,
		pair<double, double> shift_previous,
		pair<double, double> shift_next,
		bool outer_face_bool = false
	);
	/*finger print part*/

	//number of edges indexer
	/// <summary>
	/// Vector to store (pointers on) edges. 
	/// </summary>
	vector<Edge*> segments; 

	/// <summary>
	/// 4D array to check which edges already intersect	
	/// </summary>
	array_4D blocked; 

	/// <summary>
	/// Array which for given pair of vertice i, j return the index in vector <c>segments</c>
	///  so the "dummy" edge ("dummy" vertex) where to start (end) 
	/// </summary>
	vector<vector<int> > starts;

	// to store canonic fingerprints

	void create_special_vertex(int index, int x, int y);
	void recolor_fingerprint(const string& rotation);
	void create_base_star();
	void create_all_special_vertices();
	void find_the_way_to_intersect(int s_index, int t_index, int a, int b);
	//void intersect();
	//bool is_correct_fingerprint(const string& fingerprint);
	void create_all_possible_drawings();

	void write_coordinates();

	vector<shared_ptr<Vertex> > find_shortest_path(const vector<vector<double> >& distances, vector<shared_ptr<Vertex> >& face_vertices);

	//string find_canonic_fingerprint(const string& fingerprint);

	void update_most_away(vector<pair<double, double>  > vertices);

};


/// <summary>
/// Function to detect whether <c>line1</c> and <c>line2</c> intersect.
/// </summary>
/// <param name="line1"></param>
/// <param name="line2"></param>
/// <returns></returns>
inline bool if_two_segmetns_intersects(pair<shared_ptr<Vertex>, shared_ptr<Vertex> > line1, pair<shared_ptr<Vertex>, shared_ptr<Vertex> > line2)
{
	auto denominator = (line2.second->y_ - line2.first->y_) * (line1.second->x_ - line1.first->x_) - (line2.second->x_ - line2.first->x_) * (line1.second->y_ - line1.first->y_);

	auto numT = -line1.first->x_ * (line2.second->y_ - line2.first->y_) + line1.first->y_ * (line2.second->x_ - line2.first->x_) +
		line2.first->x_ * (line2.second->y_ - line2.first->y_) - line2.first->y_ * (line2.second->x_ - line2.first->x_);
	auto numS = -line1.first->x_ * (line1.second->y_ - line1.first->y_) + line1.first->y_ * (line1.second->x_ - line1.first->x_) +
		line2.first->x_ * (line1.second->y_ - line1.first->y_) - line2.first->y_ * (line1.second->x_ - line1.first->x_);


	auto s = numS / denominator;
	auto t = numT / denominator;

	if (abs(denominator) < EPSILON && (s >= -EPSILON && s <= 1 + EPSILON) && (t >= -EPSILON && t <= 1 + EPSILON))
		return true;
	return false;
}


/// <summary>
/// Function to set distance Euclidian distance for all pair of points i, j in <c>points</c>
/// into matrix <c>distances</c>.
/// </summary>
/// <param name="points"></param>
/// <param name="distances"></param>
inline void create_coordinates(const vector<shared_ptr<Vertex> >& points, vector<vector<double> >& distances) {

	for (int i = 0; i < points.size();i++) {
		for (int j = 0; j < points.size();j++) {
			distances[i][j] = sqrt((points[i]->x_ - points[j]->x_) * (points[i]->x_ - points[j]->x_) + (points[i]->y_ - points[j]->y_) * (points[i]->y_ - points[j]->y_));
		}
	}
}


/// <summary>
/// Standart dijsktra algorithm returning shortest paths from vertex 0 
/// with paths stored in vector <c>parent</c>.
/// </summary>
/// <param name="points"></param>
/// <param name="distances"></param>
/// <param name="parent"></param>
inline void dijsktra(vector<shared_ptr<Vertex> >& points, vector<vector<double> > distances, vector<int>& parent) {

	vector<double> shortest;
	shortest.resize(points.size());

	priority_queue<pair<double, int>, vector<pair<double, int> >, greater<pair<double, int> > > pq;
	vector<bool> visited;
	visited.resize(points.size());

	for (int i = 0; i < points.size(); i++) {
		shortest[i] = INF;
	}

	shortest[0] = 0;
	pq.push(make_pair(shortest[0], 0));

	while (!pq.empty()) {
		auto top = pq.top();pq.pop();

		if (visited[top.second]) continue;
		visited[top.second] = true;

		for (int w = 0; w < points.size();w++) {
			if (top.second != w) {

				if (shortest[w] > shortest[top.second] + distances[top.second][w]) {
					shortest[w] = top.first + distances[top.second][w];
					pq.push(make_pair(shortest[w], w));
					parent[w] = top.second;
				}
			}
		}
	}
}


inline void print_graph(graph* g) {
	cout << "number of edges: " << g->edges.size() << endl;

	int i = 0;
	for (auto it : g->edges) {
		cout << it.vertices_[0] << ":from to:" << it.vertices_.back() << " face:" << it.face_ << " prev:" << it.prev_->index_ << " next:" << it.next_->index_
			<< " opp:" << ((it.opposite_ == nullptr) ? -1 : it.opposite_->index_) << " index:" << it.index_ <<  " s: " << i << endl;
		i++;
	}

}

/// <summary>
/// Function to find shortest path between vertices (<c>all_vertices</c>) 0 and 1.
/// </summary>
/// <param name="distances"></param>
/// <param name="all_vertices"></param>
/// <returns></returns>
inline vector<shared_ptr<Vertex> > graph::find_shortest_path(const vector<vector<double> > & distances, vector<shared_ptr<Vertex > >& all_vertices) {


	/*
	for (int i = 0; i < distances.size();i++) {
		for (int j = 0; j < distances[i].size();j++) {
			cout << distances[i][j] << " ";
		}
		cout << endl;
	}
	*/

	vector<int> parent;
	parent.resize(all_vertices.size());
	dijsktra(all_vertices, distances, parent);

	vector<shared_ptr<Vertex> > result;

	int v = 1;
	while (v != 0) {
		result.push_back(all_vertices[v]);
		v = parent[v];
	}
	result.push_back(all_vertices[0]);
	return result;
}

/// <summary>
/// Function to create "dummy" vertices for one vertex.
/// </summary>
/// <param name="radius"></param>
/// <param name="cx"></param>
/// <param name="cy"></param>
/// <param name="n"></param>
/// <returns></returns>
inline vector<pair<double, double> > create_circle(double radius, double cx, double cy, int n) {
	vector<pair<double, double> > circle;

	double unit_angle = (double)360 / n;

	for (int i = 0; i < n;i++) {
		circle.push_back(make_pair((int)(cx + radius * cos((i + 1) * (unit_angle / 180) * M_PI)),(int)(cy + radius * sin((i + 1) * (unit_angle / 180) * M_PI))));
	}

	/*
	for (int i = 0; i < n;i++) {
		cout << circle[i].x << " " << circle[i].y << endl;
	}
	*/
	return circle;
}

inline double distance(pair<double, double> a) {
	return sqrt((a.x ) * (a.x) + (a.y) * (a.y ));
}

inline double distance(pair<double, double> a, pair<double, double> b) {
	return sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
}

inline double distance(shared_ptr<Vertex> a, shared_ptr<Vertex> b) {
	return sqrt((a->x_ - b->x_) * (a->x_ - b->x_) + (a->y_ - b->y_) * (a->y_ - b->y_));
}



/// <summary>
/// Function to create convex hull of  <c>vertices</c>.
/// </summary>
/// <param name="vertices"></param>
/// <returns></returns>
inline vector<pair<double, double> > make_convex_hull(vector<pair<double, double> > vertices) {
	typedef boost::tuple<double, double> point;
	typedef boost::geometry::model::polygon<point> polygon;

	polygon poly;

	
	vector<point> points;
	
	for(int i = 0; i < vertices.size();i++){
		points.push_back(
			boost::make_tuple<double, double>(vertices[i].x, vertices[i].y));
	}

	boost::geometry::assign_points(poly, points);

	polygon hull;
	boost::geometry::convex_hull(poly, hull);

	/*
	using boost::geometry::dsv;
	std::cout
		<< "polygon: " << dsv(poly) << std::endl
		<< "hull: " << dsv(hull) << std::endl;
	*/
	vector<pair<double, double> > result;
	
	for (int i = 0; i < hull.outer().size();i++) {
		result.push_back(make_pair(hull.outer()[i].get_head(), hull.outer()[i].get_tail().get_head()));
	}
	return result;
}

/// <summary>
/// Check whether the two pairs are "almost" the same.
/// </summary>
/// <param name="a"></param>
/// <param name="b"></param>
/// <returns></returns>
inline bool compare(pair<double, double> a, pair<double, double> b) {
	if (abs(a.x - b.x) < EPSILON && abs(a.y - b.y) < EPSILON)
		return true;
	return false;
}

/// <summary>
/// Function to update the convex hull of vertices of a graph.
/// </summary>
/// <param name="vertices"></param>
inline void graph::update_most_away(vector<pair<double, double> > vertices) {
	most_away = make_convex_hull(vertices);
}

/// <summary>
/// Fucntion to count the determinant.
/// </summary>
/// <param name="vec1"></param>
/// <param name="vec2"></param>
/// <returns></returns>
inline double det(pair<double, double> vec1, pair<double, double> vec2) {
	return vec1.x * vec2.y - vec1.y * vec2.x;
}

//shifting to the right 


/*		 
	 <----
	|	 ^
	|	 | \\shift -->
    |    | 
	v --->
*/

/// <summary>
/// Function to generate shift perpendicular to given vector <c>vect</c>
///  in order to be able to move same vertices to distinguish them.
/// </summary>
/// <param name="vertex"></param>
/// <param name="vect"></param>
/// <returns></returns>
inline pair<double, double> get_shift(shared_ptr<Vertex> vertex, pair<double, double> vect) { 

	pair<double, double> first_neg_vect;
	pair<double, double> second_neg_vect;

	//negative one is to the right

	first_neg_vect = make_pair(-vect.y, vect.x);
	second_neg_vect = make_pair(vect.y, -vect.x);
	

	if (det(vect, first_neg_vect) < 0) { //negative der is to the right
		return make_pair(10 * EPSILON * first_neg_vect.x, 10 * EPSILON * first_neg_vect.y); // 10* EPSILON to be recognizable
	}
	else {
		return make_pair(10 * EPSILON * second_neg_vect.x, 10 * EPSILON * second_neg_vect.y);
	}
}


/// <summary>
/// Get vertex perpendicular to <c>vect</c> from <c>vertex</c> and
/// on the right side depending on the clockwise, counterclockwise order of vertices.
/// </summary>
/// <param name="vertex"></param>
/// <param name="vect"></param>
/// <returns></returns>
inline pair<double, double> find_vertex_in_right_direction(shared_ptr<Vertex> vertex, pair<double, double> vect) {

	auto shift = get_shift(vertex, vect);
	return make_pair(100 * (shift.x / distance(shift)), 100 * ((shift.y / distance(shift))));

}


/// <summary>
/// Get value of linear function f(x, y) = ax+by+c.
/// </summary>
/// <param name="a"></param>
/// <param name="b"></param>
/// <param name="c"></param>
/// <param name="point"></param>
/// <returns></returns>
inline double get_value(int a, int b, int c, shared_ptr<Vertex> point) {
	return a * point->x_ + b * point->y_ + c;
}

/// <summary>
/// Function to check whether <c>asked_point</c> and <c>on_right_side</c> points
///  are on the same side of line given by <c>point</c> and vector <c>vect</c>.
/// </summary>
/// <param name="asked_point"></param>
/// <param name="point"></param>
/// <param name="vect"></param>
/// <param name="on_right_side"></param>
/// <returns></returns>
inline bool is_on_right_side_of(shared_ptr<Vertex>asked_point, shared_ptr<Vertex> point, pair<double, double> vect, shared_ptr<Vertex> on_right_side) {
	
	double a = vect.x;
	double b = vect.y;
	double c = -(a * point->x_ + b * point->y_);
	
	if (get_value(a, b, c, asked_point) * get_value(a, b, c, on_right_side) > 0)
		return true;
	return false;
}



/*
inline bool check_if_its_line_between_inner_and_outer_part(int size, int index1, int index2) {
	vector<pair<int, int> > indices_between{ make_pair(0, size - 5), make_pair(0, size - 1), make_pair(size - 6, size - 5), make_pair(size - 6, size - 1) };

	if (index1 > index2)
		swap(index1, index2);
		
	pair<int, int> indices = make_pair(index1, index2);

	for (int i = 0; i < indices_between.size(); i++) {
		if (indices_between[i] == indices)
			return true;
	}
	return false;
}
*/

inline bool are_collinear(shared_ptr<Vertex> v1, shared_ptr<Vertex> v2, shared_ptr<Vertex> v3) {

	/* Calculation the area of
	triangle.We have skipped
	multiplication with 0.5 to
	avoid floating point computations */

	double a = v1->x_ * (v2->y_ - v3->y_) + v2->x_ * (v3->y_ - v1->y_) + v3->x_ * (v1->y_ - v2->y_);

	if (abs(a) < EPSILON)
		return true;
	return false;
}

/// <summary>
/// Function to find correct way (preserving crossing properties) from vertex <c>a</c>
/// to vertex <c>b</c> through the <c>face</c>.
/// </summary>
/// <param name="a"></param>
/// <param name="b"></param>
/// <param name="face"></param>
/// <param name="a_index"></param>
/// <param name="b_index"></param>
/// <param name="shift_previous"></param>
/// <param name="shift_next"></param>
/// <param name="outer_face_bool"></param>
/// <returns></returns>
inline vector<shared_ptr<Vertex> > graph::find_path_through_triangulation(shared_ptr<Vertex> a, shared_ptr<Vertex> b,
	shared_ptr<Face> face, int a_index, int b_index,
	pair<double, double> shift_previous,
	pair<double, double> shift_next, bool outer_face_bool) {
	Edge* toa = nullptr, * tob = nullptr, * froma = nullptr, * fromb = nullptr;

	toa = a->to_;
	froma = toa->next_;

	tob = b->to_;
	fromb = tob->next_;

	/* triangulation */

	vector<shared_ptr<Vertex> > vertices;

	if (!outer_face_bool) {

		bool second_outer_face_bool = true;

		update_most_away(vertices_);

		vector<shared_ptr<Vertex> > faces_vertices;
		auto start = face->edge_;
		auto cur = start->next_;
		faces_vertices.insert(faces_vertices.end(), start->vertices_.begin(), start->vertices_.end() - 1);

		while (cur != start) {
			faces_vertices.insert(faces_vertices.end(), cur->vertices_.begin(), cur->vertices_.end() - 1);
			cur = cur->next_;
		}

		//find_vertex_shortest to outside layer
		vector<vector<double> > outside_distances; outside_distances.resize(faces_vertices.size() + outer_vertices.size());
		for (int i = 0; i < outside_distances.size(); i++)
			outside_distances[i].resize(outside_distances.size());

		vector<shared_ptr<Vertex> > outside_points;
		for (int i = 0; i < outer_vertices.size(); i++)
			outside_points.push_back(make_shared<Vertex>(outer_vertices[i]));
		outside_points.insert(outside_points.end(), faces_vertices.begin(), faces_vertices.end());
		create_coordinates(outside_points, outside_distances);

		double mn = INF; int mn_index = -1;
		for (int i = 0; i < outer_vertices.size(); i++) {
			for (int j = outer_vertices.size(); j < outside_distances[i].size(); j++) {
				if (outside_distances[i][j] < mn) {
					mn = outside_distances[i][j];
					mn_index = j - outer_vertices.size();
				}
			}
		}

		rotate(faces_vertices.begin(), faces_vertices.begin() + mn_index, faces_vertices.end());

		//if (*start->vertices_[0] == Vertex(0, 0))
		//	rotate(faces_vertices.begin(), faces_vertices.begin() + 1, faces_vertices.end());

		faces_vertices.push_back(faces_vertices[0]);

		vector<pair<double, double> > coords;
		vector<pair<double, double> > indices_coords;

		vector<bool> local_most_away; local_most_away.resize(most_away.size());

		coords.push_back(make_pair(faces_vertices[0]->x_, faces_vertices[0]->y_)); indices_coords.push_back(make_pair(faces_vertices[0]->x_, faces_vertices[0]->y_));
		for (int i = 1; i < faces_vertices.size(); i++) {
			if (*faces_vertices[i] != *faces_vertices[i - 1]) {
				if ((faces_vertices[i]->index_ == -1) &&
					(compare(make_pair(faces_vertices[i]->x_ - faces_vertices[i - 1]->x_, faces_vertices[i]->y_ - faces_vertices[i - 1]->y_), faces_vertices[i]->shift_first)
					||
					compare(make_pair(faces_vertices[i]->x_ - faces_vertices[i - 1]->x_, faces_vertices[i]->y_ - faces_vertices[i - 1]->y_), faces_vertices[i]->shift_second))) {
					auto shift = get_shift(
						faces_vertices[i],
						make_pair(faces_vertices[i]->x_ - faces_vertices[i - 1]->x_,
							faces_vertices[i]->y_ - faces_vertices[i - 1]->y_));

					indices_coords.push_back(make_pair(
						faces_vertices[i]->x_ + shift.x,
						faces_vertices[i]->y_ + shift.y
					));
				}
				else {
					indices_coords.push_back(make_pair(faces_vertices[i]->x_, faces_vertices[i]->y_));
				}
				coords.push_back(make_pair(faces_vertices[i]->x_, faces_vertices[i]->y_));
			}
			for (int j = 0; j < most_away.size(); j++) {
				if (abs(faces_vertices[i]->x_ - most_away[j].x) < EPSILON && abs(faces_vertices[i]->y_ - most_away[j].y) < EPSILON) {
					local_most_away[j] = true;
				}
			}

		}

		for (int j = 0; j < most_away.size(); j++) {
			if (!local_most_away[j]) {
				second_outer_face_bool = false;
				break;
			}
		}

		/* checking outer face */

		if (second_outer_face_bool) {
			/* minimum rotation of outer part*/
			double mn = INF;
			int index_min;
			for (int i = 0; i < outer_vertices.size(); i++) {
				if ((outer_vertices[i].x_ - faces_vertices.back()->x_) * (outer_vertices[i].x_ - faces_vertices.back()->x_) +
					(outer_vertices[i].y_ - faces_vertices.back()->y_) * (outer_vertices[i].y_ - faces_vertices.back()->y_) < mn) {
					index_min = i;
					mn = (outer_vertices[i].x_ - faces_vertices.back()->x_) * (outer_vertices[i].x_ - faces_vertices.back()->x_) +
						(outer_vertices[i].y_ - faces_vertices.back()->y_) * (outer_vertices[i].y_ - faces_vertices.back()->y_);
				}
			}

			rotate(outer_vertices.begin(), outer_vertices.begin() + index_min, outer_vertices.end());


			for (int i = 0; i < outer_vertices.size(); i++) {
				coords.push_back(make_pair(outer_vertices[i].x_, outer_vertices[i].y_));
				indices_coords.push_back(make_pair(outer_vertices[i].x_, outer_vertices[i].y_));
			}
			coords.push_back(make_pair(outer_vertices[0].x_, outer_vertices[0].y_));
			indices_coords.push_back(make_pair(outer_vertices[0].x_, outer_vertices[0].y_));

		}

		vector<vector<pair<double, double> > > polygon{ indices_coords };

		std::vector<int> indices = mapbox::earcut<int>(polygon);
		

		// rename duplicate vertices if neededs
		if (second_outer_face_bool) {
			int size = coords.size();
			for (int i = 0; i < indices.size(); i++) {
				indices[i] = indices[i] == size - 5 ? size - 1 : indices[i];
				indices[i] = indices[i] == size - 6 ? 0 : indices[i];
			}
		}
		/*
		else {
			int size = coords.size();
			for (int i = 0; i < indices.size(); i++) {
				indices[i] = indices[i] == size - 1 ? 0 : indices[i];
			}
		}
		*/
		

		set<pair<double, double> > visited_vertices;

		auto mids = vector<shared_ptr<Vertex> >();
		for (int i = 0; i < indices.size(); i += 3){

			for (int j = 0; j < 3; j++) {
				auto vertex = make_shared<Vertex>((coords[indices[i + j]].x + coords[indices[i + ((j + 1) % 3)]].x) / 2,
					(coords[indices[i + j]].y + coords[indices[i + ((j + 1) % 3)]].y) / 2);
				if (!visited_vertices.count(make_pair(vertex->x_, vertex->y_)) //should be first one there
					&&
					!(second_outer_face_bool && 
						((min(indices[i + j], indices[i + ((j + 1) % 3)]) == 0) 
							&& (max(indices[i + j], indices[i + ((j + 1) % 3)]) == coords.size() - 7)))
					&&
					!(second_outer_face_bool &&
						((min(indices[i + j], indices[i + ((j + 1) % 3)]) == coords.size() - 4)
						&& (max(indices[i + j], indices[i + ((j + 1) % 3)]) == coords.size() - 1)))
					&&
					!(!second_outer_face_bool && (abs(indices[i + j] - indices[i + ((j + 1) % 3)]) == coords.size() - 2))
					&&
					(abs(indices[i + j] - indices[i + ((j + 1) % 3)]) != 1)
					) {
					visited_vertices.insert(make_pair(vertex->x_, vertex->y_));
					mids.push_back(vertex);
				}	
			}
		}

		vector<shared_ptr<Vertex> > all_vertices{ a };
		all_vertices.push_back(b);
		all_vertices.insert(all_vertices.end(), mids.begin(), mids.end());


		vector<vector<double> > distances;
		distances.resize(all_vertices.size());
		for (int i = 0; i < all_vertices.size(); i++) {
			for (int j = 0; j < all_vertices.size(); j++) {
				distances[i].push_back(INF);
			}
		}

		/*
		vector<vector<double> > real_distances;
		real_distances.resize(all_vertices.size());
		for (int i = 0; i < all_vertices.size(); i++) {
			real_distances[i].resize(all_vertices.size());
		}

		create_coordinates(all_vertices, real_distances);
		*/

		//visited_vertices.clear();

		auto cmp = [](pair<pair<double, double>, int> a, pair<pair<double, double>, int> b) {return a.first < b.first; };
		set<pair<pair<double, double>, int>, decltype(cmp)> visited_vertices2(cmp);

		int mid_index = 0;

		for (int i = 0; i < indices.size(); i += 3){ 
			
			vector<int> triangle_indices;
			bool contains_a = false;
			bool contains_b = false;

			for (int j = 0; j < 3; j++) {
				auto vertex = make_shared<Vertex>((coords[indices[i + j]].x + coords[indices[i + ((j + 1) % 3)]].x) / 2,
					(coords[indices[i + j]].y + coords[indices[i + ((j + 1) % 3)]].y) / 2);
				if (!(second_outer_face_bool &&
						((min(indices[i + j], indices[i + ((j + 1) % 3)]) == 0)
							&& (max(indices[i + j], indices[i + ((j + 1) % 3)]) == coords.size() - 7)))
					&&
					!(second_outer_face_bool &&
						((min(indices[i + j], indices[i + ((j + 1) % 3)]) == coords.size() - 4)
							&& (max(indices[i + j], indices[i + ((j + 1) % 3)]) == coords.size() - 1)))
					&&
					!(!second_outer_face_bool && (abs(indices[i + j] - indices[i + ((j + 1) % 3)]) == coords.size() - 2))
					&&
					(abs(indices[i + j] - indices[i + ((j + 1) % 3)]) != 1)
					) {

					if (visited_vertices2.count(make_pair(make_pair(vertex->x_, vertex->y_), -1))) {
						triangle_indices.push_back(visited_vertices2.find(make_pair(make_pair(vertex->x_, vertex->y_), -1))->second);
					}
					else {
						visited_vertices2.insert(make_pair(make_pair(vertex->x_, vertex->y_), mid_index));
						triangle_indices.push_back(mid_index);
						mid_index++;
					}
				}
				if (distance(coords[indices[i + (j % 3)]], make_pair(a->x_, a->y_)) < EPSILON) {
					contains_a = true;
				}

				if (distance(coords[indices[i + (j % 3)]], make_pair(b->x_, b->y_)) < EPSILON) {
					contains_b = true;
				}
			}

			for(int u = 0;u < triangle_indices.size();u++){
				for(int v = u + 1;v < triangle_indices.size();v++){
					distances[triangle_indices[u]+2][triangle_indices[v]+2] = distance(mids[triangle_indices[u]], mids[triangle_indices[v]]);
					distances[triangle_indices[v]+2][triangle_indices[u]+2] = distance(mids[triangle_indices[v]], mids[triangle_indices[u]]);
				}
			}

			for (int u = 0; u < triangle_indices.size(); u++) {
				if (contains_a) {
					distances[triangle_indices[u] + 2][0] = distance(mids[triangle_indices[u]], a);
					distances[0][triangle_indices[u] + 2] = distance(a, mids[triangle_indices[u]]);
				}
				if(contains_b) {
					distances[triangle_indices[u] + 2][1] = distance(mids[triangle_indices[u]], b);
					distances[1][triangle_indices[u] + 2] = distance(b, mids[triangle_indices[u]]);
				}
			}

		}


		/*finding if points can be connected with line*/

		for (int i = 0; i < indices.size(); i += 3) {
			for (int j = 0; j < 3; j++) {
				if (((a->x_ == coords[indices[i + j]].x) && (a->y_ == coords[indices[i + j]].y))
					&&
					((b->x_ == coords[indices[i + ((j + 1) % 3)]].x) && (b->y_ == coords[indices[i + ((j + 1) % 3)]].y))
					) {
					distances[0][1] = distance(a, b) - EPSILON; //substracting epsilon in order to choose a -- b instead of a -- mid -- b which is also possible
					distances[1][0] = distance(b, a) - EPSILON;
				}
			}
		}


		for (int i = 0; i < indices.size(); i += 3) {
			for (int j = 0; j < 3; j++) {
				if (((b->x_ == coords[indices[i + j]].x) && (b->y_ == coords[indices[i + j]].y))
					&&
					((a->x_ == coords[indices[i + ((j + 1) % 3)]].x) && (a->y_ == coords[indices[i + ((j + 1) % 3)]].y))
					) {
					distances[0][1] = distance(a, b) - EPSILON;
					distances[1][0] = distance(b, a) - EPSILON;
				}
			}
		}

		/*choosing the most in the one direction one*/
		
		auto next_vertex = make_shared<Vertex>();

		next_vertex->x_ = b->x_ + shift_next.x;//(second_outer_face_bool ? shift_next.x : -shift_next.x);
		next_vertex->y_ = b->y_ + shift_next.y;//(second_outer_face_bool ? shift_next.y : -shift_next.y);
		if (b->index_ == -1) { // (b->shift_epsilon.x != 0 || b->shift_epsilon.y != 0) instead of b->index_ == -1
			if (!is_on_right_side_of(a, b, shift_next, next_vertex)) {
				distances[1][0] = INF;
				distances[0][1] = INF;
			}
			for (int i = 2; i < distances.size(); i++) {
				if (!is_on_right_side_of(mids[i - 2], b, shift_next, next_vertex)) { // coordinates_of_special_vertices[b_index - 1]
					distances[1][i] = INF;
					distances[i][1] = INF;
				}
			}
		}
		// a also wants right direction

		auto previous_vertex = make_shared<Vertex>();
		previous_vertex->x_ = a->x_ + shift_previous.x;//(second_outer_face_bool ? shift_previous.x : -shift_previous.x);
		previous_vertex->y_ = a->y_ + shift_previous.y;//(second_outer_face_bool ? shift_previous.y : -shift_previous.y);
		if (a->index_ == -1) { // (b->shift_epsilon.x != 0 || b->shift_epsilon.y != 0) instead of b->index_ == -1
			if (!is_on_right_side_of(b, a, shift_previous, previous_vertex)) {
				distances[0][1] = INF;
				distances[1][0] = INF;
			}
			for (int i = 2; i < distances.size(); i++) {
				if (!is_on_right_side_of(mids[i-2], a, shift_previous, previous_vertex)) {
					distances[0][i] = INF;
					distances[i][0] = INF;
				}
			}
		}

		vertices = find_shortest_path(distances, all_vertices);

		/*remove duplicates because mid can be in mids and as well normal vertex*/
		vector<shared_ptr<Vertex> > temp_uniques; temp_uniques.push_back(vertices[0]);
		for (int i = 1; i < vertices.size(); i++) {
			if (*vertices[i] != *vertices[i - 1])
				temp_uniques.push_back(vertices[i]);
		}
		vertices = temp_uniques;

		/*resolve knots */
		
		/*
		for (int i = 3; i < vertices.size(); i++) {
			if (if_two_segmetns_intersects(make_pair(vertices[i - 3], vertices[i - 2]), make_pair(vertices[i - 1], vertices[i])))
				swap(vertices[i - 2], vertices[i - 1]);
		}
		*/

		/*remove collinear ones*/
			
		
		vector<shared_ptr<Vertex> > non_colinear; 
		non_colinear.push_back(vertices[0]); non_colinear.push_back(vertices[1]); //there are always at least two vertices
		for (int i = 2; i < vertices.size(); i++) {
			if (are_collinear(non_colinear[non_colinear.size()-2], non_colinear[non_colinear.size()-1], vertices[i])) {
				non_colinear.pop_back(); non_colinear.push_back(vertices[i]); //skip when collinear
			}
			else {
				non_colinear.push_back(vertices[i]);
			}
		}
		vertices = non_colinear;
		
		return vertices;

		//print_graph(this);
		

	}
	else {
		return vector<shared_ptr<Vertex> >{ b, a };
	}


}

/// <summary>
/// Function to add edge between <c>a</c> and <c>b</c>.
/// </summary>
/// <param name="a"></param>
/// <param name="b"></param>
/// <param name="face"></param>
/// <param name="a_index"></param>
/// <param name="b_index"></param>
/// <param name="shift_previous"></param>
/// <param name="shift_next"></param>
/// <param name="outer_face_bool"></param>
inline void graph::add_edge(shared_ptr<Vertex> a, shared_ptr<Vertex> b, shared_ptr<Face> face, int a_index, int b_index, pair<double, double> shift_previous, pair<double, double> shift_next, bool outer_face_bool) {

	Edge* toa = nullptr, * tob = nullptr, * froma = nullptr, * fromb = nullptr;

	toa = a->to_;
	froma = toa->next_;

	tob = b->to_;
	fromb = tob->next_;
	
	auto vertices = find_path_through_triangulation(a, b, face, a_index, b_index, shift_previous, shift_next, outer_face_bool);

	for (int i = 1; i < vertices.size() - 1; i++) {
		vertices_.push_back(make_pair(vertices[i]->x_, vertices[i]->y_));
	}

	auto new_face = !outer_face_bool ? make_shared<Face>() : outer_face; //new face or old face when creating star

	reverse(vertices.begin(), vertices.end());
	Edge ab_edge(fromb, toa, nullptr, vertices, face, a_index*100 + b_index); //edge from a to b
	edges.push_back(ab_edge); //number_of_edges++;
	Edge* ab_edge_ptr = &edges.back();
	segments.push_back(ab_edge_ptr);

	//print_graph(this);

	reverse(vertices.begin(), vertices.end());
	Edge ba_edge(froma, tob, ab_edge_ptr, vertices, new_face, b_index*100 + a_index); //edge from b to a
	edges.push_back(ba_edge); //number_of_edges++;
	Edge* ba_edge_ptr = &edges.back();
	segments.push_back(ba_edge_ptr);

	//print_graph(this);

	ab_edge_ptr->opposite_ = ba_edge_ptr; //setting opposite edge that has been already made and face edge
	new_face->edge_ = ba_edge_ptr;

	/*if it throws an exception it is allright because we are tryint to connect unconnectable vertices*/
	froma->prev_ = ba_edge_ptr; //changing neighborhood edges
	toa->next_ = ab_edge_ptr;
	fromb->prev_ = ab_edge_ptr;
	tob->next_ = ba_edge_ptr;

	//print_graph(this);

	if (!outer_face_bool) { //because if it is outer face it is not needed so it can be quicker
		auto start_edge = ba_edge_ptr; //setting the face property to all edges around this face
		auto cur_edge = start_edge->next_;
		while (start_edge != cur_edge) {
			cur_edge->face_ = new_face;
			cur_edge = cur_edge->next_;
		}
	}

	face->edge_ = ab_edge_ptr;
}

/// <summary>
/// Function to tear up <c>edge</c> into two halves <c>a_half</c> and <c>b_half</c>.
/// </summary>
/// <param name="edge"></param>
/// <param name="a_half"></param>
/// <param name="b_half"></param>
/// <returns></returns>
inline shared_ptr<Vertex> graph::tearup_lines_in_half(Edge* edge, vector<shared_ptr<Vertex> >& a_half, vector<shared_ptr<Vertex> >& b_half) {

	a_half = vector<shared_ptr<Vertex> >(edge->vertices_.begin(), edge->vertices_.begin() + edge->vertices_.size() / 2);
	b_half = vector<shared_ptr<Vertex> >(edge->vertices_.begin() + edge->vertices_.size() / 2, edge->vertices_.end());


	auto a = a_half.back();
	auto b = b_half[0];

	auto new_vertex = make_shared<Vertex>(edge); //edge is going to this vertex //"edge" it is important because after adding vertex we add teh edge to the same direction (not face, face can be same anyway)
	new_vertex->index_ = ((edge->index_ / 100 == 0) || (edge->index_ % 100 == 0)) ? -1 : -2;
	new_vertex->x_ = (a->x_ + b->x_) / 2; new_vertex->y_ = (a->y_ + b->y_) / 2;

	new_vertex->shift_first = make_pair(new_vertex->x_ - a->x_, new_vertex->y_ - a->y_);
	new_vertex->shift_second = make_pair(new_vertex->x_ - b->x_, new_vertex->y_ - b->y_);

	vertices_.push_back(make_pair(new_vertex->x_, new_vertex->y_));

	a_half.push_back(new_vertex);
	reverse(a_half.begin(), a_half.end());

	b_half.push_back(new_vertex);
	rotate(b_half.rbegin(), b_half.rbegin() + 1, b_half.rend());

	

	return new_vertex;
}


/// <summary>
/// Creating new vertec on the <c>edge</c> as a new intersection.
/// </summary>
/// <param name="edge"></param>
inline void graph::add_vertex(Edge* edge) {

	auto opposite = edge->opposite_;

	vector<shared_ptr<Vertex> > a_half;
	vector<shared_ptr<Vertex> > b_half;

	tearup_lines_in_half(edge, a_half, b_half);

	edges.push_back(Edge(edge->next_, edge, opposite, b_half, edge->face_, edge->index_)); //new vertex to b, part of normal edge
	auto tob = &edges.back();
	segments.push_back(tob);

	edges.push_back(Edge(opposite->next_, opposite, edge, a_half, opposite->face_, opposite->index_)); //create from new_vertex to a, part of opposite
	auto toa = &edges.back();
	segments.push_back(toa);

	reverse(a_half.begin(), a_half.end());
	edge->vertices_ = a_half; //changing properties of edge
	edge->next_ = tob;
	edge->opposite_ = toa;

	reverse(b_half.begin(), b_half.end());
	opposite->vertices_ = b_half; //changing properties of opposite
	opposite->next_ = toa;
	opposite->opposite_ = tob;

}


/// <summary>
/// Deleting the last added edge.
/// </summary>
/// <param name="outer_face_bool"></param>
inline void graph::delete_edge_back(bool outer_face_bool) {

	auto edge = edges.back();
	auto opposite = *edge.opposite_;

	auto a = edge.vertices_[0];
	auto b = edge.vertices_.back();

	auto face = edge.face_;

	auto froma = opposite.next_;
	auto toa = edge.prev_;
	auto fromb = edge.next_;
	auto tob = opposite.prev_;

	/*update faces first!*/

	auto cur_edge = opposite.next_;
	if (!outer_face_bool) {
		while (opposite != *cur_edge) {
			cur_edge->face_ = face;
			cur_edge = cur_edge->next_;
		}
	}

	//print_graph(this);

	/*recconect edges*/
	froma->prev_ = toa;
	toa->next_ = froma;
	fromb->prev_ = tob;
	tob->next_ = fromb;

	/*update edges if there the bad ones*/
	a->to_ = toa;
	b->to_ = tob;

	face->edge_ = froma;

	outer_face = face; //just to end up with outer face

	/* delete the edge from list, pop from segments should be out of this function */

	for (int i = 1; i < edge.vertices_.size() - 1;i++) {
		vertices_.pop_back();
	}

	segments.pop_back();segments.pop_back();
	edges.pop_back();edges.pop_back();

	//number_of_edges -= 2;
}


/// <summary>
/// Deleting given intersection <c>vertex</c>.
/// </summary>
/// <param name="vertex"></param>
inline void graph::delete_vertex(Vertex* vertex) {

	// getting the right variables
	auto edge = vertex->to_;

	auto toa = edge->opposite_;
	auto tob = edge->next_;
	auto edge_opposite = tob->opposite_;

	auto a = toa->vertices_.back();
	auto b = tob->vertices_.back();

	// reconnecting edges and therefore removing the vertex

	vector<shared_ptr<Vertex> > former_vertices = edge->vertices_; former_vertices.pop_back();
	edge_opposite->vertices_.pop_back(); reverse(edge_opposite->vertices_.begin(), edge_opposite->vertices_.end());

	former_vertices.insert(former_vertices.end(), edge_opposite->vertices_.begin(), edge_opposite->vertices_.end());

	edge->vertices_ = former_vertices;reverse(former_vertices.begin(), former_vertices.end());
	edge_opposite->vertices_ = former_vertices; //reversed
	edge->next_ = tob->next_;
	edge_opposite->next_ = toa->next_;

	//setting opposites
	edge->opposite_ = edge_opposite;
	edge_opposite->opposite_ = edge;

	edge->face_->edge_ = edge;
	edge_opposite->face_->edge_ = edge_opposite;

	//removig the edges from the back of segments and edges
	//because of the order and recursive algorithm we know they are the last one

	vertices_.pop_back();

	segments.pop_back(); segments.pop_back();
	edges.pop_back(); edges.pop_back();

}


/// <summary>
/// Creating the "dummy" vertex (circle) representing real vertex.
/// </summary>
/// <param name="index"></param>
inline void graph::create_special_vertex(int index, int x, int y) {

	vector<shared_ptr<Vertex> > special_vertices;

	/*create vertices with coordinates*/
	for (int i = 0; i < number_of_vertices - 1;i++) {
		auto new_vertex = make_shared<Vertex>();
		new_vertex->index_ = index; //the real index of vertex
		new_vertex->x_ = x; new_vertex->y_ = y;
		special_vertices.push_back(new_vertex);
	}

	
	/* edges created */
	for (int i = 0; i < number_of_vertices - 1;i++) {
		auto first_vertex = special_vertices[i];
		auto second_vertex = special_vertices[(i + 1) % (number_of_vertices - 1)];
		edges.push_back(Edge(nullptr, nullptr, nullptr, vector<shared_ptr<Vertex> > {first_vertex, second_vertex}, outer_face, 100 * index + index));
		second_vertex->to_ = &edges.back();
		segments.push_back(&edges.back()); 
	}

	/* dependencies set */
	for (int i = 0; i < number_of_vertices - 1;i++) {
		special_vertices[i]->to_->prev_ = special_vertices[((i - 1) + (number_of_vertices - 1)) % (number_of_vertices - 1)]->to_;
		special_vertices[i]->to_->next_ = special_vertices[(i + 1) % (number_of_vertices - 1)]->to_;
	}

	/* outer face needs face*/
	outer_face->edge_ = &edges.back();

}

/// <summary>
/// Setting values of array <c>starts</c> to values of given fingerprint.
/// </summary>
/// <param name="fingerprint"></param>
inline void graph::recolor_fingerprint(const string& fingerprint) { //fingerprint does include the first ro (0..n), otherwise do the first rotation manually

	for(int i = 0; i < number_of_vertices;i++){
		for (int j = 0; j < number_of_vertices - 1;j++) { //every vertex has n-1 around itself
			starts[i][fingerprint[i * (number_of_vertices - 1) + j] - '0'] = i * (number_of_vertices - 1) + j;
		}
	}
}

/// <summary>
/// Function to create all edges incident to vertex 0.
/// </summary>
inline void graph::create_base_star() {
	for (int i = 1; i < number_of_vertices;i++) {
		add_edge(segments[starts[0][i]]->vertices_[0], segments[starts[i][0]]->vertices_[0], outer_face, 0, i, make_pair(0, 0), make_pair(0, 0), true); //from vertex is that in rotation
	}
}


/// <summary>
/// Function to create all "dummy" vertices
/// </summary>
inline void graph::create_all_special_vertices() {

	create_special_vertex(0, 0, 0); // zero ones

	coordinates_of_special_vertices = create_circle(150, 0, 0, number_of_vertices - 1);

	for (int i = 1; i < number_of_vertices;i++) { //the rest of a star
		create_special_vertex(i, coordinates_of_special_vertices[i - 1].x, coordinates_of_special_vertices[i - 1].y);
		vertices_.push_back(make_pair((int)coordinates_of_special_vertices[i - 1].x, (int)coordinates_of_special_vertices[i - 1].y));
	}
}

/*
inline void graph::redirect_previous_segment(int a_index, int b_index, shared_ptr<Vertex> destination) {
	if (segments[segments.size() - 2]->index_ == 100 * a_index + b_index) {

		auto edge_to_change = segments[segments.size() - 2];

		auto a = edge_to_change->vertices_[0];
		auto b = edge_to_change->vertices_.back();

		
		//find which face is the the stable one by maintaning
		//pointer to i.e. prev_ edge of edge_to change

		auto prev = edge_to_change->prev_;
		delete_edge_back();
		
		if(print_bool)
			write_coordinates();

		auto face = prev->face_;

		// swaping to_ attribute of divided edge new vertex is important!
		segments[edges.size() - 1]->vertices_[0]->to_ = segments[edges.size() - 2]->prev_;
		add_edge(a, b, face, a_index, b_index, make_pair(0, 0), make_pair(0, 0));
		segments[edges.size() - 3]->vertices_[0]->to_ = segments[edges.size() - 3]->prev_;
		if (print_bool)
			write_coordinates();
	}
}
*/


/// <summary>
/// Main algorithm similar to described in psedocode in thesis.
/// It recursively tries all the possible ways
/// how to pull the edges.
/// </summary>
/// <param name="s_index"></param>
/// <param name="t_index"></param>
/// <param name="a"></param>
/// <param name="b"></param>
inline void graph::find_the_way_to_intersect(int s_index, int t_index, int a, int b) {
	
	auto seg = segments[s_index]->next_;
	

	while (seg != segments[s_index]) { //the first doesnt have to be considered because it is either beggining segment so it cannot be intersected or it has been already intersected

		if (seg == segments[t_index]) {

			auto shift_previous = find_vertex_in_right_direction(segments[s_index]->vertices_[0],
				make_pair(segments[segments.size() - 3]->vertices_[1]->x_ - segments[segments.size() - 3]->vertices_[0]->x_, 
					segments[segments.size() - 3]->vertices_[1]->y_ - segments[segments.size() - 3]->vertices_[0]->y_
				));
			if (segments[s_index]->vertices_[0]->index_ != -1)
				shift_previous = make_pair(0, 0);

			add_edge(
				segments[s_index]->vertices_[0],
				segments[t_index]->vertices_[0],
				segments[s_index]->face_,
				a,
				b,
				shift_previous,
				make_pair(0, 0)
				);
			if (print_bool)
				write_coordinates();

			if (b < number_of_vertices - 1) {
				find_the_way_to_intersect(starts[a][b + 1], starts[b + 1][a], a, b + 1);

				if (done) return;

			}
			else if (a < number_of_vertices - 2) {
				find_the_way_to_intersect(starts[a + 1][a + 2], starts[a + 2][a + 1], a + 1, a + 2);

				if (done) return;
			}
			else {

				//print_graph(this);
				realized++;
				done = true;
				return;

			}

			delete_edge_back();
			if (print_bool)
				write_coordinates();
		}

		auto index_first_end = seg->index_ / 100;
		auto index_second_end = seg->index_ % 100;

		if (!blocked[min(a, b)][max(a, b)][min(index_first_end, index_second_end)][max(index_first_end, index_second_end)]) { //if there is same index, always true // it can be divided edge so we need to look at the ends of it to get the indices of vertices

			//intersecting
			blocked[min(a, b)][max(a, b)][min(index_first_end, index_second_end)][max(index_first_end, index_second_end)] = true;

			add_vertex(seg);

			auto shift_previous = find_vertex_in_right_direction(segments[s_index]->vertices_[0],
				make_pair(segments[segments.size() - 5]->vertices_[1]->x_ - segments[segments.size() - 5]->vertices_[0]->x_,
					segments[segments.size() - 5]->vertices_[1]->y_ - segments[segments.size() - 5]->vertices_[0]->y_
				));

			auto shift_next = find_vertex_in_right_direction(segments[edges.size() - 2]->vertices_[0],
				make_pair(segments[edges.size() - 2]->vertices_[1]->x_ - segments[edges.size() - 2]->vertices_[0]->x_,
					segments[edges.size() - 2]->vertices_[1]->y_ - segments[edges.size() - 2]->vertices_[0]->y_));

			if (segments[s_index]->vertices_[0]->index_ != -1)
				shift_previous = make_pair(0, 0);

			if (segments[edges.size() - 1]->vertices_[0]->index_ != -1)
				shift_next = make_pair(0, 0);

			add_edge(
				segments[s_index]->vertices_[0],
				segments[edges.size() - 1]->vertices_[0],
				segments[s_index]->face_,
				a,
				b,
				shift_previous,
				shift_next
				);
			if(print_bool)
				write_coordinates();

			/*important! changing to_ to the opposite direction */
			segments[edges.size() - 3]->vertices_[0]->to_ = segments[edges.size() - 3]->prev_; 

			//try to go further
			find_the_way_to_intersect(edges.size() - 3, t_index, a, b); //it is 3rd from the end, because it was added as second in add_vertex and then 2 more were added in add_edge
			if (done) {
				blocked[min(a, b)][max(a, b)][min(index_first_end, index_second_end)][max(index_first_end, index_second_end)] = false;
				return;
			}

			//segments[edges.size() - 3]->vertices_[0]->to_ = segments[edges.size() - 3];

			//undo-intersect //not commutative!
			delete_edge_back();
			delete_vertex((seg->vertices_.back()).get());
			blocked[min(a, b)][max(a, b)][min(index_first_end, index_second_end)][max(index_first_end, index_second_end)] = false;
			if (print_bool)
				write_coordinates();
		}
		seg = seg->next_;
	}
}

inline long long factorial(int n) {
	return (n == 1 || n == 0) ? 1 : n * factorial(n - 1);
}


/// <summary>
/// Structure for generating all fingerprints iteratively.
/// </summary>
struct fingerprints {
	bool done = false;
	string rotation_system;

	ifstream input_file;

	fingerprints(int n, int index) {
	
		string string_index = index < 10 ? "0" + to_string(index) : to_string(index);
		auto input_path = "data/graph"
			+ to_string(n) + "_" + string_index + ".txt";
		cout << input_path << endl;
		input_file.open(input_path);

		
		if (!getline(input_file, rotation_system))
			done = true;
		
	}

	///<summary>
	// Return state and then move to the next one until it is posible
	///</summary>
	string get_next() {

		string previous_one = rotation_system;

		if (!getline(input_file, rotation_system)) {
			done = true;
			input_file.close();
		}

		return previous_one;
	}
};

/// <summary>
/// Function to check whether the ends of <c>edge</c> are <c>a</c> and <c>b</c>.
/// </summary>
/// <param name="edge"></param>
/// <param name="a"></param>
/// <param name="b"></param>
/// <returns></returns>
inline bool are_there_ends(Edge* edge, int a, int b) {
	if (a > b) swap(a, b);
	if ((min(edge->index_ % 100, edge->index_ / 100) == a) && (max(edge->index_ % 100, edge->index_ / 100) == b))
		return true;
	return false;
}

//bool debug_bool = false;

/// <summary>
/// Function to write coordinates of graph into a file.
/// </summary>
inline void graph::write_coordinates() {

	cout << counter++ << endl;

	vector<vector<vector<Edge> > > drawing_edges;
	drawing_edges.resize(number_of_vertices);
	for (int i = 0; i < number_of_vertices;i++)
		drawing_edges[i].resize(number_of_vertices);

	for (auto cur = edges.begin(); cur != edges.end();cur++) {
		int a = cur->index_ / 100;
		int b = cur->index_ % 100;

		drawing_edges[a][b].push_back(*cur);
	}

	for (int i = 0; i < number_of_vertices;i++) {
		for (int j = i + 1;j < number_of_vertices;j++) {
			for (int k = 0; k < drawing_edges[i][j].size();k++) {
				if (drawing_edges[i][j][k].vertices_.size() == 1)
					cout << "HELP" << endl;
				output_file << "( ";
				for (int l = 0; l < drawing_edges[i][j][k].vertices_.size();l++) {
					output_file
						<< drawing_edges[i][j][k].vertices_[l]->x_ << " "
						<< drawing_edges[i][j][k].vertices_[l]->y_ << " ";
				}
				output_file << ") ";
			}
			output_file << endl;
		}
	}

	output_file << "#" << endl;
}


/// <summary>
/// Function to try all possible drawings, 
/// It tries all the pullings of edges for given fingerprint
/// until some realization,
/// because it is already checked fingerprint.
/// </summary>
inline void graph::create_all_possible_drawings() {

	for (int i = 0; i < number_of_vertices;i++) {
		for (int j = 0; j < number_of_vertices;j++) {
			for (int k = 0; k < number_of_vertices;k++) {
				for (int l = 0; l < number_of_vertices;l++) {
					set<int> tmp = { i, j, k, l };
					if (tmp.size() < 4) {
						blocked[i][j][k][l] = true;
					}

				}
			}
		}
	}

	auto generator_of_fingerprints = fingerprints(number_of_vertices, index);
	while (!generator_of_fingerprints.done) {
		auto fingerprint = generator_of_fingerprints.get_next();
		
		create_all_special_vertices();

		recolor_fingerprint(fingerprint);
		create_base_star();
	
		
		/*
		if (counter <= 600) {
			done = true;
		}
		*/

		//else {
		find_the_way_to_intersect(starts[1][2], starts[2][1], 1, 2);
		//}

		if (done) {
			cout << "yes" << endl;
			
			write_coordinates();

		}

		vertices_.resize(0);
		most_away.resize(0);

		edges.resize(0); segments.resize(0);
		done = false;
		outer_face = make_shared<Face>();
	}

	close_files();

	cout << "realized " << realized << endl;
}

