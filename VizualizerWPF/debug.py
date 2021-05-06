import numpy as np
import matplotlib.pyplot as plt

import itertools
data = np.genfromtxt('C:/Users/filip/Desktop/debug.txt',delimiter=',')

def collinear(x1, y1, x2, y2, x3, y3):
     
    """ Calculation the area of 
        triangle. We have skipped
        multiplication with 0.5 to
        avoid floating point computations """
    a = x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)
 
    b = (y2 - y1) / (x2 - x1)
    c = (y3 - y2) / (x3 - x2)


    if (abs(a) < 0.000000001):
        print(b, c)
        return True
    else:
        return False
 
data[:, 1] = -data[:, 1]
#print(itertools.combinations(range(len(data)), 3))
for i,j,k in itertools.combinations(range(len(data)), 3):
	if collinear(data[i, 0], data[i, 1], data[j, 0], data[j, 1], data[k, 0], data[k, 1]):
		print('YES', i, j, k)
		print(data[i, 0], data[i, 1], data[j, 0], data[j, 1], data[k, 0], data[k, 1])
print(data)
plt.plot(data[:,0], data[:, 1]) 
plt.show()