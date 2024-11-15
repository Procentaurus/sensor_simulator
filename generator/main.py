from sensor import Sensor
import threading


def main():
    print('Initializing sensors')
    sensors = [
        # Temperature sensors
        Sensor(1, 120, 70, "°C", 90, 1, 4, 10), # coolant
        Sensor(2, 210, 145, "°F", 160, 1, 6, 30),  # oil
        Sensor(3, 100, 35, "°F", 90, 1, 2, 10),  # fuel
        Sensor(4, 250, 150, "°C", 225, 1, 5, 60),  # cylinder head

        # Pressure sensors
        Sensor(5, 21, 10, "psi", 78, 0.2, 3, 10), # coolant
        Sensor(6, 5, 3, "bar", 160, 0.1, 2, 30),  # oil
        Sensor(7, 0.5, 0.2, "bar", 90, 0.01, 3, 20),  # fuel
        Sensor(8, 25, 14, "psi", 230, 0.1, 8, 15),  # supercharger boost

        # RPM sensors
        Sensor(9, 3000, 1800, "rpm", 2000, 5, 10, 10), # propeller
        Sensor(10, 3000, 2200, "rpm", 2300, 1, 15, 6),  # engine
        Sensor(11, 10000, 5000, "rpm", 6000, 10, 10, 6),  # supercharger
        Sensor(12, 2500, 1500, "rpm", 2400, 5, 3, 10),  # gearbox

        # Flow Rate sensors
        Sensor(13, 5, 0.5, "GPH", 1, 0.1, 3, 4), # fuel
        Sensor(14, 3, 0.5, "GPH", 1.5, 0.1, 2, 20),  # oil
        Sensor(15, 10, 0.1, "GPH", 5, 0.1, 5, 60),  # cooling system
        Sensor(16, 8, 0.25, "LPH", 6, 0.05, 8, 10)  # hydraulic flow
    ]
    print('Sensors initialized')

    for sensor in sensors:
        thread = threading.Thread(target=sensor.start_transmitting)
        thread.start()

if __name__ == '__main__':
    main()
