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
#include <thread>
#include <mutex>

using namespace std;

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

#ifndef SIZE_OF_ARRAY
#define SIZE_OF_ARRAY 10
#endif 


#define x first
#define y second

using array_4D = vector<vector<vector<vector<bool> > > >;

inline long long fact[SIZE_OF_ARRAY + 5];

inline void precount_factorials(){
	fact[0] = 1;
	for(int i = 1; i < SIZE_OF_ARRAY + 5;i++)
		fact[i] = i * fact[i-1];
	//cout << fact[SIZE_OF_ARRAY -1] << endl;
}


/// <summary>
/// Wraper for comunicating through threads
/// </summary>
struct canonic_wraper{
	unordered_map<string, bool> canonic_fingerprints;
    mutex shared_mutex;
};

struct Vertex;
struct Edge;
struct Face;

/// <summary>
/// Main class to store all the information about graph, edges, vertices, ...
/// </summary>
struct graph {

	/*normal part*/
	int number_of_vertices = 0; //just real vertices

	int realized = 0;
	bool done = false;

	int index;

	list<Edge> edges;

	shared_ptr<Face> outer_face;

	ofstream output_file;

	void close_files() {
		output_file.close();
	}

	graph(int n, int i, shared_ptr<canonic_wraper> shared_canonics) {
		index = i;
		canonic_fingerprints_wraper = shared_canonics;

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
		auto output_path = "data/graph"
			+ to_string(n) + "_" + string_index + ".txt";
		output_file.open(output_path);

		cout << "opened " << output_path << endl;
		

		precount_factorials();
	}

	void add_edge(shared_ptr<Vertex> a, shared_ptr<Vertex> b, shared_ptr<Face> face, int a_index, int b_index, bool outer_face_bool = false);
	void add_vertex(Edge* edge);

	void delete_edge_back(bool outer_face_bool = false);
	void delete_vertex(Vertex* a);

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

	shared_ptr<canonic_wraper> canonic_fingerprints_wraper;

	void create_special_vertex(int index);
	void recolor_fingerprint(const string& rotation);
	void create_base_star();
	void create_all_special_vertices();
	void find_the_way_to_intersect(int s_index, int t_index, int a, int b);
	//void intersect();
	bool is_correct_fingerprint(const string& fingerprint);
	void create_all_possible_drawings();

	string find_canonic_fingerprint(const string& fingerprint);

	bool is_some_of_faces_incorrect(Edge* edge);
	bool is_face_incorrect(shared_ptr<Face> face);

	void print_counters(Face* f);


};

/// <summary>
/// Class to store vertex, containing information about edge <c>to_</c> going into it
/// </summary>
struct Vertex {
	Edge* to_;

	int index_;

	Vertex() {}

	//it is really good to be the opposite one because when you create new vertex 
	//from the edge side then it is good to go from the intersection opposite side
	Vertex(Edge* to) : to_(to) {}
};

/// <summary>
/// Structure to store a face containing about one edge incident to it
/// </summary>
struct Face {
	Edge* edge_;

	Face() {
		for (int i = 0; i < SIZE_OF_ARRAY + 5;i++)
			counter_of_same_last_edges[i] = 0;	
		}

	//Face(Edge* edge) : edge_(edge) {}


	//int counter_of_last_edges = 0;
	int counter_of_same_last_edges[SIZE_OF_ARRAY + 5];
};

/// <summary>
/// Structure to store the edge containing information about <c>next_</c>, <c>prev_</c>, <c>opposite_</c> edges and vertices and face incident to it.
/// </summary>
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

inline void print_graph(graph* g) {
	cout << "number of edges: " << g->edges.size() << endl;

	int i = 0;
	for (auto it : g->edges) {
		cout << it.from_ << ":from to:" << it.to_ << " face:" << it.face_ << " prev:" << it.prev_->index_ << " next:" << it.next_->index_
			<< " opp:" << ((it.opposite_ == nullptr) ? -1 : it.opposite_->index_) << " index:" << it.index_ <<  " s: " << i << endl;
		i++;
	}
}


