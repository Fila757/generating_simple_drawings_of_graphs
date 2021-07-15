#!/usr/bin/env python3
import argparse
import datetime
import os
import re
os.environ.setdefault("TF_CPP_MIN_LOG_LEVEL", "2") # Report only TF errors by default

#34137cb5-a86e-11e7-a937-00505601122b 06bc9bb2-ef66-11e9-9ce9-00505601122b

import numpy as np
import tensorflow as tf
from fingerprint_dataset import Dataset
from metrics import RoundedAccuracy


parser = argparse.ArgumentParser()
parser.add_argument("--batch_size", default=32, type=int, help="Batch size.")
parser.add_argument("--epochs", default=100, type=int, help="Number of epochs.")
parser.add_argument("--seed", default=42, type=int, help="Random seed.")
parser.add_argument("--threads", default=4, type=int, help="Maximum number of threads to use.")
parser.add_argument("--size", default=7,type=int, help="Size of a graph.")

def main(args):
    # Fix random seeds and threads
    np.random.seed(args.seed)
    tf.random.set_seed(args.seed)
    tf.config.threading.set_inter_op_parallelism_threads(args.threads)
    tf.config.threading.set_intra_op_parallelism_threads(args.threads)

    size = args.size
    ### Getting dataset
    dataset = Dataset(args, size)
    train, dev, test = dataset.get_dataset('train'), dataset.get_dataset('dev'), dataset.get_dataset('test')

    ### Creating model
    
    inputs = tf.keras.layers.Input(shape=[size*(size-1), 1], dtype=tf.float32)

    #print(tf.shape(inputs), 'inputs')
    conv1d = tf.keras.layers.Conv1D(filters=4, kernel_size=5, strides=2, padding='same', use_bias=False, input_shape=[size*(size-1), size])(inputs)
    batch_norm = tf.keras.layers.BatchNormalization()(conv1d)
    activation = tf.keras.layers.Activation(tf.nn.relu)(batch_norm)

    conv1d = tf.keras.layers.Conv1D(filters=16, kernel_size=3, strides=2, padding='same', use_bias=False, input_shape=[size*(size-1), size])(activation)
    batch_norm = tf.keras.layers.BatchNormalization()(conv1d)
    activation = tf.keras.layers.Activation(tf.nn.relu)(batch_norm)
    
    lstm = tf.keras.layers.Bidirectional(tf.keras.layers.LSTM(64, return_sequences=True), merge_mode='sum')(activation)
    lstm = tf.keras.layers.Bidirectional(tf.keras.layers.LSTM(64, return_sequences=True), merge_mode='sum')(lstm)

    flatten = tf.keras.layers.Flatten()(lstm)
    predictions = tf.keras.layers.Dense(1, activation=tf.nn.relu)(flatten)

    ### Training

    model = tf.keras.Model(inputs, predictions)
    model.compile(
      optimizer=tf.keras.optimizers.Adam(
        learning_rate=tf.keras.optimizers.schedules.CosineDecay(
          initial_learning_rate=0.002,
          decay_steps=args.epochs * (dataset.datalen // args.batch_size),
          alpha=0.000001)
          ),
      loss = tf.keras.losses.MeanSquaredError(),
      metrics=[RoundedAccuracy()],
    )
    model.fit(train, epochs=args.epochs,validation_data=dev)


    ### Prediction

    predictions = model.predict(test)
    racc = RoundedAccuracy()
    racc.update_state(dataset.get_intersections('test'), predictions)
    print(f'test_acc: {racc.result().numpy()}')

    ### Writing to file

    with open('pred_intersections.txt', 'w') as file:
      for batch in predictions:
        for b in batch:
          file.write(str(round(b)) + '\n')



if __name__ == '__main__':
    args = parser.parse_args([] if "__file__" not in globals() else None)
    main(args)



    