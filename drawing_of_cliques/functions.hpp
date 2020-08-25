#pragma once

#include <cmath>
#include <memory>
#include <iterator>
#include <iostream>
#include <vector>
#include <list>
#include <algorithm>

using namespace std;

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

#define x first
#define y second

using array_4D = vector<vector<vector<vector<bool> > > >;

struct Vertex;
struct Edge;
struct Face;

struct graph {

	/*normal part*/
	int number_of_vertices = 0; //just real vertices
	//int number_of_edges = 0; //real edges, indexer in segments
	int realized = 0;

	list<Edge> edges;
	vector<pair<double, double> > vertices; 
	//int already_created_vertices = 0;

	shared_ptr<Face> outer_face;

	graph(int n) {
		number_of_vertices = n;
		outer_face = make_shared<Face>();

		starts.resize(number_of_vertices);
		for (int i = 0; i < number_of_vertices;i++) {
			starts[i].resize(number_of_vertices);
		}
	}

	void add_edge(shared_ptr<Vertex> a, shared_ptr<Vertex> b, shared_ptr<Face> face, bool outer_face_bool = false);
	void add_vertex(Edge* edge);
	void delete_edge_at_it(list<Edge>::iterator it);
	void delete_vertex(Vertex* a);

	/*finger print part*/

	vector<Edge*> segments; //number of edges indexer

	array_4D blocked; //iniciatize somewhere where we aready know the size

	vector<vector<int> > starts; // edge from i to j, starting indeces on the the special vertex, when j to i get the opposite edge index so on point ont the opposite special vertex 

	//TODO maybe add arguments and implement it, test the normal part
	void create_special_vertex(pair<double, double> center_of_real_vertex, int index);
	void recolor_fingerprint(const string& rotation);
	void create_base_star();
	void create_all_special_vertices();
	void find_the_way_to_intersect(int s_index, int t_index, int a, int b);
	//void intersect();
	bool is_correct_fingerprint(const string& fingerprint);
	void create_all_possible_drawings(int n);
};

struct Vertex {
	double x_ = 0, y_ = 0;

	Edge* to_;

	int index_;

	Vertex() {}

	Vertex(Edge* to) : to_(to) {}

	Vertex(Edge* to, double x, double y) : to_(to), x_(x), y_(y) {}

	Vertex(double x, double y) : x_(x), y_(y) {} //check if all uses are really connected to edge
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

	shared_ptr<Vertex> from_;
	shared_ptr<Vertex> to_;

	shared_ptr<Face> face_;

	Edge() {}

	Edge(Edge* next, Edge* prev, Edge* opposite, shared_ptr<Vertex> from, shared_ptr<Vertex> to, shared_ptr<Face> face, int index) :
		next_(next), prev_(prev), opposite_(opposite), from_(from), to_(to), face_(face), index_(index) {}

	bool operator==(const Edge& a) {
		return (from_ == a.from_ && to_ == a.to_);
	}

	bool operator!=(const Edge& a) {
		return !(*this == a);
	}

};

