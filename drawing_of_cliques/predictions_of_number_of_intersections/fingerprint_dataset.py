
import tensorflow as tf
import numpy as np
import random 

def shuffle_respectively(a, b):
    c = list(zip(a, b))
    random.shuffle(c)
    a, b = zip(*c)
    return a, b

class Dataset:

    def __init__(self, args, size):
        self._size = size
        self._dataset = dict()
        self._args = args

        self.read_fingerprints()
        self.set_datasets()
        
    
    def read_fingerprints(self):
        self._fingerprints, self._intersections = [], []
        with open(f'data/graph{self._size}.txt') as file_in:
            lines = file_in.readlines()[:500]

            for line in lines:
                splitted_line = line.split()
                self._fingerprints.append([[int(c)] for c in splitted_line[0]])
                self._intersections.append(int(splitted_line[1]))

        print(tf.shape(self._fingerprints))
        print(tf.shape(self._intersections))

    def set_datasets(self):
        datalen = len(self._fingerprints)
        self._dataset['train'] = tf.data.Dataset.from_tensor_slices((self._fingerprints[:int(0.8*datalen)], self._intersections[:int(0.8*datalen)])).batch(self._args.batch_size)
        self._dataset['dev'] = tf.data.Dataset.from_tensor_slices((self._fingerprints[int(0.8*datalen):int(0.9*datalen)], self._intersections[int(0.8*datalen):int(0.9*datalen)])).batch(self._args.batch_size)
        self._dataset['test'] = tf.data.Dataset.from_tensor_slices((self._fingerprints[int(0.9*datalen):], self._intersections[int(0.9*datalen):])).batch(self._args.batch_size)

        #print(tf.shape(self._dataset['train']))
    

    def get_dataset(self, name):
        return self._dataset[name]


