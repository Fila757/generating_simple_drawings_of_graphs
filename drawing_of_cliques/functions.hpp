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

#include "delaunator.hpp"
#include <queue>

using namespace std;

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

#ifndef INF
#define INF 10000000
#endif // !INF


#define x first
#define y second

using array_4D = vector<vector<vector<vector<bool> > > >;

struct Vertex;
struct Edge;
struct Face;

struct Vertex {
	Edge* to_;

	int index_;

	double x_, y_;

	Vertex() {}

	Vertex(double x, double y) : x_(x), y_(y) {}

	//it is really good to be the opposite one because when you create new vertex 
	//from the edge side then it is good to go from the intersection opposite side
	Vertex(Edge* to) : to_(to) {}
};

struct Face {
	Edge* edge_;

	Face() {}

	Face(Edge* edge) : edge_(edge) {}
};

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

struct graph {

	/*normal part*/
	int number_of_vertices = 0; //just real vertices

	vector<Vertex> outer_vertices{Vertex(300, 300), Vertex(-300, 300), Vertex(-300, -300), Vertex(300, -300)}; 

	int realized = 0;
	bool done = false;

	list<Edge> edges;

	shared_ptr<Face> outer_face;

	ofstream output_file;

	void close_files() {
		output_file.close();
	}

	graph(int n) {
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


		auto output_path = "../VizualizerWPF/data/graph"
			+ to_string(n) + ".txt";
		output_file.open(output_path);
	}

	void add_edge(shared_ptr<Vertex> a, shared_ptr<Vertex> b, shared_ptr<Face> face, int a_index, int b_index, bool outer_face_bool = false);
	void add_vertex(Edge* edge);

	void delete_edge_back(bool outer_face_bool = false);
	void delete_vertex(Vertex* a);

	/*finger print part*/

	//number of edges indexer
	vector<Edge*> segments; 
	array_4D blocked; 

	// edge from i to j, starting indeces on the the special vertex 
	// when j to i get the opposite edge index so on point ont the opposite special vertex 
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

	vector<shared_ptr<Vertex> > find_shortest_path(shared_ptr<Vertex> from, shared_ptr<Vertex> to, const vector<shared_ptr<Vertex> >& face_vertices);

	//string find_canonic_fingerprint(const string& fingerprint);

};

inline void create_coordinates(const vector<shared_ptr<Vertex> >& points, vector<vector<double> >& distances) {

	for (int i = 0; i < points.size();i++) {
		for (int j = 0; j < points.size();j++) {
			distances[i][j] = (points[i]->x_ - points[j]->x_) * (points[i]->x_ - points[j]->x_) + (points[i]->y_ - points[j]->y_) * (points[i]->y_ - points[j]->y_);
		}
	}
}

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

vector<shared_ptr<Vertex> > graph::find_shortest_path(shared_ptr<Vertex> from, shared_ptr<Vertex> to, const vector<shared_ptr<Vertex > >& face_vertices) {
	
	vector<shared_ptr<Vertex> > all_vertices{ from };
	all_vertices.push_back(to);
	all_vertices.insert(all_vertices.end(), face_vertices.begin(), face_vertices.end());


	vector<vector<double> > distances;
	distances.resize(all_vertices.size());
	for (int i = 0; i < all_vertices.size();i++)
		distances[i].resize(all_vertices.size());
	create_coordinates(all_vertices, distances);

	vector<int> parent;
	parent.resize(all_vertices.size());
	dijsktra(all_vertices, distances, parent);

	vector<shared_ptr<Vertex> > result;

	int v = 1;
	while (parent[v] != 0) {
		result.push_back(all_vertices[v]);
		v = parent[v];
	}
	result.push_back(all_vertices[0]);
	return result;
}

inline vector<pair<double, double> > create_circle(double radius, double cx, double cy, int n) {
	vector<pair<double, double> > circle;

	double unit_angle = (double)360 / n;

	for (int i = 0; i < n;i++) {
		circle.push_back(make_pair(cx + radius * cos((i + 1) * (unit_angle / 180) * M_PI), cy + radius * sin((i + 1) * (unit_angle / 180) * M_PI)));
	}

	/*
	for (int i = 0; i < n;i++) {
		cout << circle[i].x << " " << circle[i].y << endl;
	}
	*/
	return circle;
}