inline void graph::add_edge(shared_ptr<Vertex> a, shared_ptr<Vertex> b, shared_ptr<Face> face, bool outer_face_bool) {

	Edge* toa = nullptr, * tob = nullptr, * froma = nullptr, * fromb = nullptr;

	int counter = 0;
	if (!outer_face_bool) {
		auto start_edge = face->edge_;
		auto cur_edge = start_edge;
		do { //go around the face and find right edges, it can be done also by going around the vertices
			if (cur_edge->from_ == a) {
				froma = cur_edge; counter++;
			}
			if (cur_edge->to_ == a) {
				toa = cur_edge; counter++;
			}
			if (cur_edge->from_ == b) {
				fromb = cur_edge; counter++;
			}
			if (cur_edge->to_ == b) {
				tob = cur_edge; counter++;
			}
			if (counter == 4) break;
			cur_edge = cur_edge->next_;
		} while (start_edge != cur_edge);
	}
	else {
		for (int i = 0; i < edges.size();i++){
			auto cur_edge = segments[i];
			if (cur_edge->from_ == a) {
				froma = cur_edge; counter++;
			}
			if (cur_edge->to_ == a) {
				toa = cur_edge; counter++;
			}
			if (cur_edge->from_ == b) {
				fromb = cur_edge; counter++;
			}
			if (cur_edge->to_ == b) {
				tob = cur_edge; counter++;
			}
			if (counter == 4) break;
		}
	}
	auto new_face = !outer_face_bool ? make_shared<Face>() : outer_face; //new face or old face when creating star

	Edge ab_edge(fromb, toa, nullptr, a, b, face, edges.size()); //edge from a to b
	edges.push_back(ab_edge); //number_of_edges++;
	Edge* ab_edge_ptr = &edges.back();
	segments.push_back(ab_edge_ptr);

	Edge ba_edge(froma, tob, ab_edge_ptr, b, a, new_face, edges.size()); //edge from b to a
	edges.push_back(ba_edge); //number_of_edges++;
	Edge* ba_edge_ptr = &edges.back();
	segments.push_back(ba_edge_ptr);

	ab_edge_ptr->opposite_ = ba_edge_ptr; //setting opposite edge that has been already made and face edge
	new_face->edge_ = ba_edge_ptr;

	froma->prev_ = ba_edge_ptr; //changing neighborhood edges
	toa->next_ = ab_edge_ptr;
	fromb->prev_ = ab_edge_ptr;
	tob->next_ = ba_edge_ptr;

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

	auto a = edge->from_;
	auto b = edge->to_;

	auto new_vertex = make_shared<Vertex>(edge, (a->x_ + b->x_) / 2, (a->y_ + b->y_) / 2); //edge is goint to this vertex
	vertices.push_back(make_pair(new_vertex->x_, new_vertex->y_));


	edges.push_back(Edge(opposite->next_, opposite, edge, new_vertex, a, opposite->face_, opposite->index_)); //create from new_vertex to a, part of opposite
	auto toa = &edges.back();
	segments.push_back(toa);

	edges.push_back(Edge(edge->next_, edge, opposite, new_vertex, b, edge->face_, edge->index_)); //new vertex to b, part of normal edge
	auto tob = &edges.back();
	segments.push_back(tob);

	edge->to_ = new_vertex; //changing properties of edge
	edge->next_ = tob;
	edge->opposite_ = toa;

	opposite->to_ = new_vertex; //changing properties of opposite
	opposite->next_ = toa;
	opposite->opposite_ = tob;

}

inline vector<pair<double, double> > create_circle(double radius, double cx, double cy, int n) {
	vector<pair<double, double> > circle;

	double unit_angle = (double) 360 / n;

	for (int i = 0; i < n;i++) {
		circle.push_back(make_pair(cx + radius * cos((i + 1) * (unit_angle / 180) * M_PI), cy + radius * sin((i + 1) * (unit_angle / 180) * M_PI)));
	}

	return circle;
}

/*
inline void graph::create_next_vertex(double scale, int cx, int cy) { //size_of_block is synchronized with scale
	double x = cx, y = cy;

	//divide into four parts

	if (already_created_vertices == -1) {
		vertices.push_back(make_pair(x, y));
		already_created_vertices++;
		return;
	}

	int mod = already_created_vertices % 4;
	int div = already_created_vertices / 4;

	int sign;
	if (mod % 2 == 0) {
		sign = (mod == 1 || mod == 2) ? -1 : 1; // first and second sector even if it cannot drop in thi sector
		x += sign * size_of_block;
		//sign already done // second or first going opposite
		y += sign * (-(size_of_block - scale) + scale * div);
	}
	if (mod % 2 == 1) {
		sign = (mod == 0 || mod == 1) ? 1 : -1; // zero and first sector
		y += sign * size_of_block;
		sign = (mod == 1) ? -1 : 1; // second or first
		x += sign * (-(size_of_block - scale) + scale * div);
	}

	vertices.push_back(make_pair(x, y));

	already_created_vertices++;
}
*/

inline void graph::delete_edge_at_it(list<Edge>::iterator it) {

	bool second_is_bigger = false;

	auto edge = *it;
	auto second_it = it;
	Edge opposite;
	list<Edge>::iterator next_it = next(it, 1);
	list<Edge>::iterator prev_it = next(it, -1); //hopeffuly doesnt fall down when beggining
	if ((next_it != edges.end() && *(edge.opposite_) == *(next_it))) {
		opposite = *next_it;
		second_it = next_it;
		second_is_bigger = true;
	}
	else if (it != edges.begin() && *(edge.opposite_) == *(prev_it)) {
		opposite = *prev_it;
		second_it = prev_it;
		second_is_bigger = false;
	}


	auto a = edge.from_;
	auto b = edge.to_;

	auto face = edge.face_;

	auto froma = opposite.next_;
	auto toa = edge.prev_;
	auto fromb = edge.next_;
	auto tob = opposite.prev_;

	/*recconect edges*/
	froma->prev_ = toa;
	toa->next_ = froma;
	fromb->prev_ = tob;
	tob->next_ = fromb;

	/*update edges if there the bad ones*/
	a->to_ = toa;
	b->to_ = tob;

	/*update faces*/

	auto cur_edge = opposite.next_;

	while (opposite != *cur_edge) {
		cur_edge->face_ = face;
		cur_edge = cur_edge->next_;
	}

	face->edge_ = froma;

	/* delete the edge from list, pop from segments should be out of this function */
	if (second_is_bigger)
		edges.erase(it, second_it);
	else
		edges.erase(second_it, it);

	//number_of_edges -= 2;
}

