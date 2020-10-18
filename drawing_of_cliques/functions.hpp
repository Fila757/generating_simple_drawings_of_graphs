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


		auto output_path = "C:/Users/filip/source/repos/generating-simple-drawings-of-graphs/drawing_of_cliques/data/graph"
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
	unordered_map<string, bool> canonic_fingerprints;

	void create_special_vertex(int index);
	void recolor_fingerprint(const string& rotation);
	void create_base_star();
	void create_all_special_vertices();
	void find_the_way_to_intersect(int s_index, int t_index, int a, int b);
	//void intersect();
	bool is_correct_fingerprint(const string& fingerprint);
	void create_all_possible_drawings();

	string find_canonic_fingerprint(const string& fingerprint);

};

struct Vertex {
	Edge* to_;

	int index_;

	Vertex() {}

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
			cur_edge = cur_edge->next_;
		}
	}
}
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

}

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
	segments.pop_back(); segments.pop_back();
	edges.pop_back(); edges.pop_back();

}

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

inline void graph::recolor_fingerprint(const string& fingerprint) { //fingerprint does include the first ro (0..n), otherwise do the first rotation manually

	for(int i = 0; i < number_of_vertices;i++){
		for (int j = 0; j < number_of_vertices - 1;j++) { //every vertex has n-1 around itself
			starts[i][fingerprint[i * (number_of_vertices - 1) + j] - '0'] = i * (number_of_vertices - 1) + j;
		}
	}
}

inline void graph::create_base_star() {
	for (int i = 1; i < number_of_vertices;i++) {
		add_edge(segments[starts[0][i]]->from_, segments[starts[i][0]]->from_, outer_face, 0, i, true); //from vertex is that in rotation
	}
}

inline void graph::create_all_special_vertices() {

	for (int i = 0; i < number_of_vertices;i++) { //the rest of a star
		create_special_vertex(i);
	}
}

inline void graph::find_the_way_to_intersect(int s_index, int t_index, int a, int b) {
	
	auto seg = segments[s_index]->next_;
	
	//print_graph(this);

	//cout << "" << endl;

	while (seg != segments[s_index]) { //the first doesnt have to be considered because it is either beggining segment so it cannot be intersected or it has been already intersected

		if (seg == segments[t_index]) {

			add_edge(segments[s_index]->from_, segments[t_index]->from_, segments[s_index]->face_, a, b);

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
			add_edge(segments[s_index]->from_, segments[edges.size() - 1]->from_, segments[s_index]->face_, a, b);

			/*important! changing to_ to the opposite direction */
			segments[edges.size() - 3]->from_->to_ = segments[edges.size() - 3]->prev_; 

			//try to go further
			find_the_way_to_intersect(edges.size() - 3, t_index, a, b); //it is 3rd from the end, because it was added as second in add_vertex and then 2 more were added in add_edge
			if (done) {
				blocked[min(a, b)][max(a, b)][min(index_first_end, index_second_end)][max(index_first_end, index_second_end)]  = false;
				return;
			}

			//segments[edges.size() - 3]->from_->to_ = segments[edges.size() - 3];

			//undo-intersect //not commutative!
			delete_edge_back();
			delete_vertex((seg->to_).get());
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
	vector<int> states;
	long long treshold;
	bool done = false;
	vector<string> fingerprint;

	ifstream input_file;
	/// <summary> 
	// Set up the treshold (number of permutation of given string), then generate first rotation systems and reset the states to zeroes
	/// </summary>
	fingerprints(int n) {
		treshold = n; //its length is n-1 and -1 because 0 is on fixed position

		auto input_path = "C:/Users/filip/source/repos/generating-simple-drawings-of-graphs/drawing_of_cliques/data/graph"
			+ to_string(n - 1) + ".txt";
		input_file.open(input_path);

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
	// Return state and then move to the next one until it is posible
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

	int yes_counter = 0;

	auto generator_of_fingerprints = fingerprints(number_of_vertices);
	while (!generator_of_fingerprints.done) {
		auto fingerprint = generator_of_fingerprints.get_next();
		//cout << cur << endl;

		//check the fingerprint 
		if (!is_correct_fingerprint(fingerprint)) continue;

		//checking labeling
		fingerprint = find_canonic_fingerprint(fingerprint);
		if (canonic_fingerprints[fingerprint]) continue;
		else {
			cout << "heureka" << endl;
		}
		canonic_fingerprints[fingerprint] = true;

		create_all_special_vertices();
		recolor_fingerprint(fingerprint);
		create_base_star();

		//cout << fingerprint << endl;

		find_the_way_to_intersect(starts[1][2], starts[2][1], 1, 2);

		if (done) {
			if (yes_counter == 92) {
				cout << "stop" << endl;
			}
			cout << "yes" << endl;
			output_file << fingerprint << "\n";
			yes_counter++;
		}

		edges.resize(0); segments.resize(0);
		done = false;
	}

	close_files();

	cout << "realized " << realized << endl;
}

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
		if (counter == number_of_vertices - 1 && counter_rotation == number_of_vertices - 2) {
			counter_rotation = 0;
			counter = 0;
			invers = true;

			first_rotation = "1";
			for (int i = 1; i < number_of_vertices - 1;i++)
				first_rotation += ((number_of_vertices - i) + '0');
		}

		if (counter_rotation == number_of_vertices - 2) {
			counter_rotation = 0;
			counter++;

			rotate(first_rotation.begin(), first_rotation.begin() + 1, first_rotation.end());
		}
		else {
			counter_rotation++;
			rotate(first_rotation.begin(), first_rotation.begin() + 1, first_rotation.end());
		}

		return result;
	}
};

inline string graph::find_canonic_fingerprint(const string& fingerprint) {

	//string permutation_holder;
	//for (int i = 0; i < number_of_vertices;i++) 
	//	permutation_holder += (i + '0');

	auto permutations = smart_permutations(fingerprint, number_of_vertices);

	auto min_fingerprint = fingerprint;
	auto cur_fingerprint = fingerprint;
	auto new_fingerprint = fingerprint;

	//string minimal_permutation;

	do { //can be changes just to while because first doesnt any change
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
		//if (min_fingerprint == new_fingerprint) minimal_permutation = permutation_holder;

		for (int i = 0; i < number_of_vertices;i++) {
			auto inv_part = new_fingerprint.substr(i * (number_of_vertices - 1) + 1, number_of_vertices - 2);
			reverse(inv_part.begin(), inv_part.end());
			new_fingerprint.replace(i * (number_of_vertices - 1) + 1, number_of_vertices - 2, inv_part);
		}

		min_fingerprint = min(min_fingerprint, new_fingerprint);
		//if (min_fingerprint == new_fingerprint) minimal_permutation = permutation_holder;

	} while (!permutations.done);

	//cout << minimal_permutation << endl;
	return min_fingerprint;

}


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