inline void graph::add_edge(shared_ptr<Vertex> a, shared_ptr<Vertex> b, shared_ptr<Face> face, int a_index, int b_index, bool outer_face_bool) {

	Edge* toa = nullptr, * tob = nullptr, * froma = nullptr, * fromb = nullptr;

	toa = a->to_;
	froma = toa->next_;

    tob = b->to_;
    fromb = tob->next_;

	/* triangulation */


	vector<shared_ptr<Vertex> > faces_vertices;
	auto start = face->edge_;
	auto cur = start->next_;
	faces_vertices.insert(faces_vertices.end(), start->vertices_.begin(), start->vertices_.end() - 1);

	while (cur != start) {
		faces_vertices.insert(faces_vertices.end(), cur->vertices_.begin(), cur->vertices_.end() - 1);
		cur = cur->next_;
	}

	auto coords = vector<double>();
	for (int i = 0; i < faces_vertices.size();i++) {
		coords.push_back(faces_vertices[i]->x_);
		coords.push_back(faces_vertices[i]->y_);
	}

	delaunator::Delaunator d(coords);

	auto mids = vector<shared_ptr<Vertex> >();
	for (int i = 0; i < d.triangles.size();i+=3) {
		for (int j = 0; j < 3;j++) {
			mids.push_back(make_shared<Vertex>(d.coords[2 * d.triangles[i + j]]));
			mids.push_back(make_shared<Vertex>(d.coords[2 * d.triangles[i + j] + 1]));
		}
	}

	auto vertices = find_shortest_path(a, b, mids);

	

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
}
inline void graph::add_vertex(Edge* edge) {

	auto opposite = edge->opposite_;

	auto a = edge->vertices_[0];
	auto b = edge->vertices_.back();

	auto new_vertex = make_shared<Vertex>(edge); //edge is going to this vertex //"edge" it is important because after adding vertex we add teh edge to the same direction (not face, face can be same anyway)
	new_vertex->index_ = -1;
	new_vertex->x_ = (a->x_ + b->x_) / 2; new_vertex->y_ = (a->y_ + b->y_) / 2;

	edges.push_back(Edge(edge->next_, edge, opposite, vector<shared_ptr<Vertex> >{ new_vertex, b }, edge->face_, edge->index_)); //new vertex to b, part of normal edge
	auto tob = &edges.back();
	segments.push_back(tob);

	edges.push_back(Edge(opposite->next_, opposite, edge, vector<shared_ptr<Vertex> >{ new_vertex, a } , opposite->face_, opposite->index_)); //create from new_vertex to a, part of opposite
	auto toa = &edges.back();
	segments.push_back(toa);

	edge->vertices_.back() = new_vertex; //changing properties of edge
	edge->next_ = tob;
	edge->opposite_ = toa;

	opposite->vertices_.back() = new_vertex; //changing properties of opposite
	opposite->next_ = toa;
	opposite->opposite_ = tob;

}

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
	
	segments.pop_back();segments.pop_back();
	edges.pop_back();edges.pop_back();

	//number_of_edges -= 2;
}
inline void graph::delete_vertex(Vertex* vertex) {

	// getting the right variables
	auto edge = vertex->to_;

	auto toa = edge->opposite_;
	auto tob = edge->next_;
	auto edge_opposite = tob->opposite_;

	auto a = toa->vertices_.back();
	auto b = tob->vertices_.back();

	// reconnecting edges and therefore removing the vertex

	edge->vertices_.back() = b;
	edge_opposite->vertices_.back() = a;
	edge->next_ = tob->next_;
	edge_opposite->next_ = toa->next_;

	//setting opposites
	edge->opposite_ = edge_opposite;
	edge_opposite->opposite_ = edge;

	//removig the edges from the back of segments and edges
	//because of the order and recursive algorithm we know they are the last one
	segments.pop_back(); segments.pop_back();
	edges.pop_back(); edges.pop_back();

}

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

inline void graph::recolor_fingerprint(const string& fingerprint) { //fingerprint does include the first ro (0..n), otherwise do the first rotation manually

	for(int i = 0; i < number_of_vertices;i++){
		for (int j = 0; j < number_of_vertices - 1;j++) { //every vertex has n-1 around itself
			starts[i][fingerprint[i * (number_of_vertices - 1) + j] - '0'] = i * (number_of_vertices - 1) + j;
		}
	}
}

inline void graph::create_base_star() {
	for (int i = 1; i < number_of_vertices;i++) {
		add_edge(segments[starts[0][i]]->vertices_[0], segments[starts[i][0]]->vertices_[0], outer_face, 0, i, true); //from vertex is that in rotation
	}
}

inline void graph::create_all_special_vertices() {

	create_special_vertex(0, 0, 0); // zero ones

	auto coordinates_of_special_vertices = create_circle(150, 0, 0, number_of_vertices - 1);

	for (int i = 1; i < number_of_vertices;i++) { //the rest of a star
		create_special_vertex(i, coordinates_of_special_vertices[i - 1].x, coordinates_of_special_vertices[i - 1].y);
	}
}