inline void graph::print_counters(Face* f) {
	for (int i = 0; i < number_of_vertices - 1;i++)
		cout << f->counter_of_same_last_edges[i] << " ";
	cout << endl;
}

/// <summary>
/// Function to add new (part of) edge between vertices <c>a</c> and <c>b</c> through the <c>face</c> 
/// </summary>
/// <param name="a"></param>
/// <param name="b"></param>
/// <param name="face"></param>
/// <param name="a_index"></param>
/// <param name="b_index"></param>
/// <param name="outer_face_bool"></param>
inline void graph::add_edge(shared_ptr<Vertex> a, shared_ptr<Vertex> b, shared_ptr<Face> face, int a_index, int b_index, bool outer_face_bool) {

	Edge* toa = nullptr, * tob = nullptr, * froma = nullptr, * fromb = nullptr;

	toa = a->to_;
	froma = toa->next_;

    tob = b->to_;
    fromb = tob->next_;

	auto new_face = !outer_face_bool ? make_shared<Face>() : outer_face; //new face or old face when creating star

	Edge ab_edge(fromb, toa, nullptr, a, b, face, a_index*100 + b_index); //edge from a to b
	edges.push_back(ab_edge); //number_of_edges++;
	Edge* ab_edge_ptr = &edges.back();
	segments.push_back(ab_edge_ptr);

	//print_graph(this);

	Edge ba_edge(froma, tob, ab_edge_ptr, b, a, new_face, b_index*100 + a_index); //edge from b to a
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
			if ((cur_edge->index_ / 100 == number_of_vertices - 1) != (cur_edge->index_ % 100 == number_of_vertices - 1)) {
				new_face->counter_of_same_last_edges[min(cur_edge->index_ / 100, cur_edge->index_ % 100)]++;
			}
			cur_edge = cur_edge->next_;
		}

		for (int i = 0; i < number_of_vertices - 1;i++) {
			face->counter_of_same_last_edges[i] -= new_face->counter_of_same_last_edges[i];
		}

	}
	

	if (((ab_edge_ptr->index_ / 100) == (number_of_vertices -1)) != ((ab_edge_ptr->index_ % 100) == (number_of_vertices - 1))) {
		new_face->counter_of_same_last_edges[min(ab_edge_ptr->index_ / 100, ab_edge_ptr->index_ % 100)]++;
		face->counter_of_same_last_edges[min(ab_edge_ptr->index_ / 100, ab_edge_ptr->index_ % 100)]++;
	}

	
	face->edge_ = ab_edge_ptr;

	
}

/// <summary>
/// Creating new vertec on the <c>edge</c> as a new intersection.
/// </summary>
/// <param name="edge"></param>
inline void graph::add_vertex(Edge* edge) {


	auto opposite = edge->opposite_;


	auto a = edge->from_;
	auto b = edge->to_;

	auto new_vertex = make_shared<Vertex>(edge); //edge is going to this vertex //"edge" it is important because after adding vertex we add teh edge to the same direction (not face, face can be same anyway)
	new_vertex->index_ = -1;

	edges.push_back(Edge(edge->next_, edge, opposite, new_vertex, b, edge->face_, edge->index_)); //new vertex to b, part of normal edge
	auto tob = &edges.back();
	segments.push_back(tob);

	edges.push_back(Edge(opposite->next_, opposite, edge, new_vertex, a, opposite->face_, opposite->index_)); //create from new_vertex to a, part of opposite
	auto toa = &edges.back();
	segments.push_back(toa);

	edge->to_ = new_vertex; //changing properties of edge
	edge->next_ = tob;
	edge->opposite_ = toa;

	opposite->to_ = new_vertex; //changing properties of opposite
	opposite->next_ = toa;
	opposite->opposite_ = tob;

	if ((edge->index_ / 100 == number_of_vertices - 1) != (edge->index_ % 100 == number_of_vertices - 1)) {
		edge->face_->counter_of_same_last_edges[min(edge->index_ / 100, edge->index_ % 100)]++;
		opposite->face_->counter_of_same_last_edges[min(edge->index_ / 100, edge->index_ % 100)]++;
	}


}

