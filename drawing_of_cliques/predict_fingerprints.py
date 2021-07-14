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


parser = argparse.ArgumentParser()
parser.add_argument("--batch_size", default=16, type=int, help="Batch size.")
parser.add_argument("--epochs", default=5, type=int, help="Number of epochs.")
parser.add_argument("--seed", default=42, type=int, help="Random seed.")
parser.add_argument("--threads", default=1, type=int, help="Maximum number of threads to use.")

def main(args):
    # Fix random seeds and threads
    np.random.seed(args.seed)
    tf.random.set_seed(args.seed)
    tf.config.threading.set_inter_op_parallelism_threads(args.threads)
    tf.config.threading.set_intra_op_parallelism_threads(args.threads)


    ### Getting dataset
    dataset = Dataset(args, 7)
    train, dev, test = dataset.get_dataset('train'), dataset.get_dataset('dev'), dataset.get_dataset('test')

    ### Creating model
    
    inputs = tf.keras.layers.Input(shape=[None], dtype=tf.int32)

    conv1d = tf.keras.layers.Conv1D(64, 3, strides=2, padding='same', use_bias=False)(inputs)
    batch_norm = tf.keras.layers.Batch
    
    lstm = tf.keras.layers.Bidirectional(tf.keras.layers.LSTM(64, return_sequences=True), merge_mode='sum')(conv1d)

    predictions = tf.keras.layers.TimeDistributed(tf.keras.layers.Dense(1, activation=tf.nn.relu))(lstm)

    ### Training

    model = tf.keras.Model(inputs, new_outputs)
    model.compile(
      optimizer=tf.keras.optimizers.Adam(),
      loss = tf.keras.losses.MeanSquaredError(),
      metrics=[tf.keras.metrics.MeanSquaredError()],
    )
    model.fit(train, epochs=args.epochs,validation_data=dev)

    predictions = model.predict(test)

    #print(tf.keras.metrics.mean_squared_error(test, predictions))






if __name__ == '__main__':
    args = parser.parse_args([] if "__file__" not in globals() else None)
    main(args)



    