inline void graph::find_the_way_to_intersect(int s_index, int t_index, int a, int b) {
	
	auto seg = segments[s_index]->next_;
	
	//print_graph(this);

	//cout << "" << endl;

	while (seg != segments[s_index]) { //the first doesnt have to be considered because it is either beggining segment so it cannot be intersected or it has been already intersected

		if (seg == segments[t_index]) {

			add_edge(segments[s_index]->vertices_[0], segments[t_index]->vertices_[0], segments[s_index]->face_, a, b);

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
		}

		auto index_first_end = seg->index_ / 100;
		auto index_second_end = seg->index_ % 100;

		if (!blocked[min(a, b)][max(a, b)][min(index_first_end, index_second_end)][max(index_first_end, index_second_end)]) { //if there is same index, always true // it can be divided edge so we need to look at the ends of it to get the indices of vertices

			//print_graph(this);

			//intersecting
			blocked[min(a, b)][max(a, b)][min(index_first_end, index_second_end)][max(index_first_end, index_second_end)] = true;
			add_vertex(seg);
			add_edge(segments[s_index]->vertices_[0], segments[edges.size() - 1]->vertices_[0], segments[s_index]->face_, a, b);

			/*important! changing to_ to the opposite direction */
			segments[edges.size() - 3]->vertices_[0]->to_ = segments[edges.size() - 3]->prev_; 

			//try to go further
			find_the_way_to_intersect(edges.size() - 3, t_index, a, b); //it is 3rd from the end, because it was added as second in add_vertex and then 2 more were added in add_edge
			if (done) {
				blocked[min(a, b)][max(a, b)][min(index_first_end, index_second_end)][max(index_first_end, index_second_end)]  = false;
				return;
			}

			//segments[edges.size() - 3]->vertices_[0]->to_ = segments[edges.size() - 3];

			//undo-intersect //not commutative!
			delete_edge_back();
			delete_vertex((seg->vertices_.back()).get());
			blocked[min(a, b)][max(a, b)][min(index_first_end, index_second_end)][max(index_first_end, index_second_end)] = false;
		}
		seg = seg->next_;
	}
}

inline long long factorial(int n) {
	return (n == 1 || n == 0) ? 1 : n * factorial(n - 1);
}

/// <summary>
// This struct is my own generator and iterator for all fingerprints
/// </summary>


struct fingerprints {
	bool done = false;
	string rotation_system;

	ifstream input_file;

	fingerprints(int n) {
	
		auto input_path = "data/graph"
			+ to_string(n) + ".txt";
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

inline void graph::write_coordinates() {

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
				output_file << "( ";
				for (int l = 0; l < drawing_edges[i][j][k].vertices_.size();l++) {
					output_file
						<< drawing_edges[i][j][k].vertices_[l]->x_ << " "
						<< drawing_edges[i][j][k].vertices_[l]->y_ << " "
						<< drawing_edges[i][j][k].vertices_[l]->x_ << " "
						<< drawing_edges[i][j][k].vertices_[l]->y_ << " ";
				}
				output_file << ") ";
				output_file << endl;
			}
		}
	}

	output_file << "#" << endl;
}

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

	auto generator_of_fingerprints = fingerprints(number_of_vertices);
	while (!generator_of_fingerprints.done) {
		auto fingerprint = generator_of_fingerprints.get_next();
		
		create_all_special_vertices();
		recolor_fingerprint(fingerprint);
		create_base_star();

		//cout << fingerprint << endl;

		find_the_way_to_intersect(starts[1][2], starts[2][1], 1, 2);

		if (done) {
			cout << "yes" << endl;
			
			write_coordinates();

		}

		edges.resize(0); segments.resize(0);
		done = false;
	}

	close_files();

	cout << "realized " << realized << endl;
}

