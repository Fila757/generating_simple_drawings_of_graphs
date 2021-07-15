import tensorflow as tf
import numpy as np


class RoundedAccuracy(tf.keras.metrics.Metric):

  def __init__(self, name='rounded_accuracy', **kwargs):
    super(RoundedAccuracy, self).__init__(name=name, **kwargs)
    self._acc = tf.keras.metrics.Accuracy()

  def update_state(self, y_true, y_pred, sample_weight=None):
      y_pred_rounded = tf.math.round(y_pred)
      print(y_pred_rounded)
      self._acc(y_true, y_pred_rounded, sample_weight=sample_weight)

  def reset_state(self):
      self._acc.reset_state()

  def result(self):
    return self._acc.result()