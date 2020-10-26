#include "delaunator.hpp"
#include <cstdio>

using namespace std;

int main() {
    /* x0, y0, x1, y1, ... */
    std::vector<double> coords = {-10, 10, 10, 10, 10, -10, -10, -10, 0, 0, 
    5, 0, 0, 5, -5, 0, 0, -5};

    //triangulation happens here
    delaunator::Delaunator d(coords);

    for(int i = 0; i < d.triangles.size();i++){
        cout << d.triangles[i] << endl;
    }

    for(std::size_t i = 0; i < d.triangles.size(); i+=3) {
        printf(
            "Triangle points: [[%f, %f], [%f, %f], [%f, %f]]\n",
            d.coords[2 * d.triangles[i]],        //tx0
            d.coords[2 * d.triangles[i] + 1],    //ty0
            d.coords[2 * d.triangles[i + 1]],    //tx1
            d.coords[2 * d.triangles[i + 1] + 1],//ty1
            d.coords[2 * d.triangles[i + 2]],    //tx2
            d.coords[2 * d.triangles[i + 2] + 1] //ty2
        );
    }
}