/*
inline string graph::find_canonic_fingerprint(const string& fingerprint) {

	string permutation_holder;
	for (int i = 0; i < number_of_vertices;i++) 
		permutation_holder += (i + '0');

	auto min_fingerprint = fingerprint;
	auto cur_fingerprint = fingerprint;
	auto new_fingerprint = fingerprint;

	//string minimal_permutation;

	do { //can be changes just to while because first doesnt any change
		cur_fingerprint = fingerprint;
		new_fingerprint = fingerprint;
		for (int i = 0; i < number_of_vertices;i++) {
			int rotation_index = -1;
			for (int j = 0; j < number_of_vertices - 1;j++) {
				int index = (number_of_vertices - 1) * i + j;

				if (permutation_holder[cur_fingerprint[index] - '0'] == '0'
					|| (rotation_index == -1 && permutation_holder[cur_fingerprint[index] - '0'] == '1')) rotation_index = j;

				cur_fingerprint[index] = permutation_holder[cur_fingerprint[index] - '0'];
			}

			rotate(cur_fingerprint.begin() + (number_of_vertices - 1) * i,
				cur_fingerprint.begin() + (number_of_vertices - 1) * i + rotation_index,
				cur_fingerprint.begin() + (number_of_vertices - 1) * (i + 1));
		}
		for (int i = 0; i < number_of_vertices;i++) {
			new_fingerprint.replace((number_of_vertices - 1) * (permutation_holder[i] - '0'), number_of_vertices - 1,
				cur_fingerprint, i * (number_of_vertices - 1), number_of_vertices - 1);
		}
		
		min_fingerprint = min(min_fingerprint, new_fingerprint);
		//if (min_fingerprint == new_fingerprint) minimal_permutation = permutation_holder;

		for (int i = 0; i < number_of_vertices;i++) {
			auto inv_part = new_fingerprint.substr(i * (number_of_vertices - 1) + 1, number_of_vertices - 2);
			reverse(inv_part.begin(), inv_part.end());
			new_fingerprint.replace(i * (number_of_vertices - 1) + 1, number_of_vertices - 2, inv_part);
		}

		min_fingerprint = min(min_fingerprint, new_fingerprint);
		//if (min_fingerprint == new_fingerprint) minimal_permutation = permutation_holder;

	} while (next_permutation(permutation_holder.begin(), permutation_holder.end()));

	//cout << minimal_permutation << endl;
	return min_fingerprint;

}
*/
/*
inline bool is_correct_K4_slower(vector<int> orders[4], int a[4]) {
	
	//The realizable rotation systems of K4 with respect to (strong) isomorhism
	int order_of_K4[3][3][3] = {
		{{2, 3, 0},{3, 0, 1},{0, 1, 2}},
		{{2, 3, 0},{0, 3, 1},{0, 2, 1}},
		{{3, 2, 0},{1, 3, 0},{0, 2, 1}}
	};

	for (int u = 0; u < 3;u++) {
		int number_of_right_ones = 1;
		for (int i = 1;i < 4;i++) {
			auto copied = orders[i];
			copied.insert(copied.end(), orders[i].begin(), orders[i].end()); //copy it twice so we can easily find the pattern even if it has different rotation than mention in order_of_K4

			for (int j = 0; j < copied.size();j++) {
				int temp = 0;
				for (int k = 0; k < 3 && j + k < copied.size();k++) {
					if (copied[j + k] == a[order_of_K4[u][i - 1][k]]) temp++;
				}
				if (temp == 3) { number_of_right_ones++; break; }
			}
		}
		if (number_of_right_ones == 4) return true;
	}
	return false;
}
*/
/*
inline string find_lexical_min_rotation(string str)
{
	// To store lexicographic minimum string
	string min = str;

	for (int i = 0; i < str.length(); i++)
	{
		// left rotate string by 1 unit
		rotate(str.begin(), str.begin() + 1, str.end());

		// check if the rotation is minimum so far
		if (str.compare(min) < 0)
			min = str;
	}

	return min;
}
*/
/*
inline bool is_correct_K4(vector<int> orders[4]) {
	int number_of_positive_rotation = 0;

	for (int i = 0; i < 4;i++) {

		string rotation;
		for_each(orders[i].begin(), orders[i].end(), [&](int part) {rotation += to_string(part);});

		auto minimal_rotation = find_lexical_min_rotation(rotation);
		if (minimal_rotation[1] < minimal_rotation[2])
			number_of_positive_rotation++;
	}

	if (number_of_positive_rotation % 2 == 0)
		return true;
	return false;

}

*/
/*
inline bool graph::is_correct_fingerprint(const string& fingerprint) { //checking all K4

	int i[4];

	for (i[0] = 0; i[0] < number_of_vertices;i[0]++) {
		for (i[1] = i[0] + 1; i[1] < number_of_vertices;i[1]++) {
			for (i[2] = i[1] + 1; i[2] < number_of_vertices;i[2]++) {
				for (i[3] = i[2] + 1; i[3] < number_of_vertices;i[3]++) {

					vector<int> order[4];

					for (int v = 0; v < 4;v++) {
						for (int u = 0; u < number_of_vertices - 1;u++) {
							if (fingerprint[u + i[v] * (number_of_vertices - 1)] - '0' == i[(1 + v) % 4]
								|| fingerprint[u + i[v] * (number_of_vertices - 1)] - '0' == i[(2 + v) % 4]

								|| fingerprint[u + i[v] * (number_of_vertices - 1)] - '0' == i[(3 + v) % 4])
							{
								order[v].push_back(fingerprint[u + i[v] * (number_of_vertices - 1)] - '0');
							}
						}
					}
					// for slower version
					//int temp[4];
					//temp[0] = i[0]; temp[1] = order[0][0];temp[2] = order[0][1];temp[3] = order[0][2];

					if (!is_correct_K4(order)) return false;
				}
			}
		}
	}

	return true;
}
*/