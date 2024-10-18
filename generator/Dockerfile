FROM python:3.9-slim

WORKDIR /app

COPY . /app

RUN pip install pika

EXPOSE 5672

CMD ["python", "main.py"]