/// <summary>
/// Deleting the last added edge.
/// </summary>
/// <param name="outer_face_bool"></param>
inline void graph::delete_edge_back(bool outer_face_bool) {

	auto edge = edges.back();
	auto opposite = *edge.opposite_;

	auto a = edge.from_;
	auto b = edge.to_;

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
			if ((cur_edge->index_ / 100 == number_of_vertices - 1) != (cur_edge->index_ % 100 == number_of_vertices - 1)) {
				face->counter_of_same_last_edges[min(cur_edge->index_ / 100, cur_edge->index_ % 100)]++;
			}
			cur_edge = cur_edge->next_;
		}
	}


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
	
	if ((edge.index_ / 100 == number_of_vertices - 1) != (edge.index_ % 100 == number_of_vertices - 1)) {
		face->counter_of_same_last_edges[min(edge.index_ / 100, edge.index_ % 100)]--;
	}

	segments.pop_back();segments.pop_back();
	edges.pop_back();edges.pop_back();

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

	auto a = toa->to_;
	auto b = tob->to_;

	// reconnecting edges and therefore removing the vertex

	edge->to_ = b;
	edge_opposite->to_ = a;
	edge->next_ = tob->next_;
	edge_opposite->next_ = toa->next_;

	//setting opposites
	edge->opposite_ = edge_opposite;
	edge_opposite->opposite_ = edge;

	//removig the edges from the back of segments and edges
	//because of the order and recursive algorithm we know they are the last one

	// face changing, importnat!
	edge->face_->edge_ = edge;
	edge_opposite->face_->edge_ = edge_opposite;


	if ((edge->index_ / 100 == number_of_vertices - 1) != (edge->index_ % 100 == number_of_vertices - 1)) {
		edge->face_->counter_of_same_last_edges[min(edge->index_ / 100, edge->index_ % 100)]--;
		edge_opposite->face_->counter_of_same_last_edges[min(edge->index_ / 100, edge->index_ % 100)]--;
	}

	segments.pop_back(); segments.pop_back();
	edges.pop_back(); edges.pop_back();

}

/// <summary>
/// Creating the "dummy" vertex (circle) representing real vertex.
/// </summary>
/// <param name="index"></param>
inline void graph::create_special_vertex(int index) {

	vector<shared_ptr<Vertex> > special_vertices;

	/*create vertices with coordinates*/
	for (int i = 0; i < number_of_vertices - 1;i++) {
		auto new_vertex = make_shared<Vertex>();
		new_vertex->index_ = index; //the real index of vertex
		special_vertices.push_back(new_vertex);
	}

	
	/* edges created */
	for (int i = 0; i < number_of_vertices - 1;i++) {
		auto first_vertex = special_vertices[i];
		auto second_vertex = special_vertices[(i + 1) % (number_of_vertices - 1)];
		edges.push_back(Edge(nullptr, nullptr, nullptr, first_vertex, second_vertex, outer_face, 100*index + index));
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
		add_edge(segments[starts[0][i]]->from_, segments[starts[i][0]]->from_, outer_face, 0, i, true); //from vertex is that in rotation
	}
}

/// <summary>
/// Function to create all "dummy" vertices
/// </summary>
inline void graph::create_all_special_vertices() {

	for (int i = 0; i < number_of_vertices;i++) { //the rest of a star
		create_special_vertex(i);
	}
}