inline void graph::delete_vertex(Vertex* vertex) {

	// getting the right variables
	auto edge = vertex->to_;

	auto toa = edge->opposite_;
	auto tob = edge->next_;
	auto edge_opposite = tob->opposite_;

	auto a = toa->to_;
	auto b = tob->to_;

	// reconnecting edges and therefore removing the vertex

	edge->to_ = b;
	edge_opposite->to_ = a;
	edge->next_ = tob->next_;
	edge_opposite->next_ = toa->next_;

	//removig the edges from the back of segments and edges
	//because of the order and recursive algorithm we know they are the last one
	segments.pop_back(); segments.pop_back();
	edges.pop_back(); edges.pop_back();

	vertices.pop_back();

}

inline void print_graph(graph* g) {
	cout << "number of edges: " << g->edges.size() << endl;

	for (auto it : g->edges) {
		cout << it.from_ << ":from to:" << it.to_ << " face:" << it.face_ << " prev:" << it.prev_->index_ << " next:" << it.next_->index_
			<< " opp:" << ((it.opposite_ == nullptr) ? -1 : it.opposite_->index_) << " index:" << it.index_ << endl;
	}

	for (auto it : g->vertices) {
		cout << it.x << ":x y:" << it.y << endl;
	}
}

inline void graph::create_special_vertex(pair<double, double> center_of_real_vertex, int index) {

	//maybe change the radius so the accuracy shouldnot be a problem
	auto special_vertex_coordinates = create_circle(1, center_of_real_vertex.x, center_of_real_vertex.y, number_of_vertices - 1); // -1 because you dont wan to be connected to yourself

	vector<shared_ptr<Vertex> > special_vertices;

	/*create vertices with coordinates*/
	for (int i = 0; i < number_of_vertices - 1;i++) {
		auto new_vertex = make_shared<Vertex>(special_vertex_coordinates[i].x, special_vertex_coordinates[i].y);
		new_vertex->index_ = index; //the real index of vertex
		special_vertices.push_back(new_vertex);
		vertices.push_back(make_pair(new_vertex->x_, new_vertex->y_));
	}

	
	/* edges created */
	for (int i = 0; i < number_of_vertices - 1;i++) {
		auto first_vertex = special_vertices[i];
		auto second_vertex = special_vertices[(i + 1) % (number_of_vertices - 1)];
		edges.push_back(Edge(nullptr, nullptr, nullptr, first_vertex, second_vertex, outer_face, edges.size()));
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

	auto edges_it = edges.begin();

	for(int i = 0; i < number_of_vertices;i++){
		for (int j = 0; j < number_of_vertices - 1;j++) { //every vertex has n-1 around itself
			starts[i][fingerprint[i * (number_of_vertices - 1) + j] - '0'] = edges_it->index_;
			edges_it++;
		}
	}
}

inline void graph::create_base_star() {
	for (int i = 1; i < number_of_vertices;i++) {
		add_edge(segments[starts[0][i]]->from_, segments[starts[i][0]]->from_, outer_face, true); //from vertex is that in rotation
	}
}

inline void graph::create_all_special_vertices() {
	auto circle = create_circle(10, 0, 0, number_of_vertices - 1);

	create_special_vertex(make_pair(0, 0), 0); //the center

	for (int i = 0; i < number_of_vertices - 1;i++) { //the rest of a star
		cout << circle[i].x << ":x 2 y:" << circle[i].y << endl;
		create_special_vertex(circle[i], i + 1);
	}
}

inline void graph::find_the_way_to_intersect(int s_index, int t_index, int a, int b) {
	
	auto seg = segments[s_index]->next_;
	
	while (seg != segments[s_index]) { //the first doesnt have to be considered because it is either beggining segment so it cannot be intersected or it has been already intersected
		
		if (seg == segments[t_index]) {

			add_edge(segments[s_index]->from_, segments[t_index]->from_, segments[s_index]->face_);

			if (b < number_of_vertices - 1) {
				find_the_way_to_intersect(starts[a][b + 1], starts[b + 1][a], a, b + 1);
			}
			else if (a < number_of_vertices - 2) {
				find_the_way_to_intersect(starts[a + 1][a + 2], starts[a + 2][a + 1], a + 1, a + 2);
			}
			else {
				realized++;
			}
		}
		if (!blocked[a][b][seg->from_->index_][seg->to_->index_]) { //if there is same index, always true
			blocked[a][b][seg->from_->index_][seg->to_->index_] = true;

			//intersecting
			add_vertex(seg);

			//try to go further
			find_the_way_to_intersect(seg->opposite_->index_, t_index, a, b);

			//undo-intersect
			delete_vertex((seg->to_).get());

			blocked[a][b][seg->from_->index_][seg->to_->index_] = false;
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
	vector<int> states;
	long long treshold;
	bool done = false;
	vector<string> fingerprint;

	/// <summary> 
	// Set up the treshold (number of permutation of given string), then generate first rotation systems and reset the states to zeroes
	/// </summary>
	fingerprints(int n) {
		treshold = factorial(n-1); //its length is n-1

		for (int i = 1; i < n;i++) { 
			string rotation_system = "";
			for (int j = 0; j < n;j++) {
				if (i != j) rotation_system += (j + '0');
			}
			fingerprint.push_back(rotation_system);
		}

		for (int i = 0; i < n;i++) {
			states.push_back(0);
		}
	}

	///<summary>
	// Return state and then move to the next one until it is posible
	///</summary>
	const string& get_next() {
		string res;
		for_each(fingerprint.begin(), fingerprint.end(), [&](const string& part) {res += part;});

		for (long long i = treshold - 1; i >= 0;i--) {
			if (states[i] == treshold - 1) {
				if (i == 0) done = true; //There is no other fingerprint
				states[i] = 0;
				next_permutation(fingerprint[i].begin() + 1, fingerprint[i].end()); // + 1 because rotation system is n-1 times counted so 0 can be fixed as the first position number
			}
			else {
				states[i]++;
				next_permutation(fingerprint[i].begin() + 1, fingerprint[i].end());
				break;
			}
		}

		return res;
	}
};

inline void graph::create_all_possible_drawings(int n) {

	create_all_special_vertices();
	create_base_star();

	auto generator_of_fingerprints = fingerprints(n);
	while (!generator_of_fingerprints.done) {
		auto cur = generator_of_fingerprints.get_next();

		//check the fingerprint 
		if (!is_correct_fingerprint(cur)) continue;

		recolor_fingerprint(cur);

		find_the_way_to_intersect(starts[0][1], starts[1][0], 0, 1);
	}
}
     

inline bool is_correct_K4(vector<int> orders[4], int a[4]) {
	
	//The realizable rotation systems of K4 with respect to (strong) isomorhism
	int order_of_K4[3][3][3] = { 
		{{2, 3, 0},{3, 0, 1},{0, 1, 2}},
		{{2, 3, 0},{0, 3, 1},{0, 2, 1}},
		{{3,2,0}, {1, 3, 0}, {0, 2, 1}}
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

inline bool graph::is_correct_fingerprint(const string& fingerprint) { //checking all K4

	string prefix = "";
	for (int i = number_of_vertices - 1; i > 0;i--) prefix += (i + '0');
	auto whole_fingerprint = prefix + fingerprint;

	int i[4];

	for (i[0] = 0; i[0] < number_of_vertices;i[0]++) {
		for (i[1] = i[0] + 1; i[1] < number_of_vertices;i[1]++) {
			for (i[2] = i[1] + 1; i[2] < number_of_vertices;i[2]++) {
				for (i[3] = i[2] + 1; i[3] < number_of_vertices;i[3]++) {

					vector<int> order[4];
					for (int v = 0; v < 4;v++) {
						for (int u = 0; u < number_of_vertices - 1;u++) {
							if (u + i[0 + v] * (number_of_vertices - 1) == i[(1 + v) % 4]
								|| u + i[0 + v] * (number_of_vertices - 1) == i[(2 + v) % 4]
								|| u + i[0 + v] * (number_of_vertices - 1) == i[(3 + v) % 4])
							{
								order[0 + v].push_back(u);
							}
						}
					}

					int temp[4];
					temp[0] = i[0]; temp[1] = order[0][0]; temp[2] = order[0][1]; temp[3] = order[0][2];
					if (!is_correct_K4(order, temp)) return false;
				}
			}
		}
	}

	return true;
}
