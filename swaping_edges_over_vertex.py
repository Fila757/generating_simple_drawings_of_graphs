import sys
import numpy as np

n = 4

#matrix_sum = np.ones(((n*(n-1))/2, n)) * (n-2) #e=uv, w #rectilinear

matrix = np.zeros((int((n*(n-1))/2), n, n)) 

my_dict = dict()
indexer = 0
for i in range(n):
	for j in range(i + 1, n):
		my_dict[(i, j)] = indexer
		my_dict[(j, i)] = indexer
		indexer += 1

#print(my_dict)

def init(): #rectilinear
	for i in range(n):
		for j in range(i + 1, n):
			for k in range(n):
				for l in range(len(matrix[my_dict[(i, j)], k])):
					if i == k or j == k:
						continue
					else:
						if l == i or l == j or l == k:
							matrix[my_dict[(i, j)], k, l] = -1
						else:
							if not (j - i == 1 or j - i == n - 1):
								matrix[my_dict[(i, j)], k, l] = 1
							

def reverse(row):
	for i in range(len(row)):
		if row[i] == 0:
			row[i] = 1
		elif row[i] == 1:
			row[i] = 0


def cr(matrix):
	return np.sum(matrix) / 4 #every crossing counted for times

def swap_edge_over_vertex(u, v, chosen):
	left, right = [], []
	for i in range(len(matrix[my_dict[(u, v)], chosen])):
		if matrix[my_dict[(u, v)], chosen, i] == 1:
			left.append(i)
		elif matrix[my_dict[(u, v)], chosen, i] == 0:
			right.append(i)

	reverse(matrix[my_dict[(u, v)], chosen])
	for l in left:
		matrix[my_dict[(u, v)], l, chosen] = 0
	for r in right:
		matrix[my_dict[(u, v)], r, chosen] = 1

	for l in left:
		matrix[my_dict[(l, chosen)], u, v] = 0
		matrix[my_dict[(l, chosen)], v, u] = 0
	for r in right:
		matrix[my_dict[(r, chosen)], u, v] = 1
		matrix[my_dict[(r, chosen)], v, u] = 1

def get_cr(matrix):
	matrix_sum = np.copy(matrix)
	matrix_sum[matrix_sum == -1] = 0
	return cr(matrix_sum)

init()
print(matrix, get_cr(matrix))
while(True):
	u, v, chosen = [int(a) for a in input().split()]
	swap_edge_over_vertex(u,v, chosen)
	print(matrix, get_cr(matrix))
















