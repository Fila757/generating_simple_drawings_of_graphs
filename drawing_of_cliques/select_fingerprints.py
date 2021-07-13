#!/usr/bin/env python3

import sys

with open(sys.argv[1], 'r') as file:
    lines = file.readlines()

with open(sys.argv[1], 'w') as file:
    for line in lines:
        file.write(line.split()[0] + '\n')


