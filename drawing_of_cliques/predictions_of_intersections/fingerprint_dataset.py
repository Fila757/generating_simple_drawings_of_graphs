
import tensorflow as tf
import numpy as np
import random 

def shuffle_respectively(a, b):
    c = list(zip(a, b))
    random.shuffle(c)
    a, b = zip(*c)
    return list(a), list(b)

class Dataset:

    _train_constant = 0.8 # end of train
    _dev_constant = 0.9 # end of dev

    def __init__(self, args, size):
        self._size = size
        self._dataset = dict()
        self._args = args

        self.read_fingerprints()
        self.set_datasets()
        
    
    def read_fingerprints(self):
        self._fingerprints, self._intersections = [], []
        with open(f'../data/graph{self._size}.txt') as file_in:
            lines = file_in.readlines()

            for line in lines:
                splitted_line = line.split()
                self._fingerprints.append([[int(c)] for c in splitted_line[0]])
                self._intersections.append(int(splitted_line[1]))
            
        self._fingerprints, self._intersections = shuffle_respectively(self._fingerprints, self._intersections)

    def set_datasets(self):
        datalen = len(self._fingerprints)
        self._datalen = datalen
        self._dataset['train'] = tf.data.Dataset.from_tensor_slices((self._fingerprints[:int(self._train_constant*datalen)], self._intersections[:int(self._train_constant*datalen)])).batch(self._args.batch_size)
        self._dataset['dev'] = tf.data.Dataset.from_tensor_slices((self._fingerprints[int(self._train_constant*datalen):int(self._dev_constant*datalen)], self._intersections[int(self._train_constant*datalen):int(self._dev_constant*datalen)])).batch(self._args.batch_size)
        self._dataset['test'] = tf.data.Dataset.from_tensor_slices((self._fingerprints[int(self._dev_constant*datalen):], self._intersections[int(self._dev_constant*datalen):])).batch(self._args.batch_size)
        
        self.print_test()
        #print(tf.shape(self._dataset['train']))
    

    def print_test(self):
        with open('true_intersections.txt', 'w') as file:
            for i in iter(self.get_intersections('test')):
                file.write(str(i) + '\n')



    def get_dataset(self, name):
        return self._dataset[name]

    def get_intersections(self, name):
        datalen = len(self._fingerprints)
        if name == 'train':
            return self._intersections[:int(self._train_constant*datalen)]
        elif name == 'dev':
            return self._intersections[int(self._train_constant*datalen):int(self._dev_constant*datalen)]
        else:
            return self._intersections[int(self._dev_constant*datalen):]

    @property
    def datalen(self):
        return self._datalen * self._train_constant