/// <summary>
/// Main algorithm described in psedocode in thesis.
/// It recursively tries all the possible ways
/// how to pull the edges.
/// </summary>
/// <param name="s_index"></param>
/// <param name="t_index"></param>
/// <param name="a"></param>
/// <param name="b"></param>
inline void graph::find_the_way_to_intersect(int s_index, int t_index, int a, int b) {
	
	//cout << a << " " << b << endl;
	auto seg = segments[s_index]->next_;
	
	//print_graph(this);

	//cout << "" << endl;

	while (seg != segments[s_index]) { //the first doesnt have to be considered because it is either beggining segment so it cannot be intersected or it has been already intersected

		if (seg == segments[t_index]) {

			add_edge(segments[s_index]->from_, segments[t_index]->from_, segments[s_index]->face_, a, b);

			
			if (b < number_of_vertices - 2) { //the last vertex is out due to heuristics
				find_the_way_to_intersect(starts[a][b + 1], starts[b + 1][a], a, b + 1);

				if (done) return;

			}
			else if ((b == number_of_vertices - 2) && (a < number_of_vertices - 3)) {
				find_the_way_to_intersect(starts[a + 1][a + 2], starts[a + 2][a + 1], a + 1, a + 2);

				if (done) return;
			}
			else if ((b == number_of_vertices - 2) && (a == number_of_vertices - 3)) {
				find_the_way_to_intersect(starts[1][b + 1], starts[b + 1][1], 1, b + 1);

				if (done) return;
			}
			else if ((b == number_of_vertices - 1) && (a < number_of_vertices - 2)) {
				find_the_way_to_intersect(starts[a + 1][b], starts[b][a + 1], a + 1, b);

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
			add_edge(segments[s_index]->from_, segments[edges.size() - 1]->from_, segments[s_index]->face_, a, b);



			/*important! changing to_ to the opposite direction */
			segments[edges.size() - 3]->from_->to_ = segments[edges.size() - 3]->prev_;

			if(max(a, b) != number_of_vertices - 1 || !is_some_of_faces_incorrect(segments[edges.size() - 2])){
				//try to go further
				//cout << "#############IN####################" << endl;
				find_the_way_to_intersect(edges.size() - 3, t_index, a, b); //it is 3rd from the end, because it was added as second in add_vertex and then 2 more were added in add_edge
				if (done) {
					blocked[min(a, b)][max(a, b)][min(index_first_end, index_second_end)][max(index_first_end, index_second_end)]  = false;
					return;
				}
			}
			//cout << a << " " << b << endl;
		
			//segments[edges.size() - 3]->from_->to_ = segments[edges.size() - 3];

			//undo-intersect //not commutative!
			delete_edge_back();
			delete_vertex((seg->to_).get());
			blocked[min(a, b)][max(a, b)][min(index_first_end, index_second_end)][max(index_first_end, index_second_end)] = false;
		}
		seg = seg->next_;
	}
}


/// <summary>
/// Function to check if current operation of adding edge
/// did not cause violation of one theorems providing us heuristics to fasten the code
/// </summary>
/// <param name="edge"></param>
/// <returns></returns>
inline bool graph::is_some_of_faces_incorrect(Edge* edge){
	auto opposite = edge->opposite_;


	if(is_face_incorrect(edge->face_) || is_face_incorrect(opposite->face_)){
		//cout << "face incorrect" << endl;
		return true;
	}
	return false;
}


/// <summary>
/// Function to check whether the <c>face</c> do not violate the (heuristic) theorems conditions.
/// </summary>
/// <param name="face"></param>
/// <returns></returns>
inline bool graph::is_face_incorrect(shared_ptr<Face> face) {
	int sum = 0;
 	for(int i = 0; i < number_of_vertices - 1;i++){
		if (face->counter_of_same_last_edges[i] >= 2) //treshold by article
			return true;
		//cout << face->counter_of_same_last_edges[i] << " ";

		sum += (face->counter_of_same_last_edges[i] > 0);
	}
	//cout << endl;
	if(sum >= 3)
		return true;
	return false;
}


inline long long factorial(int n) {
	return fact[n];
}

/// <summary>
// This struct is my own generator and iterator for all fingerprints
/// </summary>


/// <summary>
/// Structure for generating all fingerprints iteratively.
/// </summary>
struct fingerprints {
	vector<int> states;
	long long treshold;
	bool done = false;
	vector<string> fingerprint;

	ifstream input_file;
	/// <summary> 
	// Set up the treshold (number of permutation of given string), then generate first rotation systems and reset the states to zeroes
	/// </summary>
	fingerprints(int n, int index) {
		treshold = n; //its length is n-1 and -1 because 0 is on fixed position

		string string_index = index < 10 ? "0" + to_string(index) : to_string(index);
		auto input_path = "data/graph"
			+ to_string(n - 1) + "_" + string_index + ".txt";

		//cout << "input " << input_path << endl;
		input_file.open(input_path);

		cout << "opened also " << input_path << endl;

		string rotation_system;
		if (!getline(input_file, rotation_system)) {
			done = true;
		}
		
		for (int i = 0; i < n - 1;i++) {
			fingerprint.push_back(rotation_system.substr(i * (n - 2), (n - 2)) + to_string(n - 1));
		}

		string last_rotation;
		for (int i = 0; i < n - 1;i++)
			last_rotation += (i + '0');

		fingerprint.push_back(last_rotation);

		for (int i = 0; i < n;i++) {
			states.push_back(0);
		}

		
	}

	///<summary>
	// Get new fingerprint and then move to the next one until it is possible.
	///</summary>
	string get_next() {
		string res;
		for_each(fingerprint.begin(), fingerprint.end(), [&](const string& part) {res += part;});

		for (long long i = states.size() - 1; i >= 0;i--) {
			if (i == states.size() - 1) {
				if (states[i] == factorial(treshold - 2) - 1) { // -2 because first position is fixed for "0"
					states[i] = 0;
					next_permutation(fingerprint[i].begin() + 1, fingerprint[i].end());
				}
				else {
					states[i]++;
					next_permutation(fingerprint[i].begin() + 1, fingerprint[i].end());
					break;
				}
			}
			else {
				if (states[i] == (treshold - 2) - 1) {
					if (i == 0) {
						string rotation_system;
						if (!getline(input_file, rotation_system)) {
							done = true;
							return res;
						}

						fingerprint.clear();
						for (int i = 0; i < treshold - 1;i++) {
							fingerprint.push_back(rotation_system.substr(i * (treshold - 2), (treshold - 2)) + to_string(treshold - 1));
						}

						string last_rotation;
						for (int i = 0; i < treshold - 1;i++)
							last_rotation += (i + '0');

						fingerprint.push_back(last_rotation);

						states.clear();
						for (int i = 0; i < treshold;i++) {
							states.push_back(0);
						}
					}
					else {
						states[i] = 0;
						fingerprint[i] = fingerprint[i][0] + fingerprint[i].substr(2) + fingerprint[i][1];
					}
				}
				else {
					swap(fingerprint[i][(treshold - 2) - states[i]], fingerprint[i][(treshold - 2) - (states[i] + 1)]);
					states[i]++;
					break;
				}
			}
		}

		return res;
	}
};


/// <summary>
/// Function to try all possible drawings, 
/// First getting all the fingerprints 
/// then checking if all K4's are alright
/// followed by finding canonic fingerprint
/// and checking whether it has not been seen.
/// Finally try all the pullings of edges
/// until end or some realization.
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

		//check the fingerprint 
		if (!is_correct_fingerprint(fingerprint)) continue;


		//checking labeling
		fingerprint = find_canonic_fingerprint(fingerprint);


		{
			lock_guard<mutex> canonic_lock(canonic_fingerprints_wraper->shared_mutex);
			if (canonic_fingerprints_wraper->canonic_fingerprints[fingerprint]) continue;
			canonic_fingerprints_wraper->canonic_fingerprints[fingerprint] = true;
		}

		//cout << "after lock guard" << endl;

		create_all_special_vertices();
		recolor_fingerprint(fingerprint);
		create_base_star();

		//cout << fingerprint << endl;

		find_the_way_to_intersect(starts[1][2], starts[2][1], 1, 2);

		//print_graph(this);
		if (done) {
			cout << "yes" << endl;
			output_file << fingerprint << "\n";
		}

		edges.resize(0); segments.resize(0);
		done = false;
		outer_face = make_shared<Face>();
	}

	close_files();

	cout << "realized " << realized << endl;
}

