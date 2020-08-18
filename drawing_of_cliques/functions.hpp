#pragma once

#define _USE_MATH_DEFINES
#include <memory>
#include <iterator>
#include <iostream>
#include <vector>
#include <list>
#include <cmath>
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
	int number_of_vertices = 0; //just real vertices
	int number_of_edges = 0; //real edges, indexer in segments
	list<Edge> edges;
	vector<pair<int, int > > vertices; 
	//int already_created_vertices = 0;
	shared_ptr<Face> outer_face;

	graph(int n) {
		number_of_vertices = n;
		outer_face = make_shared<Face>();
	}

	void add_edge(shared_ptr<Vertex> a, shared_ptr<Vertex> b, shared_ptr<Face> face, bool outer_face_bool = false);
	void add_vertex(Edge* edge);
	void delete_edge_at_it(list<Edge>::iterator it);

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
	void intersect();
	void create_all_possible_drawings();
};

struct Vertex {
	double x_ = 0, y_ = 0;

	Edge* to_;

	int index_;

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




inline void graph::add_edge(shared_ptr<Vertex> a, shared_ptr<Vertex> b, shared_ptr<Face> face, bool outer_face_bool) {

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

	auto new_face = !outer_face_bool ? make_shared<Face>() : outer_face; //new face or old face when creating star

	Edge ab_edge(fromb, toa, nullptr, a, b, face, number_of_edges); //edge from a to b
	edges.push_back(ab_edge); number_of_edges++;
	Edge* ab_edge_ptr = &edges.back();
	segments.push_back(ab_edge_ptr);

	Edge ba_edge(froma, tob, ab_edge_ptr, b, a, move(new_face), number_of_edges); //edge from b to a
	edges.push_back(ba_edge); number_of_edges++;
	Edge* ba_edge_ptr = &edges.back();
	segments.push_back(ba_edge_ptr);

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


vector<pair<double, double> > create_circle(int radius, int cx, int cy, int n) {
	vector<pair<double, double> > circle;

	double unit_angle = 360 / n;

	for (int i = 0; i < n;i++) {
		circle.push_back(make_pair(radius * cos(unit_angle / 180 * M_PI), radius * sin(unit_angle / 180 * M_PI)));
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

	bool second_is_bigger;

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

inline void graph::create_special_vertex(pair<double, double> center_of_real_vertex, int index) {

	//maybe change the radius so the accuracy shouldnot be a problem
	auto special_vertex_coordinates = create_circle(1, center_of_real_vertex.x, center_of_real_vertex.y, number_of_vertices - 1); // -1 because you dont wan to be connected to yourself

	vector<shared_ptr<Vertex> > special_vertices;

	/*create vertices with coordinates*/
	for (int i = 0; i < number_of_vertices - 1;i++) {
		auto new_vertex = make_shared<Vertex>(special_vertex_coordinates[i].x, special_vertex_coordinates[i].y);
		new_vertex->index_ = index; //the real index of vertex
		special_vertices.push_back(new_vertex);
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
		special_vertices[i]->to_->prev_ = special_vertices[((i - 1) + (number_of_vertices - 1)) % number_of_vertices - 1]->to_;
		special_vertices[i]->to_->next_ = special_vertices[(i + 1) % (number_of_vertices - 1)]->to_;
	}
}




inline void graph::recolor_fingerprint(const string& fingerprint) { //fingerprint does include the first ro (0..n), otherwise do the first rotation manually

	auto edges_it = edges.begin();

	for(int i = 0; i < number_of_vertices;i++){
		for (int j = 0; j < number_of_vertices - 1;j++) { //every vertex has n-1 around itself
			starts[i][fingerprint[i * (number_of_vertices) + j] - '0'] = edges_it->index_;
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
				// done ?!
			}
		}
		if (!blocked[a][b][seg->from_->index_][seg->to_->index_]) { //if there is same index, always true
			blocked[a][b][seg->from_->index_][seg->to_->index_] = true;

			add_vertex(seg);
			find_the_way_to_intersect();

			find_the_way_to_intersect( // , t_index, a, b); //first divide this seg and "//" replace with new created vertex I think
			undo_intersect(seg);
			blocked[a][b][seg->from_->index_][seg->to_->index_] = false;
		}
		seg = seg->next_;
	}
}
