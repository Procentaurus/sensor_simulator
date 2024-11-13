Simple simulator of WW2 plane's on-board gear sensors. All sensors are generating randomly changing values and sending them to the configured RabbitMQ queues.

To run:
- `docker-compose up -d rabbitmq mongodb onboard-system`
- wait a moment
- `docker-compose up -d generator`

## How to connect with OnboardSystem

### WebSocket

To receive live update connect with WebSocket on port 5109 e.g. `ws://localhost:5109/ws`

### HTTP API

To receive all records send GET request on port 5109 e.g.: 

- `http://localhost:5109/api/sensors` (JSON format)
- `http://localhost:5109/api/sensors/csv` (CSV file)

To receive filtered or sorted data send GET request on port 5109 e.g.:

- `http://localhost:5109/api/sensors?orderByTime=true`
- `http://localhost:5109/api/sensors/csv?minValue=0`

Available (optional) arguments:
- sensorIds e.g. `sensorIds=1&sensorIds=2&sensorIds=3`
- timestamp e.g. `timestamp=1&timestamp=2&timestamp=3`
- minValue e.g. `minValue=0`
- maxValue e.g. `maxValue=0`
- orderByTime e.g. `orderByTime=true`
- orderById e.g. `orderById=true`
