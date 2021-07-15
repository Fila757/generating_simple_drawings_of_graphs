#!/usr/bin/env python3

import sys
import math

def z(n):
    return 1/4*math.floor(n/2)*math.floor((n-1)/2)*math.floor((n-2)/2)*math.floor((n-3)/2)

with open(sys.argv[1], 'r') as file:
    lines = file.readlines()


line = lines[0]
n = int(1/2*(math.sqrt(4*len(line.split()[0]) + 1) + 1))
print(n)

with open(f'graph{n}_minimal.txt', 'w') as file:
    for line in lines:
        splitted_line = line.split()
        intersections = int(splitted_line[1])

        if intersections <= z(n):
            if intersections < z(n):
                raise AssertionError(f"There is a drawing with less then z({n}).")
            file.write(line)


