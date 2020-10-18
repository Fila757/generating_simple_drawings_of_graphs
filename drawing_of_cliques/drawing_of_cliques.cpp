// drawing_of_cliques.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "functions.hpp"

using namespace std;

/*
string find_lexical_min_rotation(string str)
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
int main()
{
    auto g = graph(6);

    cout << "minimal" << g.find_canonic_fingerprint("123540523401435012450153202314") << endl;

    //cout << "minimal: " << g.find_canonic_fingerprint("12340423014301240321") << endl;
    
    //cout << "minimal: " << g.find_canonic_fingerprint("12340234013401240123") << endl;
    //cout << "minimal: " << g.find_canonic_fingerprint("12340234013401420132") << endl;
    //cout << "minimal: " << g.find_canonic_fingerprint("12340234013404120312") << endl;
    //cout << "minimal: " << g.find_canonic_fingerprint("12340234014301240132") << endl;
    //cout << "minimal: " << g.find_canonic_fingerprint("12340243013401420312") << endl;
    //cout << "minimal: " << g.find_canonic_fingerprint("12340342014302410321") << endl;
 
    //g.create_all_possible_drawings();
    
    // testing relabeling because we have found 6 realizable RS written 

    //string basic_string = "01234";

    // with three intersectiona are not izomorfic

    // with 5 intersection 1 and 3 no, 1 and 2 no, 2 and 3 no

    /*
    do {
        set<string> first_set = { "0213", "0214", "0314", "1423", "0423"};
        set<string> second_set = { "0213", "0214", "1324", "0413", "0423" };

        if (basic_string == "21043") {
            cout << basic_string << endl;
        }

        //cout << first_set.size() << endl;
        //cout << basic_string << endl;
        set<string> tmp;
        for (auto it = first_set.begin(); it != first_set.end();it++) {
            string elem = *it;
            for (int i = 0; i < elem.size();i++) {
                elem[i] = basic_string[elem[i] - '0'];
            }

            string f = elem.substr(0, 2);
            string s = elem.substr(2, 2);

            f = find_lexical_min_rotation(f);
            s = find_lexical_min_rotation(s);

            if (f > s) swap(f, s);
            tmp.insert(f + s);
        }

        first_set = tmp;
        tmp.clear();

        bool bad = false;

        
        cout << "second set" << endl;
        for (auto it = second_set.begin(); it != second_set.end();it++) {
            cout << *it << " ";
        }
        cout << endl;
        
        for (auto it = first_set.begin(); it != first_set.end();it++) {
            cout << *it << endl;
            if (!second_set.count(*it)) {
                bad = true;
                break;
            }
        }

        if (!bad) {
            cout << "HEEEEEEEEUREEKA" << endl;
            cout << basic_string << endl;
            break;
        }

    } while (next_permutation(basic_string.begin(), basic_string.end()));
    */
    //print_graph(&g);
}