/// <summary>
/// Structure to go through only needed fingerprints.
/// First try all rellabilings of rotation producing "12345" and then all producing "15432".
/// Then move the another vertex.
/// 
/// This is equivalent to first try all posible relabeling to get "12345" 
/// and then the same for inverse. 
/// Altough we want inverse rotation,
/// in <c>find_canonic_fingerprint</c> we try inverse, therefore from relabeling
/// "15432" we get also "12345". 
///
/// </summary>
struct smart_permutations {

	string fingerprint;
	int number_of_vertices;
	bool done = false;
	string first_rotation;
	string result;
	bool invers = false;

	int counter = 0;
	int counter_rotation = 0;

	string create_permutation(const string& rotation1, const string& roration2) {

		string permutation = "";
		for (int i = 0; i < number_of_vertices;i++)
			permutation += (i + '0');

		int sum_r1 = ((number_of_vertices - 1) * (number_of_vertices))/ 2;
		int sum_r2 = sum_r1;

		for (int i = 0; i < rotation1.size();i++) {
			permutation[rotation1[i] - '0'] = roration2[i];
			sum_r1 -= (rotation1[i] - '0');
			sum_r2 -= (roration2[i] - '0');
		}
		
		permutation[sum_r1] = sum_r2 + '0'; //last
		return permutation;
	}

