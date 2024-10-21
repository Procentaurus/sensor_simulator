import json
import time

from sender import Sender
from generator import Generator

class Sensor(Sender, Generator):

    def __init__(self, identification, max_val, min_val, unit, init_val, step,
                 max_steps_change, sec_interval):
        Generator.__init__(self, max_val, min_val, step, max_steps_change)
        Sender.__init__(self)
        self.__id = identification
        self.__last_val = init_val
        self.__unit = unit
        self.__sec_interval = sec_interval

    def create_signal(self):
        s = {'id': self.__id, 'value': self.__last_val, 'unit': self.__unit}
        return json.dumps(s)

    def update_last_val(self, new_val):
        self.__last_val = new_val

    def start_transmitting(self):
        self.connect_to_queue()
        print(f'Sensor {self.__id} starts transmitting')
        while True:
            time.sleep(self.__sec_interval)
            new_val = self.get_measurement(self.__last_val)
            self.update_last_val(new_val)
            self.send(self.create_signal())
