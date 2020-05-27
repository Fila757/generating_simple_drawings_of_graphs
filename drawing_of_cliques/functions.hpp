#pragma once
#include <memory>
#include <iterator>
#include <iostream>
#include <vector>
#include <list>
//Edge(Edge* next, Edge* prev, Edge* opposite, Vertex* from, Vertex* to, Face* face, int index)
using namespace std;

#define x first
#define y second

using array_4D = vector<vector<vector<vector<bool> > > >;

struct Vertex;
struct Edge;
struct Face;

//pair<int, int> create_next_vertex(double size_of_block, int already_created_vertices, double scale = 1, int cx = 0, int cy = 0);

struct graph {
	/*normal part*/
	int number_of_vertices = 0; //all vertices
	int number_of_edges = 0; //real edges, indexer in segments
	list<Edge> edges;
	vector<pair<int, int > > vertices;
	int size_of_block = 0;
	int already_created_vertices = -1;
	shared_ptr<Face> outer_face;

	graph(int n) {
		size_of_block = (n - 1) % 8 == 0 ? (n - 1) / 8 : (n - 1) / 8 + 1;
		/*
		4*(2k+1) - 4 + 1 = 8k + 1
		* * * * *
		* . . . *
		* . * . *
		* . . . *
		* * * * *
		*/

		outer_face = make_shared<Face>();
	}

	void add_edge(shared_ptr<Vertex> a, shared_ptr<Vertex> b, shared_ptr<Face> face);
	void add_vertex(Edge* edge);
	void delete_edge_at_it(list<Edge>::iterator it);
	void create_next_vertex(double scale = 1, int cx = 0, int cy = 0);

	/*finger print part*/

	vector<int> segments; //number of edges indexer

	array_4D blocked; //iniciatize somewhere where we aready know the size

	vector<vector<int> > starts; // edge from i to j, starting indeces on the the special vertex, when j to i get the opposite edge index so on point ont the opposite special vertex 

	//TODO maybe add arguments and implement it, test the normal part
	void create_special_vertex(const string& rotation);
	void create_base_start();
	void find_the_way_to_intersect();
	void intersect();
	void create_all_possible_drawings();
};

struct Vertex {
	double x_ = 0, y_ = 0;

	Edge* to_;

	Vertex() {}

	Vertex(Edge* to) : to_(to) {}

	Vertex(Edge* to, int x, int y) : to_(to), x_(x), y_(y) {}
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




inline void graph::add_edge(shared_ptr<Vertex> a, shared_ptr<Vertex> b, shared_ptr<Face> face) {

	Edge* toa = nullptr, * tob = nullptr, * froma = nullptr, * fromb = nullptr;

	int counter = 0;
	auto start_edge = face->edge_;
	auto cur_edge = start_edge->next_;
	while (start_edge != cur_edge) { //go around the face and find right edges, it can be done also by going around the vertex
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
	}

	auto new_face = make_shared<Face>(); //new face

	Edge ab_edge(fromb, toa, nullptr, a, b, face, number_of_edges); //edge from a to b
	edges.push_back(ab_edge); number_of_edges++;
	Edge* ab_edge_ptr = &edges.back();

	Edge ba_edge(froma, tob, ab_edge_ptr, b, a, move(new_face), number_of_edges); //edge from b to a
	edges.push_back(ba_edge); number_of_edges++;
	Edge* ba_edge_ptr = &edges.back();

	ab_edge_ptr->opposite_ = ba_edge_ptr; //setting opposite edge that has been already made and face edge
	new_face->edge_ = ba_edge_ptr;

	froma->prev_ = ba_edge_ptr; //changing neighborhood edges
	toa->next_ = ab_edge_ptr;
	fromb->prev_ = ab_edge_ptr;
	tob->next_ = ba_edge_ptr;

	start_edge = ba_edge_ptr; //setting the face property to all edges around this face
	cur_edge = start_edge->next_;
	while (start_edge != cur_edge) {
		cur_edge->face_ = new_face;
		cur_edge = cur_edge->next_;
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
	edges.push_back(Edge(edge->next_, edge, opposite, new_vertex, b, edge->face_, edge->index_)); //new vertex to b, part of normal edge
	auto tob = &edges.back();

	edge->to_ = new_vertex; //changing properties of edge
	edge->next_ = tob;
	edge->opposite_ = toa;

	opposite->to_ = new_vertex; //changing properties of opposite
	opposite->next_ = toa;
	opposite->opposite_ = tob;

}

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


inline void graph::delete_edge_at_it(list<Edge>::iterator it) {

	auto edge = *it;
	auto second_it = it;
	Edge opposite;
	list<Edge>::iterator next_it = next(it, 1);
	list<Edge>::iterator prev_it = next(it, -1);
	if ((next_it != edges.end() && *(edge.opposite_) == *(next_it))) {
		opposite = *next_it;
		second_it = next_it;
	}
	else if (it != edges.begin() && *(edge.opposite_) == *(prev_it)) {
		opposite = *prev_it;
		second_it = prev_it;
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

	/*delete the edge from list*/ //it has to stay where it is otherwise delete 2 two times the same but check which one
	edges.erase(it); edges.erase(second_it); //it is not working remember the bigger and smaller one and give the correct range
}

inline void print_graph(graph* g) {
	cout << "number of edges: " << g->number_of_edges << endl;

	for (auto it : g->edges) {
		cout << it.from_ << ":from to:" << it.to_ << " face:" << it.face_ << " prev:" << it.prev_
			<< " to:" << it.to_ << " opp:" << it.opposite_ << " index:" << it.index_ << endl;
	}

	for (auto it : g->vertices) {
		cout << it.x << ":x y:" << it.y << endl;
	}
}

inline void graph::create_special_vertex(const string& rotation) {
	int first = rotation[0] - '0';
	int second = rotation[1] - '0';

	auto first_vertex = make_shared<Vertex>();
	auto second_vertex = make_shared<Vertex>();
	edges.push_back(Edge(nullptr, nullptr, nullptr, first_vertex, second_vertex, outer_face, edges.size()));
	first_vertex->

	for (int i = 1; i < rotation.size;i++) {
		first_vertex = rotation[i] - '0';
		second_vertex = rotation[(i + 1) % rotation.size()] - '0';
	}

	for()
}