	smart_permutations(const string& fingerprint, int number_of_vertices) {
		this->fingerprint = fingerprint;
		this->number_of_vertices = number_of_vertices;

		for (int i = 1; i < number_of_vertices;i++)
			first_rotation += (i + '0');

	}

	string next() {

		result = create_permutation(fingerprint.substr((number_of_vertices - 1) * counter, number_of_vertices - 1), first_rotation);
		//cout << result << ":result " << endl;

		if (invers && counter == number_of_vertices - 1 && counter_rotation == number_of_vertices - 2) {
			done = true;
			return result;
		}

		if (invers && counter_rotation == number_of_vertices - 2) {
			invers = false;
			counter++;
			counter_rotation = 0;

			first_rotation = "";
			for (int i = 1; i < number_of_vertices;i++)
				first_rotation += (i + '0');
		}
		else if (counter_rotation == number_of_vertices - 2) {
			counter_rotation = 0;
			invers = true;

			first_rotation = "1";
			for (int i = 1; i < number_of_vertices - 1;i++)
				first_rotation += ((number_of_vertices - i) + '0');

			//rotate(first_rotation.begin(), first_rotation.begin() + 1, first_rotation.end());
		}
		else {
			counter_rotation++;
			rotate(first_rotation.begin(), first_rotation.begin() + 1, first_rotation.end());
		}

		return result;
	}
};

/// <summary>
/// Function to find canonic fingerprint through all needed rellabelings of itself (and inversion)
/// </summary>
/// <param name="fingerprint"></param>
/// <returns></returns>
inline string graph::find_canonic_fingerprint(const string& fingerprint) {

	auto permutations = smart_permutations(fingerprint, number_of_vertices);

	auto min_fingerprint = fingerprint;
	auto cur_fingerprint = fingerprint;
	auto new_fingerprint = fingerprint;

	do {
		auto permutation_holder = permutations.next();

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

		for (int i = 0; i < number_of_vertices;i++) {
			auto inv_part = new_fingerprint.substr(i * (number_of_vertices - 1) + 1, number_of_vertices - 2);
			reverse(inv_part.begin(), inv_part.end());
			new_fingerprint.replace(i * (number_of_vertices - 1) + 1, number_of_vertices - 2, inv_part);
		}

		min_fingerprint = min(min_fingerprint, new_fingerprint);

	} while (!permutations.done);

	//cout << minimal_permutation << endl;
	return min_fingerprint;

}


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

/// <summary>
/// Check if given rotations of K4 form a realizable K4.
/// </summary>
/// <param name="orders"></param>
/// <returns></returns>
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

/// <summary>
/// Function to check whether the <c>fingerprint</c> consists only of realizable K4's.
/// </summary>
/// <param name="fingerprint"></param>
/// <returns></returns>
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

					if (!is_correct_K4(order)) return false;
				}
			}
		}
	}

	return true;
}
