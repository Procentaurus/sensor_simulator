import random


class Generator:

    def __init__(self, max_val, min_val, step, max_steps_change):
        self.__max_val = max_val
        self.__min_val = min_val
        self.__step = step
        self.__max_steps_change = max_steps_change

    def __get_lower_values(self, last_val, k):
        max_val_change = self.__step * self.__max_steps_change
        res = list(range(
            round(k * (max(last_val - max_val_change, self.__min_val))),
            round(k * last_val),
            round(k * self.__step)))
        return [round(x / k, 2) for x in res] if k > 1 else res

    def __get_higher_values(self, last_val, k):
        max_val_change = self.__step * self.__max_steps_change
        res = list(range(
            round(k * (last_val + self.__step)),
            round(k * (min(last_val + max_val_change + self.__step, self.__max_val))),
            round(k * self.__step)))
        return [round(x / k, 2) for x in res] if k > 1 else res

    def get_measurement(self, last_val):
        k = self.calculate_proportion()
        lower_values = self.__get_lower_values(last_val, k)
        higher_values = self.__get_higher_values(last_val, k)
        combined_range = lower_values + [last_val] + higher_values
        lower_weights = self.__create_lower_weights(lower_values)
        higher_weights = self.__create_higher_weights(higher_values)
        combined_weights = lower_weights + [self.__step] + higher_weights
        return random.choices(combined_range, weights=combined_weights, k=1)[0]

    def __create_lower_weights(self, values):
        return [self.__step * (i + 1) for i in range(len(values))]

    def __create_higher_weights(self, values):
        return [self.__step * (i + 1) for i in range(len(values))][::-1]

    def calculate_proportion(self):
        if self.__step < 1:
            return 1 / self.__step
        else: return 1
